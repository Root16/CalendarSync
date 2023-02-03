using CalendarSync;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const uint DefaultDaysToSync = 7;
var daysToSync = args.Length == 1 ? uint.Parse(args[0]) : DefaultDaysToSync;

Parser.Default.ParseArguments<Options>(args)
				   .WithParsed<Options>(async o =>
				   {
					   Console.WriteLine($"{o.SecondaryAccountRefreshToken} {o.PrimaryAccountRefreshToken} {o.ClientId}");

					   var host = Host.CreateDefaultBuilder().Build();
					   var logger = host.Services.GetRequiredService<ILogger<Program>>();
					   logger.LogInformation("Logging established");

					   var primaryAccountRefreshToken = o.PrimaryAccountRefreshToken;
					   var secondaryAccountRefreshToken = o.SecondaryAccountRefreshToken;
					   var clientId = o.ClientId;
					   var orgConnectionString = o.OrgConnectionString;


					   var source = new SecondaryAccToPrimaryAccProfile(secondaryAccountRefreshToken);
					   var dest = new PrimaryAccToSecondaryAccProfile(primaryAccountRefreshToken);
					   var service = new CalendarSyncService(dest, source, logger, clientId, orgConnectionString);
					   var startTime = DateTime.UtcNow;
					   var endTime = startTime.AddDays(daysToSync);

					   await service.SyncRangeBidirectionalAsync(startTime.ToString("O"), endTime.ToString("O"));
				   });

public class Options
{
	[Option('s', "secondaryAccountRefreshToken", Required = true, HelpText = "Refresh token for account1")]
	public string SecondaryAccountRefreshToken { get; set; }

	[Option('p', "primaryAccountRefreshToken", Required = true, HelpText = "Refresh token for account2")]
	public string PrimaryAccountRefreshToken { get; set; }

	[Option('c', "clientId", Required = true, HelpText = "Trusted ClientId")]
	public string ClientId { get; set; }

	[Option('d', "daysToSync", Required = false, HelpText = "Default number of days into the future to sync")]
	public string DefaultDaysToSync { get; set; }
	[Option('o', "orgConnectionString", Required = false, HelpText = "Connection string to a dev dataverse org")]
	public string OrgConnectionString { get; set; }
}
