using Calendula;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.TaskScheduler;

const string DailyTaskName = "Calendula";
var configuration = new ConfigurationBuilder()
 .AddJsonFile("appsettings.json")
 .Build(); var host = Host.CreateDefaultBuilder().Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(@"
            BE NOT AFRAID
            Your calendars will begin to sync
            DO NOT close this window until you see the final logging message.
");

var primaryAccountRefreshToken = configuration["PrimaryAccountRefreshToken"];
var primaryAccountSubjectPrefix = configuration["PrimaryAccountSubjectPrefix"];
var secondaryAccountRefreshToken = configuration["SecondaryAccountRefreshToken"];
var secondaryAccountSubjectPrefix = configuration["SecondaryAccountSubjectPrefix"];
var hour24Time = int.Parse(configuration["Hour24Time"]);
var minute24Time = int.Parse(configuration["Minute24Time"]);
var clientId = configuration["ClientId"];
var orgConnectionString = configuration["OrgConnectionString"];
var daysToSync = uint.Parse(configuration["DaysToSync"]);

try
{
    using TaskService ts = new TaskService();
    var CalendulaTask = ts.GetTask(DailyTaskName);

    if (CalendulaTask != null)
    {
        ts.RootFolder.DeleteTask(DailyTaskName);
    }

    TaskDefinition td = ts.NewTask();

    td.RegistrationInfo.Description = "Every day at 9 am sync calendars"; DailyTrigger trigger = new()
    {
        StartBoundary = DateTime.Today + new TimeSpan(hour24Time, minute24Time, 0),
        DaysInterval = 1
    };

    var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Calendula";

    td.Triggers.Add(trigger);
    td.Actions.Add(new ExecAction($"{dir}/Calendula.Console.exe", dir, null));
    ts.RootFolder.RegisterTaskDefinition(DailyTaskName, td);

    var source = new SecondaryAccToPrimaryAccProfile(secondaryAccountRefreshToken, secondaryAccountSubjectPrefix);
    var dest = new PrimaryAccToSecondaryAccProfile(primaryAccountRefreshToken, primaryAccountSubjectPrefix);
    var service = new CalendulaService(dest, source, logger, clientId, orgConnectionString);
    var startTime = DateTime.UtcNow;
    var endTime = startTime.AddDays(daysToSync);
    await service.SyncRangeBidirectionalAsync(startTime.ToString("O"), endTime.ToString("O"));

    logger.LogInformation(@"
            Calendar syncing complete! 
            Go forth about your day and be productive.
    ");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
finally
{
    Console.ReadKey();
}
