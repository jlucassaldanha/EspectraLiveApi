using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchUserData(
	string Id, 
	string Login, 
	[property: JsonPropertyName("display_name")] string DisplayName, 
	[property: JsonPropertyName("profile_image_url")] string ProfileImgUrl
);
public record TwitchUserRequest(List<TwitchUserData> Data );
