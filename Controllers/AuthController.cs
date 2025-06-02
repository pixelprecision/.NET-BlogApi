// ========================================
// Controllers/AuthController.cs
// ========================================
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBlogApi.Models;
using MyBlogApi.Models.DTOs;
using MyBlogApi.Services;

namespace MyBlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;

        public AuthController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email, // Using email as username
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            // Hash the password and save the user
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            // Generate token for immediate login after registration
            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate token
            var token = _tokenService.CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            });
        }
    }
}