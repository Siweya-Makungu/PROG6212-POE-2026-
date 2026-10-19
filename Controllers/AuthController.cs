using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[Route("api/auth")]
public class AuthController(RaceDayDbContext db,TokenService tokens) : ApiControllerBase
{
 private readonly PasswordHasher<User> hasher=new();
 
 [HttpPost("register")]
 public async Task<IActionResult> Register(RegisterRequest request)
 {
  if(!new[]{"Participant","Organiser"}.Contains(request.Role,StringComparer.OrdinalIgnoreCase)) return BadRequest("Role must be Participant or Organiser.");
  var email=request.Email.Trim().ToLowerInvariant();
  if(await db.Users.AnyAsync(x=>x.Email==email)) return Conflict("Email already registered.");
  var user=new User{FirstName=request.FirstName,LastName=request.LastName,Email=email,Role=request.Role.Equals("Organiser",StringComparison.OrdinalIgnoreCase)?"Organiser":"Participant"};
  user.PasswordHash=hasher.HashPassword(user,request.Password);
  db.Users.Add(user);await db.SaveChangesAsync();
  return StatusCode(201,new{user.Id,user.FirstName,user.LastName,user.Email,user.Role});
 }
 
 [HttpPost("login")]
 public async Task<IActionResult> Login(LoginRequest request)
 {
  var user=await db.Users.FirstOrDefaultAsync(x=>x.Email==request.Email.Trim().ToLowerInvariant());
  if(user==null || hasher.VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed) return Unauthorized("Invalid email or password.");
  return Ok(new{token=tokens.Create(user),user.Id,user.Role});
 }
}
