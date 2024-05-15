using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController]
public class UserController(
    ILogger<UserController> logger,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : ControllerBase
{
    [HttpGet("me", Name = "Me"), Authorize]
    public async Task<IActionResult> Get(ApplicationDbContext context)
    {
        var id = User.GetUserId();
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        var user = context.Users.Select(u => new
            {
                u.Id, u.UserName, u.Email, Roles = roles
            })
            .FirstOrDefault(u => u.Id == id);
        if (user == null) return Unauthorized();
        
        /*Dictionary<string, object?> userInfo = new();
        userInfo[nameof(user.Id)] = user.Id;
        userInfo[nameof(user.UserName)] = user.UserName;
        userInfo[nameof(user.Email)] = user.Email;
        userInfo[nameof(user.Roles)] = user.Roles;*/
        if (User.IsInRole(Roles.Visitor))
        {
            var visitor = await context.Visitors.FirstAsync(v => v.UserId == id);
            return Ok(new { user.Id, user.UserName, user.Email, user.Roles, Visitor = visitor });
        }
        if (User.IsInRole(Roles.Employee))
        {
            var specialist = await context.Specialists.FirstAsync(v => v.UserId == id);
            return Ok(new { user.Id, user.UserName, user.Email, user.Roles, Specialist = specialist });
        }
        return Ok(user);
    }

    [HttpPost("logout", Name = "Logout"), Authorize]
    public async Task<IActionResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    private ObjectResult ProblemFromIdentityError(IEnumerable<IdentityError> errors) => Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: string.Join(", ", errors.Select(e => e.Description))
    );

    [HttpPost("register-visitor")]
    public async Task<IActionResult> RegisterVisitor(VisitorRegisterModel model)
    {
        var user = new ApplicationUser
        {
            Email = model.Email!,
            UserName = model.Email!
        };

        var identityResult = await userManager.CreateAsync(user, model.Password!);
        if (!identityResult.Succeeded) return ProblemFromIdentityError(identityResult.Errors);

        identityResult = await userManager.AddToRoleAsync(user, Roles.Visitor);
        if (!identityResult.Succeeded) return ProblemFromIdentityError(identityResult.Errors);

        Visitor visitor = new()
        {
            User = user,
            FirstName = model.FirstName!,
            LastName = model.LastName!,
            Patronymic = model.Patronymic,
            Type = model.Type!.Value,
            Birthday = model.Birthday!.Value
        };

        context.Visitors.Add(visitor);
        await context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        // var user = await userManager.GetUserAsync(User); // ?
        var id = User.GetUserId();
        var user = context.Users.First(u => u.Id == id);

        var identityResult = await userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);
        if (!identityResult.Succeeded) return ProblemFromIdentityError(identityResult.Errors);

        return Ok();
    }

    /*[Authorize(Roles.Admin)]
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
    }*/
}

/*public record UserPostModel(string Email, string Password, string[]? Roles);

public record UserPutModel(string Email, string[]? Roles);*/

public record ChangePasswordModel(string? OldPassword, string? NewPassword);

public class ChangePasswordModelValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordModelValidator()
    {
        RuleFor(user => user.OldPassword).NotNull().NotEmpty().MinimumLength(6);
        RuleFor(user => user.NewPassword).NotNull().NotEmpty().MinimumLength(6);
    }
}
