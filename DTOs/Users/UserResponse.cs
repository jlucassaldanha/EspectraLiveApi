namespace SpectraLiveApi.DTOs.Users;

public record UserResponse(
	string DisplayName, 
	string ProfileImgUrl,
	Guid Id
);