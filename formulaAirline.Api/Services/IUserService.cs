using formulaAirline.Api.Model;

namespace formulaAirline.Api.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(string id);
        Task<User> DeleteUserAsync(User user);
    }
}
