#!/usr/bin/env dotnet
#:package System.CommandLine@2.0.14

using System;
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
	
	Option<DateOnly> dateOption = new Option<DateOnly>("--date")
	{
	    Description = "Date climb sent or attempted (uses current culture format)",
	    DefaultValueFactory = _ => DateOnly.FromDateTime(DateTime.Now)
	};

	Option<bool> sentOption = new Option<bool>("--sent")
	{
	    Description = "Climb sent",
	    DefaultValueFactory = _ => true
	};

	Option<int> attemptOption = new Option<int>("--attempted")
	{
	    Description = "Climb attempted x times",
	    DefaultValueFactory = _ => 1
	};

	Option<string> noteOption = new Option<string>("--note")
	{
	    Description = "Enter a short note about the climb",
	    DefaultValueFactory = _ => "There's nothing here."
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

	Command listCommand = new Command("list", "List all climbs in log");

	Command removeCommand = new Command("remove", "Remove a climb from the log")
	{
	    Arguments = { titleArgument }
	};

	Command addAttemptCommand = new Command("addAttempt", "Edit a climb in the log")
	{
	    Arguments = { titleArgument }
	};

	RootCommand rootCommand = new RootCommand("A simple climbing send log CLI")
	{
	    Options = { verboseOption },
	    Subcommands = { createCommand, listCommand, removeCommand, addAttemptCommand }
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
	    List<ClimbLog> climbs = LoadClimbs();

	    if(climbs.Count == 0)
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
