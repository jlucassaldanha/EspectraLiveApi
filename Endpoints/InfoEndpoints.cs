using System.Security.Claims;
using SpectraLiveApi.DTOs.Users;
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
	}
}