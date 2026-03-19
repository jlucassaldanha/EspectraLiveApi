using Microsoft.EntityFrameworkCore;
using SpectraLiveApi.Data;
using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _db;

	public UserRepository(AppDbContext db)
	{
		_db = db;
	}

	public async Task<User?> GetProfileByTwitchIdAsync(string twitchId)
	{
		return await _db.Users.FirstOrDefaultAsync(u => u.TwitchId == twitchId);
	}

	public async Task<User?> GetProfileByUserIdAsync(Guid id)
	{
		return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
	}

	public async Task AddAsync(User user)
	{
		await _db.Users.AddAsync(user);
		await _db.SaveChangesAsync();
	}

	public async Task UpdateAsync(User user)
	{
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
	}
}

