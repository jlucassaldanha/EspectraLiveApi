namespace SpectraLiveApi.Common.Models;

public record UserData(
	string AccessToken,
	string RefreshToken,
	int ExpiresIn,
	string TwitchId,
	string Login, 
	string DisplayName, 
	string ProfileImgUrl,
	Guid Id
);

public record UserError( string Message);