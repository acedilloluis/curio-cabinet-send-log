namespace CurioCabinetCli;

internal partial class Program
{
    static Command StatsCommand(Option<bool> verboseOption)
    {
        Command statsCommand = new Command("stats", "Generate summary statiscs");

        statsCommand.SetAction(parseResult =>
        {
            bool verbose = parseResult.GetValue(verboseOption);
            List<ClimbLog> climbs = LoadClimbs();

            if (climbs.Count == 0)
            {
                Console.Error.WriteLine("No climbs found.");
                return -1;
            }

            climbs.Sort((c1, c2) => c1.Attempts.CompareTo(c2.Attempts));
            IEnumerable<string> distinctGrades = climbs.Select(climb => climb.Grade).Distinct();

            int totalClimbs = climbs.Count;
            int totalFlashes = climbs.Where(climb => climb.Attempts == 1).Count();
            var mostCommonGrade = climbs.GroupBy(climb => climb.Grade)
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .OrderByDescending(counts => counts.Count)
                .First();

            string format1 = "| {0, -12} | {1, -13} | {2, -20} |";
            string line1 = new string('-', 55);

            Console.WriteLine(line1);
            Console.WriteLine(format1, "Total Climbs", "Total Flashes", "Mode Grade");
            Console.WriteLine(line1);

            Console.WriteLine(format1, totalClimbs, totalFlashes, $"{mostCommonGrade.Grade} ({mostCommonGrade.Count} climbs)");
            Console.WriteLine(line1);

            double avgAttempts = climbs.Average(climb => climb.Attempts);
            double medianAttempts = totalClimbs % 2 != 0 ?
                climbs[totalClimbs / 2].Attempts :
                (climbs[totalClimbs / 2].Attempts + climbs[(totalClimbs - 1) / 2].Attempts) / 2;
            int minAttempts = climbs.Min(climb => climb.Attempts);
            int maxAttempts = climbs.Max(climb => climb.Attempts);

            string format2 = "| {0, -12} | {1, -12} | {2, -15} | {3, -16} |";
            string line2 = new string('-', 68);

            Console.WriteLine(line2);
            Console.WriteLine(format2, "Min Attempts", "Max Attempts", "Median Attempts", "Average Attempts");
            Console.WriteLine(line2);

            Console.WriteLine(format2, minAttempts, maxAttempts, medianAttempts, avgAttempts);
            Console.WriteLine(line2);

            if (verbose)
            {
                string format3 = "| {0, -8} | {1, -5} | {2, -16}|";
                string line3 = new string('-', 39);

                Console.WriteLine(line3);
                Console.WriteLine(format3, "Grade", "Total", "Average Attempts");
                Console.WriteLine(line3);

                foreach (string grade in distinctGrades)
                {
                    int total = climbs.Where(climb => climb.Grade == grade).Count();
                    double avgAttemptsPerGrade = climbs
                        .Where(climb => climb.Grade == grade)
                        .Average(climb => climb.Attempts);

                    Console.WriteLine(format3, grade, total, $"{avgAttemptsPerGrade:F2}");
                    Console.WriteLine(line3);
                }
            }
            return 0;
        });

        return statsCommand;
    }
}
