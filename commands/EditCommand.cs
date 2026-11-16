namespace CurioCabinetCli;
internal partial class Program
{
    static Command EditCommand(Option<bool> verboseOption)
    {
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

        Argument<string> titleArgument = new Argument<string>("title")
        {
            Description = "Name of climb"
        };

        Command editCommand = new Command("edit", "Edit a climb in the log")
        {
            Arguments = { titleArgument },
            Options = { editTitleOption, editGradeOption, editDateOption, editNoteOption, editSentOption, editAttemptsOption }
        };

        editCommand.SetAction(parseResult =>
        {
            string title = parseResult.GetValue(titleArgument)!;
            List<ClimbLog> climbs = LoadClimbs();
            ClimbLog climb = climbs.FirstOrDefault(c => c.Title == title)!;

            if (climb is null)
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

            Console.WriteLine($"Edited climb: {updatedTitle}");
            if (verbose)
                Console.WriteLine($"{climbs.First(c => c.Title == updatedTitle)}");
            return 0;
        });

        return editCommand;
    }
}
