using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs;

public record TwichUserData(
	string Id, 
	string Login, 
	[property: JsonPropertyName("display_name")] string DisplayName, 
	[property: JsonPropertyName("profile_img_url")] string ProfileImgUrl
);
public record TwitchUserResponse(List<TwichUserData> Data );