using System.Security.Claims;

namespace SpectraLiveApi.Endpoints;

public static class PrefsEndpoints
{
	public static void MapPrefsEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/prefs");

		group.MapPost("/unviews", async (ClaimsPrincipal user) =>
		{
			var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdString))
				return Results.Unauthorized();

			return Results.Ok(new { Message = userIdString });
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