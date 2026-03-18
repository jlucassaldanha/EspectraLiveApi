using Microsoft.Extensions.Options;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Services;
using SpectraLiveApi.DTOs.Users;

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
				return Results.Problem(
					detail: sessionResponse.Error.Message, 
					statusCode: (int)sessionResponse.Error.ErrorCode
				);

			if (sessionResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado em callback" });

			context.Response.Cookies.Append("sessionToken",  sessionResponse.Data.SessionToken, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Expires = DateTimeOffset.UtcNow.AddMinutes(5)
				}
			);
	
			return Results.Ok(new { message = "sessionToken gravado com sucesso."});
		});

		group.MapGet("/me", async (HttpContext context, AuthService authService, JwtService jwtService) =>
		{
			context.Request.Cookies.TryGetValue("sessionToken", out var sessionToken);
			context.Request.Cookies.TryGetValue("authToken", out var authToken);
			
			UserData userData;

			if (sessionToken != null)
			{
				var result = await authService.GetUserInformationWithSession(sessionToken);
				
				if (result.Error != null)
					return Results.Problem(
						detail: result.Error.Message,
						statusCode: (int)result.Error.ErrorCode
					);
				
				if (result.Data == null)
					return Results.InternalServerError(new { Error = "Erro inesperado ao tentar buscar informações do usuário." });

				userData = result.Data;

				var newAuthToken = jwtService.GenerateToken(userData.Id.ToString(), userData.TwitchId);
				
				context.Response.Cookies.Delete("sessionToken");

				context.Response.Cookies.Append("authToken", newAuthToken, new CookieOptions
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
					return Results.Problem(
						detail: result.Error.Message,
						statusCode: (int)result.Error.ErrorCode
					);
				
				if (result.Data == null)
					return Results.InternalServerError(new { Error = "Erro inesperado ao tentar buscar informações do usuário." });

				userData = result.Data;
			}
			else
			{
				return Results.Unauthorized();
			}

			var responseData = new UserResponse(
				userData.DisplayName,
				userData.ProfileImgUrl,
				userData.Id
			);

			return Results.Ok(responseData);
		});

		group.MapGet("/logout", (HttpContext context) => {
			context.Response.Cookies.Delete("sessionToken");
			context.Response.Cookies.Delete("authToken");
			Results.Ok(new { message = "Logout realizado com sucesso."});
		});
	}
}