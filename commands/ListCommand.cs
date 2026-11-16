namespace CurioCabinetCli;

internal partial class Program
{
    static Command ListCommand(Option<bool> verboseOption)
    {
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

        Command listCommand = new Command("list", "List all climbs in log")
        {
            Options = { filterByDateOption, filterBySentOption, filterByGradeOption }
        };

        listCommand.SetAction(parseResult =>
        {
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
                return 0;
            }

            foreach (ClimbLog climb in climbs)
                Console.WriteLine($"{climb}");

            return 0;
        });

        return listCommand;
    }
}
