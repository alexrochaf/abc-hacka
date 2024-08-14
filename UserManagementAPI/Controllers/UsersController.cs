namespace UserManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;
    private readonly ITokenService _tokenService;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _logger = logger;
        _tokenService = tokenService;

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all users");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting user with id {id}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            var createdUser = await _userRepository.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a new user");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        try
        {
            await _userRepository.UpdateUserAsync(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while updating user with id {id}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var exists = await _userRepository.UserExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while deleting user with id {id}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateToken([FromBody] UserLoginModel model)
    {
        try
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid login model");
            }

            var user = await _userRepository.GetUserByUsernameAsync(model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }

            var tokenResult = await _tokenService.GenerateTokenAsync(user);
            if (tokenResult.IsSuccess)
            {
                return Ok(new { token = tokenResult.Token, expires = tokenResult.Expires });
            }
            else
            {
                _logger.LogError("Token generation failed: {ErrorMessage}", tokenResult.ErrorMessage);
                return StatusCode(500, "An error occurred while generating the token.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating token");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}