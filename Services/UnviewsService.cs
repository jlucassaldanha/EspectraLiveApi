using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Services;

public class UnviewsService
{
	private readonly IUnviewsRepository _unviewsRepository;

	public UnviewsService(IUnviewsRepository unviewsRepository)
	{
		_unviewsRepository = unviewsRepository;
	}

	public async Task AddUnviewsToUser(string[] unviewsId, string userId)
	{
		var existingUnviews = await _unviewsRepository.GetUnviewsByUserIdAsync(userId);
		var existingUnviewsIds = existingUnviews.Select(u => u.TwitchId).ToList();
		var missingUnviews = unviewsId.Except(existingUnviewsIds).ToList();

		if (missingUnviews.Count > 0)
		{
			var newUnviews = missingUnviews.Select(twitchId => new Unviews(userId, twitchId)).ToList();
			await _unviewsRepository.AddUnviewsAsync(newUnviews);
		}
	}

	public async Task DeleteUnviewsFromUser(string[] unviewsId, string userId)
	{
		await _unviewsRepository.DeleteUnviewsFromUserIdAsync(unviewsId, userId);
	}

	public async Task<Result<UnviewsIds>> ListUnviewsIds(string userId)
	{
		var unviews = await _unviewsRepository.GetUnviewsByUserIdAsync(userId);
		var unviewsIds = unviews.Select(u => u.TwitchId);

		return Result<UnviewsIds>.Success(new UnviewsIds(unviewsIds.ToList()));
	}
}