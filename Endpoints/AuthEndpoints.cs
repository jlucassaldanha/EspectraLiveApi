namespace SpectraLiveApi.Endpoints;

public static class AuthEndpoints
{
	public static void MapAuthEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/auth");

		group.MapGet("/login", async () =>
		{
			var CLIENT_ID = "";
			var REDIRECT_URI = "";

			string twitchAuthUrl = 
				"https://id.twitch.tv/oauth2/authorize" +
				"?response_type=code" +
        		$"&client_id={CLIENT_ID}" +
        		$"&redirect_uri={REDIRECT_URI}" +
				"&scope=user:read:email moderation:read moderator:read:chatters";
				
			return Results.Redirect(twitchAuthUrl);
		});
	}
}