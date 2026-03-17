using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
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

		group.MapGet("/callback", async (HttpContext context, AuthService authService, string code, string? error) =>
		{
			if (error != null) return Results.Unauthorized();

			var sessionResponse = await authService.GetSessionWithTwitchCode(code);

			if (sessionResponse.Error != null)
				return Results.BadRequest(new { Error = sessionResponse.Error.ErrorMessage });
			

			if (sessionResponse.Success == null)
				return Results.BadRequest(new { Error = "Erro inesperado em callback" });

			context.Response.Cookies.Append("SessionToken",  sessionResponse.Success.SessionToken, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Expires = DateTimeOffset.UtcNow.AddMinutes(5)
				}
			);
	
			return Results.Ok(new { message = "SessionToken gravado com sucesso."});
		});

		group.MapGet("/me", async (HttpContext context, AuthService authService, JwtService jwtService) =>
		{
			context.Request.Cookies.TryGetValue("SessionToken", out var sessionToken);
			context.Request.Cookies.TryGetValue("AuthToken", out var authToken);
			
			UserData userData;

			if (sessionToken != null)
			{
				var result = await authService.GetUserInformationWithSession(sessionToken);
				
				if (result.Error != null)
					return Results.BadRequest(result.Error.Message);
				
				if (result.Success == null)
					return Results.BadRequest(new { Error = "Erro inesperado ao tentar buscar informações do usuário." });

				userData = result.Success;

				var newAuthToken = jwtService.GenerateToken(userData.Id.ToString(), userData.TwitchId);
				
				context.Response.Cookies.Delete("SessionToken");

				context.Response.Cookies.Append("AuthToken", newAuthToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					}
				);
			}
			else if (sessionToken == null && authToken != null)
			{
				var result = await authService.GetUserInformationWithJwt(authToken);

				if (result.Error != null)
					return Results.BadRequest(result.Error.Message);
				
				if (result.Success == null)
					return Results.BadRequest(new { Error = "Erro inesperado ao tentar buscar informações do usuário." });

				userData = result.Success;
			}
			else
			{
				return Results.Unauthorized();
			}

			return Results.Ok(userData);
		});

		group.MapGet("/logout", (HttpContext context) => {
			context.Response.Cookies.Delete("SessionToken");
			context.Response.Cookies.Delete("AuthToken");
			Results.Ok(new { message = "Logout realizado com sucesso."});
		});
	}
}