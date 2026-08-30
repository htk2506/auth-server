using AuthServer.Database.Models;

namespace AuthServer.Repositories.Interfaces
{
    /// <summary>
    /// Handles data storage operations for <see cref="AppUser"/> objects.
    /// </summary>
    public interface IAppUserRepository
    {
        /// <summary>
        /// Finds an <see cref="AppUser"/> by their username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The <see cref="AppUser"/> if they were found and null otherwise.</returns>
        Task<AppUser?> FindByUsernameAsync(string username);
    }
}
