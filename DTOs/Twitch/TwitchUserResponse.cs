using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchUserData(
	string Id,  
	[property: JsonPropertyName("display_name")] string DisplayName, 
	[property: JsonPropertyName("profile_image_url")] string ProfileImgUrl
);
public record TwitchUserResponse(List<TwitchUserData> Data );
