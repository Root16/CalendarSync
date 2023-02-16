using CalendarSync;
using CalendarSync.Console;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.TaskScheduler;

const string DailyTaskName = "SyncCalendar";
var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

var host = Host.CreateDefaultBuilder().Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Logging established");

var primaryAccountRefreshToken = configuration["PrimaryAccountRefreshToken"];
var primaryAccountSubjectPrefix = configuration["PrimaryAccountSubjectPrefix"];
var secondaryAccountRefreshToken = configuration["PrimaryAccountRefreshToken"];
var secondaryAccountSubjectPrefix = configuration["SecondaryAccountSubjectPrefix"];
var clientId = configuration["PrimaryAccountRefreshToken"];
var orgConnectionString = configuration["PrimaryAccountRefreshToken"];
var daysToSync = uint.Parse(configuration["DaysToSync"]);

try
{
	using TaskService ts = new TaskService();
	if (ts.GetTask(DailyTaskName) == null)
	{
		TaskDefinition td = ts.NewTask();
		td.RegistrationInfo.Description = "Every day at 9 am sync calendars";

		DailyTrigger trigger = new()
		{
			StartBoundary = DateTime.Today + new TimeSpan(10, 45, 0),
			DaysInterval = 1
		};
		td.Triggers.Add(trigger);

		var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/CalendarSync";

		td.Actions.Add(new ExecAction($"{dir}/CalendarSync.Console.exe", dir, null));

		ts.RootFolder.RegisterTaskDefinition(DailyTaskName, td);
	}

	var source = new SecondaryAccToPrimaryAccProfile(secondaryAccountRefreshToken, secondaryAccountSubjectPrefix);
	var dest = new PrimaryAccToSecondaryAccProfile(primaryAccountRefreshToken, primaryAccountSubjectPrefix);
	var service = new CalendarSyncService(dest, source, logger, clientId, orgConnectionString);
	var startTime = DateTime.UtcNow;
	var endTime = startTime.AddDays(daysToSync);

	Console.WriteLine("TESTTTTTT");
	Console.ReadKey();

	//await service.SyncRangeBidirectionalAsync(startTime.ToString("O"), endTime.ToString("O"));

}
catch (Exception e)
{
	Console.WriteLine(e.ToString());
}
finally
{
	Console.ReadKey();
}
