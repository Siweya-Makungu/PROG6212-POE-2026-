using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.DTOs;
public record RegisterRequest([Required] string FirstName,[Required] string LastName,[Required,EmailAddress] string Email,[Required,MinLength(8)] string Password,[Required] string Role);
public record LoginRequest([Required,EmailAddress] string Email,[Required] string Password);
public record UpdateProfileRequest([Required] string FirstName,[Required] string LastName,string? Phone);
public record EventRequest([Required] string EventName,[Required] string Description,DateTime EventDate,[Required] string Location);
public record CategoryRequest([Required] string Name,[Range(0.01,1000)] decimal DistanceKm);
public record EnrollmentRequest([Range(1,int.MaxValue)] int CategoryId);
public record ResultRequest([Range(1,int.MaxValue)] int EnrollmentId,TimeSpan FinishTime,[Range(1,int.MaxValue)] int Position,[Range(0.01,1000)] decimal Pace);
