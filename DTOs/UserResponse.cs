using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs;

public record TwitchUserData(
	string Id, 
	string Login, 
	[property: JsonPropertyName("display_name")] string DisplayName, 
	[property: JsonPropertyName("profile_img_url")] string ProfileImgUrl
);
public record TwitchUserResponse(List<TwitchUserData> Data );

public record TwitchUserError(int Status, string Message);