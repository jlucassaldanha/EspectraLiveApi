using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Services;

namespace SpectraLiveApi.Endpoints;

public static class AuthEndpoints
{
	public static void MapAuthEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/auth");

		group.MapGet("/login", async (IOptions<TwitchSettings> twitchOptions, IOptions<SpectraLiveSettings> spectraLiveOptions) =>
		{
			string twitchAuthUrl = 
				"https://id.twitch.tv/oauth2/authorize" +
				"?response_type=code" +
        		$"&client_id={twitchOptions.Value.ClientId}" +
        		$"&redirect_uri={spectraLiveOptions.Value.ApiUrl}/auth/callback" +
				"&scope=user:read:email moderation:read moderator:read:chatters";

			return Results.Redirect(twitchAuthUrl);
		});

		group.MapGet("/callback", async (HttpContext context, IConfiguration config, AuthService authService, string code, string? error) =>
		{
			if (error != null) return Results.Unauthorized();

			var sessionResponse = await authService.GetSessionWithTwitchCode(code);

			if (sessionResponse.Error != null)
				return Results.BadRequest(new { Error = sessionResponse.Error.ErrorMessage });
			

			if (sessionResponse.Success == null)
				return Results.BadRequest(new { Error = "Erro inesperado em callback" });

			context.Response.Cookies.Append(
				"sessionToken", 
				sessionResponse.Success.SessionToken, 
				new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Expires = DateTimeOffset.UtcNow.AddMinutes(5)
				}
			);
			
			var frontendUrl = config["SpectraLive:FrontendUrl"];
	
			return Results.Redirect(config["SpectraLive:FrontendUrl"] + "/dashboard");
		});

		group.MapGet("/me", async (HttpContext context, AuthService authService) =>
		{
			context.Request.Cookies.TryGetValue("SessionToken", out var sessionToken);
			context.Request.Cookies.TryGetValue("AuthToken", out var authToken);

			
			if (authToken == null && sessionToken != null)
			{
				await authService.GetUserInformationWithSession();
				
				context.Response.Cookies.Delete("SessionToken");
			}

			if (sessionToken == null && authToken != null)
			{
				await authService.GetUserInformationWithJwt();
			}

			
		});
	}
}