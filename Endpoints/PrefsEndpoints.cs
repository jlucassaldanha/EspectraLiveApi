using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpectraLiveApi.Common;
using SpectraLiveApi.DTOs.Twitch;
using SpectraLiveApi.Services;

namespace SpectraLiveApi.Endpoints;

public static class PrefsEndpoints
{
	public static void MapPrefsEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/preferences");

		group.MapPost("/unviews", async (ClaimsPrincipal user, UnviewsService unviewsService, [FromBody] TwitchIdsRequest unviewIds) =>
		{
			var userId = user.GetUserId();

			if (userId == null)
				return Results.Unauthorized();

			if (unviewIds.TwitchIds == null || unviewIds.TwitchIds.Length == 0) 
				return Results.BadRequest("Nenhum ID informado.");

			await unviewsService.AddUnviewsToUser(unviewIds.TwitchIds, userId);
			
			return Results.Ok();
		})
		.RequireAuthorization();

		group.MapDelete("/unviews", async (ClaimsPrincipal user, UnviewsService unviewsService, [FromBody] TwitchIdsRequest unviewIds) =>
		{
			var userId = user.GetUserId();

			if (userId == null)
				return Results.Unauthorized();

			if (unviewIds.TwitchIds == null || unviewIds.TwitchIds.Length == 0) 
				return Results.BadRequest("Nenhum ID informado.");

			await unviewsService.DeleteUnviewsFromUser(unviewIds.TwitchIds, userId);

			return Results.Ok();
		}).RequireAuthorization();

		group.MapGet("/unviews", async (ClaimsPrincipal user, UnviewsService unviewsService) => 
		{
			var userId = user.GetUserId();

			if (userId == null)
				return Results.Unauthorized();

			var response = await unviewsService.ListUnviewsIds(userId);

			if (response.Error != null)
				return Results.Problem(
					detail: response.Error.Message, 
					statusCode: (int)response.Error.ErrorCode
				);

			if (response.Data == null)
				return Results.InternalServerError(new { Error = "Erro inesperado ao buscar dados de ids da Twitch" });

			return Results.Ok(new TwitchIdsResponse(response.Data.IdsList));
		})
		.RequireAuthorization();
	}
}