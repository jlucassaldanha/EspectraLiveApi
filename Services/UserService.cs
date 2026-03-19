using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;
using System.Net;

namespace SpectraLiveApi.Services;

public class UserService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly string _apiUrl;
	private readonly IUserRepository _userRepository;

	public UserService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi,  IOptions<SpectraLiveSettings> options, IUserRepository userRepository)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_apiUrl = options.Value.ApiUrl;
		_userRepository = userRepository;
	}

	public async Task<Result<UserProfile>> GetUserProfile(string userId)
	{
		User? userProfileFromDb = await _userRepository.GetProfileByUserIdAsync(Guid.Parse(userId));

		if (userProfileFromDb == null)
			return Result<UserProfile>.Failure(new ("Usuário não encontrado no banco de dados.", HttpStatusCode.NotFound));

		var userProfile = new UserProfile(
			userProfileFromDb.TwitchId,
			userProfileFromDb.DisplayName,
			userProfileFromDb.ProfileImgUrl,
			userProfileFromDb.Id
		);

		return Result<UserProfile>.Success(userProfile);
	}
}