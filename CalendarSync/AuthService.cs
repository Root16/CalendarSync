﻿using System.Text.Json;

namespace CalendarSync
{
	public class AuthService
	{
		private readonly string ClientId = "";
		private HttpClient Client { get; set; } = new HttpClient();
		public AuthService(string clientId)
		{
			ClientId = clientId;
		}

		public async Task<RefreshTokenResponse> GetToken(string refreshToken)
		{
			var values = new Dictionary<string, string>()
			{
				["client_id"] = ClientId,
				["grant_type"] = "refresh_token",
				["scope"] = "offline_access Calendars.ReadWrite",
				["refresh_token"] = refreshToken,
			};
			var body = new FormUrlEncodedContent(values);

			using var response = await Client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", body);

			var resp = await response.Content.ReadAsStringAsync();
			using var stream = await response.Content.ReadAsStreamAsync();
			response.EnsureSuccessStatusCode();
			var responseBody = await JsonSerializer.DeserializeAsync<RefreshTokenResponse>(stream);
			return responseBody;
		}
	}
}