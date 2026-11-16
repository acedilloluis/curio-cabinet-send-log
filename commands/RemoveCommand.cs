namespace CurioCabinetCli;

internal partial class Program
{
    static Command RemoveCommand(Option<bool> verboseOption)
    {
        Argument<string> titleArgument = new Argument<string>("title")
        {
            Description = "Name of climb"
        };

        Command removeCommand = new Command("remove", "Remove a climb from the log")
        {
            Arguments = { titleArgument }
        };

        removeCommand.SetAction(parseResult =>
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

            climbs.Remove(climb);
            SaveClimbs(climbs);

            Console.WriteLine($"Removed climb: {climb.Title}");
            if (verbose)
                Console.WriteLine($"{climb}");

            return 0;
        });

        return removeCommand;
    }
}
