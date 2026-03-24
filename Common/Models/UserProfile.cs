namespace SpectraLiveApi.Common.Models;

public record UserProfile(
	string TwitchId,
	string DisplayName, 
	string ProfileImgUrl,
	Guid Id
);