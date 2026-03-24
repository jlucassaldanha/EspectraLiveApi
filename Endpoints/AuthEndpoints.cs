using Microsoft.Extensions.Options;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Services;
using SpectraLiveApi.DTOs.Auth;
using System.Security.Claims;

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

		group.MapGet("/callback", async (IOptions<SpectraLiveSettings> spectraLiveOptions, HttpContext context, AuthService authService, JwtService jwtService, string code, string? error) =>
		{
			if (error != null) return Results.Unauthorized();

			var response = await authService.GetTwitchAuthDataWithCode(code);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado em callback" });

			var result = await authService.UpsertUserWithTwitchAuthData(response.Data);
				
			if (result.Error != null)
				return Results.Problem(
					detail: result.Error.Message,
					statusCode: (int)result.Error.ErrorCode
				);
			
			if (result.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao tentar registrar informações do usuário." });

			var sessionToken = jwtService.GenerateToken(
				result.Data.Id.ToString(), 
				result.Data.TwitchId, 
				DateTime.UtcNow.AddMinutes(1)
			);

			context.Response.Cookies.Append("sessionToken",  sessionToken, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Expires = DateTimeOffset.UtcNow.AddMinutes(5)
				}
			);
	
			//return Results.Ok(new { message = $"Usuário gravado com sucesso."});
			return Results.Redirect(spectraLiveOptions.Value.FrontendUrl + "/success");
		});

		group.MapGet("/token", async (HttpContext context, AuthService authService, JwtService jwtService) =>
		{
			context.Request.Cookies.TryGetValue("sessionToken", out var sessionToken);

			if (sessionToken != null)
			{
				var principal = jwtService.ValidateToken(sessionToken);

				if (principal == null)
					return Results.Unauthorized();

				var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var twitchId = principal.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;
				
				if (userId == null || twitchId == null) 
					return Results.Unauthorized();
				
				var newToken = jwtService.GenerateToken(userId, twitchId, DateTime.UtcNow.AddDays(7));

				context.Response.Cookies.Append("jwt",  newToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None,
						Expires = DateTimeOffset.UtcNow.AddMinutes(5)
					}
				);
				
				context.Response.Cookies.Delete("sessionToken");

				var token = new TokenResponse(newToken);

				return Results.Ok(token);
			}
			else
			{
				return Results.Unauthorized();
			}
		});

		group.MapGet("/logout", (HttpContext context) => {
			context.Response.Cookies.Delete("sessionToken");
			context.Response.Cookies.Delete("jwt");
			Results.Ok(new { message = "Logout realizado com sucesso."});
		});
	}
}