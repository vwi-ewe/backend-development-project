using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Models;

namespace UserManagementApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        // In-memory data store for demonstration purposes.
        private static readonly List<User> Users = new()
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@techhive.com", Department = "IT" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@techhive.com", Department = "HR" }
        };

        private static int _nextId = 3;

        // GET: api/users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            try
            {
                return Ok(Users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving users.");
            }
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            try
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving the user.");
            }
        }

        // POST: api/users
        [HttpPost]
        public ActionResult<User> CreateUser(User newUser)
        {
            try
            {
                if (newUser == null)
                {
                    return BadRequest("User data is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (Users.Any(u => string.Equals(u.Email, newUser.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict($"A user with email '{newUser.Email}' already exists.");
                }

                newUser.Id = _nextId++;
                Users.Add(newUser);

                return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the user.");
            }
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, User updatedUser)
        {
            try
            {
                if (updatedUser == null)
                {
                    return BadRequest("User data is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                if (Users.Any(u => u.Id != id && string.Equals(u.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict($"A user with email '{updatedUser.Email}' already exists.");
                }

                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Email = updatedUser.Email;
                user.Department = updatedUser.Department;

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the user.");
            }
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                Users.Remove(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the user.");
            }
        }
    }
}
