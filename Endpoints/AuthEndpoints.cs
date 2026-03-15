namespace SpectraLiveApi.Endpoints;

public static class AuthEndpoints
{
	public static void MapAuthEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/auth");

		group.MapGet("/login", async (IConfiguration config) =>
		{
			var clientId = config["CLIENT_ID"];
			var redirectUri = config["API_URI"] + "/auth/callback";

			string twitchAuthUrl = 
				"https://id.twitch.tv/oauth2/authorize" +
				"?response_type=code" +
        		$"&client_id={clientId}" +
        		$"&redirect_uri={redirectUri}" +
				"&scope=user:read:email moderation:read moderator:read:chatters";

			return Results.Redirect(twitchAuthUrl);
		});

		group.MapGet("/callback", async (ICloneable config, string code, string? error) =>
		{
			if (error != null)
			{
				return Results.Unauthorized();
			}

			return null;
		});
	}
}