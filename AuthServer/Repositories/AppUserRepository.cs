using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly AppDbContext _dbContext;

        public AppUserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AppUser?> FindByUsernameAsync(string username)
        {
            return await _dbContext.AppUsers.FirstOrDefaultAsync((AppUser x) => x.Username.Equals(username.Trim().ToLower()));
        }
    }
}
