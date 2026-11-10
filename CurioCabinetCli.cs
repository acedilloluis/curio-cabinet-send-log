#:package System.CommandLine@2.0.14

using System;
using System.Linq;
using System.IO;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;

internal partial class Program
{
    static int Main(string[] args)
    {
	//options
	Option<bool> verboseOption = new Option<bool>("--verbose")
	{
	    Description = "Show detailed output",
	    Recursive = true
	};

	//options for create command
	Option<DateOnly> dateOption = new Option<DateOnly>("--date", "-d")
	{
	    Description = "Date climb sent or attempted (uses current culture format)",
	    DefaultValueFactory = _ => DateOnly.FromDateTime(DateTime.Now)
	};

	Option<bool> sentOption = new Option<bool>("--sent", "-s")
	{
	    Description = "Climb sent",
	    DefaultValueFactory = _ => true
	};

	Option<int> attemptOption = new Option<int>("--attempted", "-a")
	{
	    Description = "Climb attempted x times",
	    DefaultValueFactory = _ => 1
	};

	Option<string> noteOption = new Option<string>("--note", "-n")
	{
	    Description = "Enter a short note about the climb",
	    DefaultValueFactory = _ => "There's nothing here."
	};

	//options for edit command
	Option<string> editTitleOption = new Option<string>("--edtitle", "-edt")
	{
	    Description = "Edit the title of a climb"
	};

	Option<string> editGradeOption = new Option<string>("--edgrade", "-edg")
	{
	    Description = "Edit the grade of a climb"
	};

	Option<DateOnly?> editDateOption = new Option<DateOnly?>("--eddate", "-edd")
	{
	    Description = "Edit the date of a climb"
	};

	Option<string> editNoteOption = new Option<string>("--ednote", "-edn")
	{
	    Description = "Edit the note of a climb"
	};

	Option<bool?> editSentOption = new Option<bool?>("--edsent", "-eds")
	{
	    Description = "Edit whether the climb was sent or not"
	};

	Option<int?> editAttemptsOption = new Option<int?>("--edattempts", "-eda")
	{
	    Description = "Change the number of attempts"
	};

	//options for list command
	Option<DateOnly?> filterByDateOption = new Option<DateOnly?>("--filterdate", "-fd")
	{
	    Description = "Filter log by date"
	};

	Option<string> filterByGradeOption = new Option<string>("--filtergrade", "-fg")
	{
	    Description = "Filter log by grade"
	};

	Option<bool?> filterBySentOption = new Option<bool?>("--filtersent", "-fs")
	{
	    Description = "Filter log by whether climb was sent or not"
	};

	//arguments
	Argument<string> titleArgument = new Argument<string>("title")
	{
	    Description = "Name of climb"
	};

	Argument<string> gradeArgument = new Argument<string>("grade")
	{
	    Description = "Grade of climb"
	};

	//commands
	Command createCommand = new Command("create", "Add a new a entry to the log")
	{
	    Arguments = { titleArgument, gradeArgument },
	    Options = { dateOption, sentOption, attemptOption, noteOption }
	};

	Command listCommand = new Command("list", "List all climbs in log")
	{
	    Options = { filterByDateOption, filterBySentOption, filterByGradeOption }
	};

	Command removeCommand = new Command("remove", "Remove a climb from the log")
	{
	    Arguments = { titleArgument }
	};

	Command addAttemptCommand = new Command("addAttempt", "Add one attempt to a climb in the log")
	{
	    Arguments = { titleArgument }
	};

	Command editCommand = new Command("edit", "Edit a climb in the log")
	{
	    Arguments = { titleArgument },
	    Options = { editTitleOption, editGradeOption, editDateOption, editNoteOption, editSentOption, editAttemptsOption }
	};

	RootCommand rootCommand = new RootCommand("A simple climbing send log CLI tool")
	{
	    Options = { verboseOption },
	    Subcommands = { createCommand, listCommand, removeCommand, addAttemptCommand, editCommand }
	};

	//actions
	createCommand.SetAction(parseResult =>
	{
	    string title = parseResult.GetValue(titleArgument)!;
	    string grade = parseResult.GetValue(gradeArgument)!;
	    DateOnly date = parseResult.GetValue(dateOption);
	    bool sent = parseResult.GetValue(sentOption);
	    int attempts = parseResult.GetValue(attemptOption);
	    string note = parseResult.GetValue(noteOption)!;
	    bool verbose = parseResult.GetValue(verboseOption);

	    List<ClimbLog> climbs = LoadClimbs();
	    int id = climbs.Count > 0 ? climbs.Max(c => c.Id) + 1 : 1;
	    ClimbLog climb = new ClimbLog(
		id,
		title,
		grade,
		date,
		sent,
		attempts,
		note
	    );
	    climbs.Add(climb);
	    SaveClimbs(climbs);

	    Console.WriteLine($"Added climb {id}: {title} | {grade} | {date}");
	    if (verbose)
		Console.WriteLine($"Sent: {sent} |  Attempts: {attempts} |  Note: {note}");
	});

	listCommand.SetAction(parseResult =>
	{
	    bool verbose = parseResult.GetValue(verboseOption);
	    DateOnly? date = parseResult.GetValue(filterByDateOption)!;
	    string grade = parseResult.GetValue(filterByGradeOption)!;
	    bool? sent = parseResult.GetValue(filterBySentOption)!;
	    List<ClimbLog> climbs = LoadClimbs();

	    climbs = climbs.Where(climb =>
	        (sent is null || climb.Sent == sent) &&
		(date is null || climb.Date == date) &&
		(grade is null || climb.Grade == grade)
	    ).ToList();
	    
	    if (climbs.Count == 0)
	    {
		Console.WriteLine("No climbs logged.");
		return;
	    }
	    
	    foreach(ClimbLog climb in climbs)
	    {
		Console.WriteLine($"Climb {climb.Id}: {climb.Title} | {climb.Grade} | {climb.Date}");

		if(verbose)
		    Console.WriteLine($"Sent: {climb.Sent} | Attempts: {climb.Attempts} | Note: {climb.Note}");
	    }
	});

	removeCommand.SetAction(parseResult =>
	{
	    string title = parseResult.GetValue(titleArgument)!;
	    bool verbose = parseResult.GetValue(verboseOption);

	    List<ClimbLog> climbs = LoadClimbs();
	    ClimbLog climb = climbs.FirstOrDefault(c => c.Title == title)!;

	    if(climb is null)
	    {
		Console.Error.WriteLine($"Climb {title} not found.");
		return -1;
	    }

	    climbs.Remove(climb);
	    SaveClimbs(climbs);

	    Console.WriteLine($"Removed climb {climb.Id}: {climb.Title}");
	    if(verbose)
		Console.WriteLine($"Note: {climb.Note}");

	    return 0;
	});

	editCommand.SetAction(parseResult =>
	{
	    string title = parseResult.GetValue(titleArgument)!;
	    List<ClimbLog> climbs = LoadClimbs();
	    ClimbLog climb = climbs.FirstOrDefault(c => c.Title == title)!;

	    if(climb is null)
	    {
		Console.Error.WriteLine($"Climb {title} not found.");
		return -1;
	    }

	    bool verbose = parseResult.GetValue(verboseOption);
	    string updatedTitle = parseResult.GetValue(editTitleOption) ?? climb.Title;
	    string updatedGrade = parseResult.GetValue(editGradeOption) ?? climb.Grade;
	    string updatedNote = parseResult.GetValue(editNoteOption) ?? climb.Note;
	    DateOnly updatedDate = parseResult.GetValue(editDateOption) ?? climb.Date;
	    bool updatedSent = parseResult.GetValue(editSentOption) ?? climb.Sent;
	    int updatedAttempts = parseResult.GetValue(editAttemptsOption) ?? climb.Attempts;

	    climbs[climbs.IndexOf(climb)] = climb with
	    {
		Title = updatedTitle,
		Grade = updatedGrade,
		Date = updatedDate,
		Note = updatedNote,
		Sent = updatedSent,
		Attempts = updatedAttempts
	    };
	    SaveClimbs(climbs);

	    Console.WriteLine($"Edited climb {climb.Id}: {updatedTitle} | {updatedDate}");
	    if(verbose)
		Console.WriteLine($"Sent: {updatedSent} |  Attempts: {updatedAttempts} |  Note: {updatedNote}");
	    return 0;
	});

	addAttemptCommand.SetAction(parseResult =>
	{
	    string title = parseResult.GetValue(titleArgument)!;
	    bool verbose = parseResult.GetValue(verboseOption);

	    List<ClimbLog> climbs = LoadClimbs();
	    ClimbLog climb = climbs.FirstOrDefault(c => c.Title == title)!;

	    if(climb is null)
	    {
		Console.Error.WriteLine($"Climb {title} not found.");
		return -1;
	    }
	    
	    int updatedAttempts = climb.Attempts + 1;
	    climbs[climbs.IndexOf(climb)] = climb with { Attempts = updatedAttempts };
	    SaveClimbs(climbs);

	    Console.WriteLine($"Edited climb {climb.Id}: {climb.Title}");
	    if(verbose)
		Console.WriteLine($"Sent: {climb.Sent} |  Attempts: {updatedAttempts} |  Note: {climb.Note}");
	    return 0;
	});

	return rootCommand.Parse(args).Invoke();
    }

    //data helper functions
    static string GetClimbLogFilePath() => Path.Combine(
	Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
	"CurioCabinetSendLog",
	"climbs.json");

    static List<ClimbLog> LoadClimbs()
    {
	string path = GetClimbLogFilePath();

	if(!File.Exists(path))
	    return new List<ClimbLog>();

	string json = File.ReadAllText(path);
	return JsonSerializer.Deserialize(json, ClimbJsonContext.Default.ListClimbLog) ?? new List<ClimbLog>();
    }

    static void SaveClimbs(List<ClimbLog> climbs)
    {
	string path = GetClimbLogFilePath();
	Directory.CreateDirectory(Path.GetDirectoryName(path)!);
	string json = JsonSerializer.Serialize(climbs, ClimbJsonContext.Default.ListClimbLog);
	File.WriteAllText(path, json);
    }

    public record ClimbLog(
	int Id,
	string Title,
	string Grade,
	DateOnly Date,
	bool Sent,
	int Attempts,
	string Note
    );

    [System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = true)]
    [System.Text.Json.Serialization.JsonSerializable(typeof(List<ClimbLog>))]
    internal partial class ClimbJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
}
