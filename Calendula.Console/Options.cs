using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendula.Console
{
    public class Options
    {
        [Option('s', "secondaryAccountRefreshToken", Required = true, HelpText = "Refresh token for seondary account")]
        public string SecondaryAccountRefreshToken { get; set; }

        [Option('x', "secondaryAccountSubjectPrefix", Required = true, HelpText = "Subject prefix for secondary account")]
        public string SecondaryAccountSubjectPrefix { get; set; }

        [Option('p', "primaryAccountRefreshToken", Required = true, HelpText = "Refresh token for primary account")]
        public string PrimaryAccountRefreshToken { get; set; }

        [Option('y', "primaryAccountSubjectPrefix", Required = true, HelpText = "Subject prefix primary account")]
        public string PrimaryAccountSubjectPrefix { get; set; }

        [Option('c', "clientId", Required = true, HelpText = "Trusted ClientId")]
        public string ClientId { get; set; }

        [Option('o', "orgConnectionString", Required = true, HelpText = "Connection string to a dev dataverse org")]
        public string OrgConnectionString { get; set; }

        [Option('d', "daysToSync", Required = false, HelpText = "Default number of days into the future to sync")]
        public uint DaysToSync { get; set; }
    }
}
