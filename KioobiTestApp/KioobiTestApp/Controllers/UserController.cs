using Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.CustomExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KioobiTestApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _usersService;
        public UserController(IUserService usersService)
        {
            _usersService = usersService;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                _usersService.Register(registerUserDto);
                return StatusCode(StatusCodes.Status201Created, "User registered!");
            }
            catch (UserException e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured!");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<string> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                string token = _usersService.Login(loginDto);
                return Ok(token);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured!");
            }
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _usersService.Delete(id);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured!");
            }
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var claims = User.Claims;
                string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                _usersService.Update(updateUserDto, Convert.ToInt32(userId));
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured!");
            }
        }
    }
}
