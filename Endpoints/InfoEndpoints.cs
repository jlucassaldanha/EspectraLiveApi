using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpectraLiveApi.DTOs.Twitch;
using SpectraLiveApi.DTOs.Users;
using SpectraLiveApi.Services;

namespace SpectraLiveApi.Endpoints;

public static class InfoEndpoints
{
	public static void MapInfoEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/information");

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

		group.MapGet("/moderators", async (ClaimsPrincipal user, TwitchService twitchService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var response = await twitchService.GetTwitchUserModeratorsIds(userId, twitchId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			return Results.Ok(new { data = response.Data });
		})
		.RequireAuthorization();

		group.MapGet("/chatters", async (ClaimsPrincipal user, TwitchService twitchService) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var response = await twitchService.GetTwitchUserChattersIds(userId, twitchId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			return Results.Ok(new { data = response.Data });
		})
		.RequireAuthorization();

		group.MapGet("/users", async (ClaimsPrincipal user, UserService userService, AuthService authService, TwitchService twitchService, [FromBody] TwitchIdsRequest twitchIds) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var twitchId = user.Claims.FirstOrDefault(c => c.Type == "twitchId")?.Value;
			
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(twitchId))
				return Results.Unauthorized();

			var response = await twitchService.GetTwitchUsersData(userId, twitchId, twitchIds.TwitchIds.ToList());

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar users na Twitch" });	

			return Results.Ok(new { data = response.Data });
		})
		.RequireAuthorization();	
	}
}