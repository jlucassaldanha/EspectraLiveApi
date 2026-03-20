using System.Security.Claims;
using SpectraLiveApi.DTOs.Users;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Services;

namespace SpectraLiveApi.Endpoints;

public static class InfoEndpoints
{
	public static void MapInfoEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/info");

		group.MapGet("/me", async (ClaimsPrincipal user, UserService userService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var result = await userService.GetUserProfile(userId);

			if (result.Error != null)
				return Results.Problem(
					detail: result.Error.Message,
					statusCode: (int)result.Error.ErrorCode
				);

			if (result.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao tentar ler informações do usuário." });

			var userResponse = new UserResponse(
				result.Data.DisplayName,
				result.Data.ProfileImgUrl,
				result.Data.Id	
			);	
			
			return Results.Ok(userResponse);

		})
		.RequireAuthorization();

		group.MapGet("/mods", async (ClaimsPrincipal user, UserService userService, TwitchApiClient twitchApiClient) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var response = await userService.GetTwitchUserAuthData(userId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			var mods = await twitchApiClient.GetUserMods(response.Data.AccessToken, twitchId);

			if (mods.Error != null)
				return Results.Problem(
					detail: mods.Error.Message, 
					statusCode: (int)mods.Error.ErrorCode
				);

			if (mods.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar mods na Twitch" });	

			return Results.Ok(mods.Data);
		})
		.RequireAuthorization();

		group.MapGet("/users", async (ClaimsPrincipal user, UserService userService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var response = await userService.GetTwitchUserAuthData(userId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			// Colocar parte de pesquisar usuarios
			return Results.Ok();
			
		})
		.RequireAuthorization();	

		group.MapGet("/chatters", async (ClaimsPrincipal user, UserService userService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var response = await userService.GetTwitchUserAuthData(userId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			// Colocar parte de pesquisar usuarios
			return Results.Ok();
			
		})
		.RequireAuthorization();	
	}
}