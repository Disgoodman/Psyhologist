using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;

namespace Psychologist.Server.Controllers;

[ApiController]
public class UserController(
    ILogger<UserController> logger,
    UserManager<ApplicationUser> userManager)
    : ControllerBase
{
    [HttpGet("me", Name = "Me"), Authorize]
    public IActionResult Get(ApplicationDbContext context)
    {
        var id = User.GetUserId();
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        var user = context.Users.Select(u => new
            {
                u.Id, u.UserName, u.Email, Roles = roles
            })
            .FirstOrDefault(u => u.Id == id);
        return Ok(user);
    }

    [HttpPost("logout", Name = "Logout"), Authorize]
    public async Task<IActionResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    [Authorize(Roles.Admin)]
    [HttpGet("user", Name = "GetUsers")]
    public async Task<IActionResult> GetUsers(ApplicationDbContext context)
    {
        var users = from user in context.Users
            join userRole in context.UserRoles on user.Id equals userRole.UserId
            join role in context.Roles on userRole.RoleId equals role.Id
            group role by new { user.Id, user.Email }
            into g
            select new
            {
                g.Key.Id,
                g.Key.Email,
                Roles = g.Select(r => r.Name).ToArray()
            };

        var array = users.ToArray();
        return Ok(array);
    }

    [Authorize(Roles.Admin)]
    [HttpPost("user", Name = "CreateUser")]
    public async Task<IActionResult> CreateUser(UserPostModel body)
    {
        var user = new ApplicationUser
        {
            Email = body.Email,
            UserName = body.Email
        };

        var identityResult = await userManager.CreateAsync(user, body.Password);
        if (!identityResult.Succeeded)
        {
            return BadRequest(identityResult.Errors);
        }

        if (body.Roles != null)
        {
            var roles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, roles);
            var userCreateRolesResult = await userManager.AddToRolesAsync(user, body.Roles);
            if (!userCreateRolesResult.Succeeded)
            {
                return BadRequest(userCreateRolesResult.Errors);
            }
        }


        return Ok();
    }

    [Authorize(Roles.Admin)]
    [HttpPut("user/{id:int}", Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUser(int id, UserPutModel body)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.Email = body.Email;
        user.UserName = body.Email;
        var userUpdateResult = await userManager.UpdateAsync(user);
        if (!userUpdateResult.Succeeded)
        {
            return BadRequest(userUpdateResult.Errors);
        }

        if (body.Roles != null)
        {
            var roles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, roles);
            var userUpdateRolesResult = await userManager.AddToRolesAsync(user, body.Roles);
            if (!userUpdateRolesResult.Succeeded)
            {
                return BadRequest(userUpdateRolesResult.Errors);
            }
        }

        return Ok();
    }

    [Authorize(Roles.Admin)]
    [HttpDelete("user/{id:int}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userDeleteResult = await userManager.DeleteAsync(user);
        if (!userDeleteResult.Succeeded)
        {
            return BadRequest(userDeleteResult.Errors);
        }

        return Ok();
    }
}

public record UserPostModel(string Email, string Password, string[]? Roles);

public record UserPutModel(string Email, string[]? Roles);