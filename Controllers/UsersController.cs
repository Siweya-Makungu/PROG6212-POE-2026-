using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
namespace RaceDay.Api.Controllers;
[Authorize,Route("api/users")]
public class UsersController(RaceDayDbContext db):ApiControllerBase
{
 /// <summary>Get the logged-in user's profile.</summary>
 [HttpGet("me")]
 public async Task<IActionResult> Me(){var u=await db.Users.FindAsync(CurrentUserId);return u==null?NotFound():Ok(new{u.Id,u.FirstName,u.LastName,u.Email,u.Phone,u.Role});}
 /// <summary>Update the logged-in user's personal details.</summary>
 [HttpPut("me")]
 public async Task<IActionResult> Update(UpdateProfileRequest request)
 {
  var u=await db.Users.FindAsync(CurrentUserId);if(u==null)return NotFound();
  u.FirstName=request.FirstName;u.LastName=request.LastName;u.Phone=request.Phone;
  await db.SaveChangesAsync();return Ok(new{u.Id,u.FirstName,u.LastName,u.Email,u.Phone,u.Role});
 }
}
