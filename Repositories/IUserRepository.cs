using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Repositories;

public interface IUserRepository
{
	Task<User?> GetProfileByTwitchIdAsync(string twitchId);
	Task<User?> GetProfileByUserIdAsync(Guid id);
	Task AddAsync(User user);
	Task UpdateAsync(User user);
}