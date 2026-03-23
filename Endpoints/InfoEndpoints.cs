using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpectraLiveApi.DTOs.Twitch;
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

		group.MapGet("/mods", async (ClaimsPrincipal user, UserService userService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var authResponse = await userService.GetTwitchUserAuthData(userId);

			if (authResponse.Error != null)
				return Results.Problem(
					detail: authResponse.Error.Message, 
					statusCode: (int)authResponse.Error.ErrorCode
				);

			if (authResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			var modsIdsResponse = await userService.GetTwitchUserMods(authResponse.Data.AccessToken, authResponse.Data.RefreshToken, twitchId);

			if (modsIdsResponse.Error != null)
				return Results.Problem(
					detail: modsIdsResponse.Error.Message, 
					statusCode: (int)modsIdsResponse.Error.ErrorCode
				);

			if (modsIdsResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar mods na Twitch" });	

			var modsResponse = await userService.GetTwitchUsersData(authResponse.Data.AccessToken, authResponse.Data.RefreshToken, twitchId, modsIdsResponse.Data.Ids);

			if (modsResponse.Error != null)
				return Results.Problem(
					detail: modsResponse.Error.Message, 
					statusCode: (int)modsResponse.Error.ErrorCode
				);

			if (modsResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar mods na Twitch" });	

			return Results.Ok(modsResponse.Data);
		})
		.RequireAuthorization();

		group.MapGet("/chatters", async (ClaimsPrincipal user, UserService userService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var authResponse = await userService.GetTwitchUserAuthData(userId);

			if (authResponse.Error != null)
				return Results.Problem(
					detail: authResponse.Error.Message, 
					statusCode: (int)authResponse.Error.ErrorCode
				);

			if (authResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			var chattersIdsResponse = await userService.GetTwitchUserChatters(authResponse.Data.AccessToken, authResponse.Data.RefreshToken, twitchId);

			if (chattersIdsResponse.Error != null)
				return Results.Problem(
					detail: chattersIdsResponse.Error.Message, 
					statusCode: (int)chattersIdsResponse.Error.ErrorCode
				);

			if (chattersIdsResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar chatters na Twitch" });	

			var chattersResponse = await userService.GetTwitchUsersData(authResponse.Data.AccessToken, authResponse.Data.RefreshToken, twitchId, chattersIdsResponse.Data.Ids);

			if (chattersResponse.Error != null)
				return Results.Problem(
					detail: chattersResponse.Error.Message, 
					statusCode: (int)chattersResponse.Error.ErrorCode
				);

			if (chattersResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar chatters na Twitch" });	

			return Results.Ok(chattersResponse.Data);
		})
		.RequireAuthorization();

		group.MapGet("/users", async (ClaimsPrincipal user, UserService userService, [FromBody] TwitchIdsRequest twitchIds) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;
			
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var authResponse = await userService.GetTwitchUserAuthData(userId);

			if (authResponse.Error != null)
				return Results.Problem(
					detail: authResponse.Error.Message, 
					statusCode: (int)authResponse.Error.ErrorCode
				);

			if (authResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			var usersResponse = await userService.GetTwitchUsersData(authResponse.Data.AccessToken, authResponse.Data.RefreshToken, twitchId, twitchIds.TwitchIds.ToList());

			if (usersResponse.Error != null)
				return Results.Problem(
					detail: usersResponse.Error.Message, 
					statusCode: (int)usersResponse.Error.ErrorCode
				);

			if (usersResponse.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar users na Twitch" });	

			return Results.Ok(usersResponse.Data);
			
		})
		.RequireAuthorization();	
	}
}