using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Text.Json;

namespace Calendula
{
    public class AuthService
    {
        private const string TokenCacheFileName = "CalendulaTokenCache";

        private IPublicClientApplication ClientApp;
        private MsalCacheHelper Cache;

        protected AuthService(IPublicClientApplication clientApp, MsalCacheHelper cache)
        {
            ClientApp = clientApp;
            Cache = cache;
        }

        public static async Task<AuthService> BuildAsync(string clientId)
        {
            var storageProperties = new StorageCreationPropertiesBuilder(TokenCacheFileName, MsalCacheHelper.UserRootDirectory)
                .Build();

            var cache = await MsalCacheHelper.CreateAsync(storageProperties);

            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithRedirectUri("http://localhost")
                .Build();

            cache.RegisterCache(app.UserTokenCache);

            return new AuthService(app, cache);
        }

        public async Task<string> GetToken(string username)
        {
            var accounts = await ClientApp.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a => a.Username == username);

            var scopes = new[] { "offline_access", "https://graph.microsoft.com/Calendars.ReadWrite" };
            AuthenticationResult authResult;

            try
            {
                authResult = await ClientApp.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                var tokenBuilder = ClientApp.AcquireTokenInteractive(scopes)
                    .WithPrompt(Prompt.NoPrompt);

                tokenBuilder = account == null
                    ? tokenBuilder.WithLoginHint(username)
                    : tokenBuilder.WithAccount(account);

                authResult = await tokenBuilder.ExecuteAsync();
            }

            return authResult.AccessToken;
        }
    }
}