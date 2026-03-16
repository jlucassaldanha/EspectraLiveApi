using Microsoft.EntityFrameworkCore;
using SpectraLiveApi.Models;

namespace SpectraLiveApi.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<User> Users { get; set; }
	public DbSet<UnviewUser> UnviewUsers { get; set; }
}