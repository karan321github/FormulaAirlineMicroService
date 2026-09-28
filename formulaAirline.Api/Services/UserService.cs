using formulaAirline.Api.Model;
using formulaAirline.Api.Repository;

namespace formulaAirline.Api.Services
{
    public class UserService : IUserService
    {
        public readonly IRepository<User> _userRepository;
        public readonly ILogger<UserService> _logger;

        public UserService(IRepository<User> userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        public async Task<User?> CreateUserAsync(User user)
        {
            try
            {
                const string defaultRole = "User";
                user.Role = defaultRole;
                await _userRepository.AddAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a user.");
                throw new Exception("An error occurred while creating a user.", ex);
            }
        }

        public async Task<User> DeleteUserAsync(User user)
        {
            try
            {
                var users = await _userRepository.FindAsync(u => u.Id == user.Id);
                var deletedUser = users.FirstOrDefault();
                if (deletedUser == null)
                {
                    throw new Exception("User not found.");
                }
                await _userRepository.DeleteAsync(deletedUser);
                return deletedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a user.");
                throw new Exception("An error occurred while deleting a user.", ex);
            }


        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return _userRepository.FindAsync(u => u.Email == email)
                               .ContinueWith(task => task.Result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a user by email.");
                throw new Exception("An error occurred while retrieving a user by email.", ex);
            }

        }

        public Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                return _userRepository.FindAsync(u => u.Id == id)
              .ContinueWith(task => task.Result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a user by ID.");
                throw new Exception("An error occurred while retrieving a user by ID.", ex);
            }
          
        }

        public async Task<User?> UpdateUserAsync(string id)
        {
            try
            {
                var existingUserTask = await _userRepository.FindAsync(u => u.Id == id);
                if (existingUserTask.FirstOrDefault() == null)
                {
                    throw new Exception("User not found.");
                }
                await _userRepository.UpdateAsync(existingUserTask.FirstOrDefault());
                return existingUserTask.FirstOrDefault();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a user.");
                throw new Exception("An error occurred while updating a user.", ex);
            }
            
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            try
            {
                return await _userRepository.FindAsync(_ => true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
                throw new Exception("An error occurred while retrieving all users.", ex);
            }
            
        }

    }
}
