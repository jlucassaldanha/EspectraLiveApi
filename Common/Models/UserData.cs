namespace SpectraLiveApi.Common.Models;

public record UserData(
	string AccessToken,
	string RefreshToken,
	int ExpiresIn,
	string TwitchId,
	string DisplayName, 
	string ProfileImgUrl,
	Guid Id
);