#!/usr/bin/env dotnet
#:package System.CommandLine@2.0.14
#:include commands/CreateCommand.cs
#:include commands/EditCommand.cs
#:include commands/RemoveCommand.cs
#:include commands/ListCommand.cs
#:include commands/StatsCommand.cs
#:include commands/AddAttemptCommand.cs

global using System.CommandLine;
using System.Text.Json;

namespace CurioCabinetCli;

internal partial class Program
{
    static int Main(string[] args)
    {
		Option<bool> verboseOption = new Option<bool>("--verbose")
		{
			Description = "Show detailed output",
			Recursive = true
		};
		
		Command createCommand = CreateCommand(verboseOption);
		Command addAttemptCommand = AddAttemptCommand(verboseOption);
		Command editCommand = EditCommand(verboseOption);
		Command removeCommand = RemoveCommand(verboseOption);
		Command listCommand = ListCommand(verboseOption);
		Command statsCommand = StatsCommand(verboseOption);

		RootCommand rootCommand = new RootCommand("A simple climbing send log CLI tool")
		{
			Options = { verboseOption },
			Subcommands = { createCommand, listCommand, removeCommand, addAttemptCommand, editCommand, statsCommand }
		};

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
