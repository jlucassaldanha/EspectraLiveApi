using SpectraLiveApi.Models;

namespace SpectraLiveApi.Repositories;

public interface IUserRepository
{
	Task<User?> GetProfileByTwitchIdAsync(string twitchId);
	Task AddAsync(User user);
	Task UpdateAsync(User user);
}