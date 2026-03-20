using System.Security.Claims;

namespace SpectraLiveApi.Common;

public static class ClaimsPrincipalExpensions
{
	public static string? GetUserId(this ClaimsPrincipal user)
	{
		return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
	}
}