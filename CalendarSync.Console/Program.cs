using CalendarSync;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const uint DefaultDaysToSync = 7;

Parser.Default.ParseArguments<Options>(args)
				   .WithParsed<Options>(async o =>
				   {
					   var host = Host.CreateDefaultBuilder().Build();
					   var logger = host.Services.GetRequiredService<ILogger<Program>>();
					   logger.LogInformation("Logging established");

					   var primaryAccountRefreshToken = o.PrimaryAccountRefreshToken;
					   var secondaryAccountRefreshToken = o.SecondaryAccountRefreshToken;
					   var clientId = o.ClientId;
					   var orgConnectionString = o.OrgConnectionString;

					   var daysToSync = o.DaysToSync != 0 ? o.DaysToSync : DefaultDaysToSync;

					   var source = new SecondaryAccToPrimaryAccProfile(secondaryAccountRefreshToken);
					   var dest = new PrimaryAccToSecondaryAccProfile(primaryAccountRefreshToken);
					   var service = new CalendarSyncService(dest, source, logger, clientId, orgConnectionString);
					   var startTime = DateTime.UtcNow;
					   var endTime = startTime.AddDays(daysToSync);

					   await service.SyncRangeBidirectionalAsync(startTime.ToString("O"), endTime.ToString("O"));
				   });

