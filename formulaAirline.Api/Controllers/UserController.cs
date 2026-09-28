using AutoMapper;
using formulaAirline.Api.Model;
using formulaAirline.Api.Repository;
using formulaAirline.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace formulaAirline.Api.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IMessageProducer _messageProducer;
        private readonly IRepository<User> _userRepository;
        private readonly IFlightService _flightService;
        private readonly IMapper _mapper;


        public UserController(IUserService userService, ILogger<UserController> logger, IMessageProducer messageProducer, IRepository<User> userRepository, IFlightService flightService, IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _messageProducer = messageProducer;
            _userRepository = userRepository;
            _flightService = flightService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving users: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser?.Id }, createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id);
                if (updatedUser == null)
                {
                    return NotFound($"User with ID {id} not found");
                }
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }
                await _userService.DeleteUserAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
