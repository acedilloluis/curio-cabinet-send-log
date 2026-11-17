#:package TUnit@1.*
#:include ../CurioCabinetCli.cs

//basic functionality tests, checking if commands return exit code 0 for valid input
[NotInParallel]
public class ExitCodeTests
{
    [Test]
    [Arguments("create", "test climb", "5.10a")]
    public async Task TestValidCreateCommands(params string[] args)
    {
        int result = CurioCabinetCli.Program.App(args);
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task TestListCommand()
    {
        int result = CurioCabinetCli.Program.App(new string[] { "list" });
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("edit", "test", "-eda", "4")]
    public async Task TestEditCommand(params string[] args)
    {
        int result = CurioCabinetCli.Program.App(args);
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("remove", "test")]
    public async Task TestRemoveCommand(params string[] args)
    {
        int result = CurioCabinetCli.Program.App(args);
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("stats")]
    public async Task TestStatsCommand(params string[] args)
    {
        int result = CurioCabinetCli.Program.App(args);
        await Assert.That(result).IsEqualTo(0);
    }
}