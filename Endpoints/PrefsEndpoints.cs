using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpectraLiveApi.Services;

namespace SpectraLiveApi.Endpoints;

public static class PrefsEndpoints
{
	public static void MapPrefsEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/prefs");

		group.MapPost("/unviews", async (ClaimsPrincipal user, UserService userService, [FromQuery] string[] twitchId) =>
		{
			var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			if (twitchId == null || twitchId.Length == 0) 
				return Results.BadRequest("Nenhum ID informado.");

			var response = await userService.GetTwitchUserAuthData(userId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de autenticação da Twitch" });

			

			return Results.Ok(new { Message = userId });
		})
		.RequireAuthorization();

		group.MapDelete("/unviews/{id:guid}", async () => {});

		group.MapGet("/unviews", async (ClaimsPrincipal user) => 
		{
			var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdString))
				return Results.Unauthorized();

			return Results.Ok(new { Message = userIdString });
		})
		.RequireAuthorization();
	}
}