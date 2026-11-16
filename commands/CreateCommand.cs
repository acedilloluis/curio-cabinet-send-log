namespace CurioCabinetCli;

internal partial class Program
{
    static Command CreateCommand(Option<bool> verboseOption)
    {
        Option<DateOnly> dateOption = new Option<DateOnly>("--date", "-d")
        {
            Description = "Date climb sent or attempted (uses current culture format)",
            DefaultValueFactory = _ => DateOnly.FromDateTime(DateTime.Now)
        };

        Option<bool> sentOption = new Option<bool>("--sent", "-s")
        {
            Description = "Climb sent",
            DefaultValueFactory = _ => false
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

        Argument<string> titleArgument = new Argument<string>("title")
        {
            Description = "Name of climb"
        };

        Argument<string> gradeArgument = new Argument<string>("grade")
        {
            Description = "Grade of climb"
        };

        Command createCommand = new Command("create", "Add a new a entry to the log")
        {
            Arguments = { titleArgument, gradeArgument },
            Options = { dateOption, sentOption, attemptOption, noteOption }
        };

        createCommand.SetAction(parseResult =>
        {
            string title = parseResult.GetValue(titleArgument)!;
            string grade = parseResult.GetValue(gradeArgument)!;
            DateOnly date = parseResult.GetValue(dateOption);
            bool sent = parseResult.GetValue(sentOption);
            int attempts = parseResult.GetValue(attemptOption);
            string note = parseResult.GetValue(noteOption)!;

            List<ClimbLog> climbs = LoadClimbs();
            ClimbLog climb = new ClimbLog(
                title,
                grade,
                date,
                sent,
                attempts,
                note
            );
            climbs.Add(climb);
            SaveClimbs(climbs);

            Console.WriteLine($"{climb}");
            return 0;
        });

        return createCommand;
    }
}
