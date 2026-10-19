using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models;
namespace RaceDay.Api.Services;
public class TokenService(IConfiguration config)
{
 public string Create(User user)
 {
  var key=config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing. Configure user-secrets.");
  if(Encoding.UTF8.GetByteCount(key)<32) throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");
  var claims=new[]{new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),new Claim(ClaimTypes.Email,user.Email),new Claim(ClaimTypes.Role,user.Role)};
  var credentials=new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256);
  return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims:claims,expires:DateTime.UtcNow.AddHours(2),signingCredentials:credentials));
 }
}
