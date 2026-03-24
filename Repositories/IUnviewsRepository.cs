using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Repositories;

public interface IUnviewsRepository
{
	Task<List<Unviews>> GetUnviewsByUserIdAsync(string userId);
	Task AddUnviewsAsync(IEnumerable<Unviews> unviewUsers);
	Task DeleteUnviewsFromUserIdAsync(IEnumerable<string> twitchIds, string userId);
}