using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;
using System.Net;

namespace SpectraLiveApi.Services;

public class UserService
{
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<Result<UserProfile>> GetUserProfile(string userId)
	{
		User? userProfileFromDb = await _userRepository.GetProfileByUserIdAsync(Guid.Parse(userId));

		if (userProfileFromDb == null)
			return Result<UserProfile>.Failure(new Error("Usuário não encontrado no banco de dados.", HttpStatusCode.NotFound));

		var userProfile = new UserProfile(
			userProfileFromDb.TwitchId,
			userProfileFromDb.DisplayName,
			userProfileFromDb.ProfileImgUrl,
			userProfileFromDb.Id
		);

		return Result<UserProfile>.Success(userProfile);
	}
}