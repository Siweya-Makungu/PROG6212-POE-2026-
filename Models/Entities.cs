using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class User
{
 public int Id { get; set; }
 [MaxLength(80)] public string FirstName { get; set; } = "";
 [MaxLength(80)] public string LastName { get; set; } = "";
 [MaxLength(180)] public string Email { get; set; } = "";
 public string PasswordHash { get; set; } = "";
 [MaxLength(30)] public string Role { get; set; } = "Participant";
 [MaxLength(30)] public string? Phone { get; set; }
 public List<Enrollment> Enrollments { get; set; } = new();
}
public class RaceEvent
{
 public int Id { get; set; }
 [MaxLength(150)] public string EventName { get; set; } = "";
 [MaxLength(2000)] public string Description { get; set; } = "";
 public DateTime EventDate { get; set; }
 [MaxLength(180)] public string Location { get; set; } = "";
 public int OrganiserId { get; set; }
 public User? Organiser { get; set; }
 public List<EventCategory> Categories { get; set; } = new();
}
public class EventCategory
{
 public int Id { get; set; }
 public int EventId { get; set; }
 public RaceEvent? Event { get; set; }
 [MaxLength(100)] public string Name { get; set; } = "";
 public decimal DistanceKm { get; set; }
 public List<Enrollment> Enrollments { get; set; } = new();
}
public class Enrollment
{
 public int Id { get; set; }
 public int ParticipantId { get; set; }
 public User? Participant { get; set; }
 public int CategoryId { get; set; }
 public EventCategory? Category { get; set; }
 public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
 public RaceResult? Result { get; set; }
}
public class RaceResult
{
 public int Id { get; set; }
 public int EnrollmentId { get; set; }
 public Enrollment? Enrollment { get; set; }
 public TimeSpan FinishTime { get; set; }
 public int Position { get; set; }
 public decimal Pace { get; set; }
}
