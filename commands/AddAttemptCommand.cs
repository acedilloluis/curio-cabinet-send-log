namespace CurioCabinetCli;

internal partial class Program
{
    static Command AddAttemptCommand(Option<bool> verboseOption)
    {
        Argument<string> titleArgument = new Argument<string>("title")
        {
            Description = "Name of climb"
        };

        Command addAttemptCommand = new Command("addAttempt", "Add one attempt to a climb in the log")
        {
            Arguments = { titleArgument }
        };

        addAttemptCommand.SetAction(parseResult =>
        {
            string title = parseResult.GetValue(titleArgument)!;
            bool verbose = parseResult.GetValue(verboseOption);

            List<ClimbLog> climbs = LoadClimbs();
            ClimbLog climb = climbs.FirstOrDefault(c => c.Title == title)!;

            if (climb is null)
            {
                Console.Error.WriteLine($"Climb {title} not found.");
                return -1;
            }

            int updatedAttempts = climb.Attempts + 1;
            climbs[climbs.IndexOf(climb)] = climb with { Attempts = updatedAttempts };
            SaveClimbs(climbs);

            Console.WriteLine($"Edited climb: {climb.Title}");
            if (verbose)
                Console.WriteLine($"{climb}");
            return 0;
        });

        return addAttemptCommand;
    }
}
