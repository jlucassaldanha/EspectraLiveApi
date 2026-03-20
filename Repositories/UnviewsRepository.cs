using Microsoft.EntityFrameworkCore;
using SpectraLiveApi.Data;
using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Repositories;

public class UnviewsRepository : IUnviewsRepository
{
	private readonly AppDbContext _db;

	public UnviewsRepository(AppDbContext db)
	{
		_db = db;
	}

	public async Task<List<Unviews>> GetUnviewsByUserIdAsync(string userId)
	{
		return await _db.Unviews
			.Where(u => u.UserId == userId)
			.ToListAsync();
	}
	public async Task AddUnviewsAsync(IEnumerable<Unviews> unviews)
	{
		await _db.Unviews.AddRangeAsync(unviews);
		await _db.SaveChangesAsync();
	}
	public async Task DeleteUnviewsFromUserIdAsync(IEnumerable<string> twitchIds, string userId)
	{
		await _db.Unviews
			.Where(u => u.UserId == userId)
			.Where(u => twitchIds.Contains(u.TwitchId))
			.ExecuteDeleteAsync();
	}
}