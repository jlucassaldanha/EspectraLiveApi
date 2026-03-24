using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchUsersIdsData(
	[property: JsonPropertyName("user_id")] string UserId
);

public record TwitchPaginationData(string? Cursor);

public record TwitchUsersIdsResponse(
	List<TwitchUsersIdsData> Data, 
	TwitchPaginationData? Pagination
);
