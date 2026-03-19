using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Repositories;

public interface IUnviewUserRepository
{
	Task<User?> ListUnviewsByUserIdAsync(string userId);
	Task<User?> GetProfileByUserIdAsync(Guid id);
	Task AddAsync(User user);
	Task UpdateAsync(User user);
}