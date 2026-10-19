using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
 public DbSet<User> Users => Set<User>();
 public DbSet<RaceEvent> Events => Set<RaceEvent>();
 public DbSet<EventCategory> Categories => Set<EventCategory>();
 public DbSet<Enrollment> Enrollments => Set<Enrollment>();
 public DbSet<RaceResult> Results => Set<RaceResult>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  b.Entity<User>().HasIndex(x=>x.Email).IsUnique();
  b.Entity<RaceEvent>().HasOne(x=>x.Organiser).WithMany().HasForeignKey(x=>x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<EventCategory>().HasOne(x=>x.Event).WithMany(x=>x.Categories).HasForeignKey(x=>x.EventId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Enrollment>().HasOne(x=>x.Participant).WithMany(x=>x.Enrollments).HasForeignKey(x=>x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Enrollment>().HasOne(x=>x.Category).WithMany(x=>x.Enrollments).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Enrollment>().HasIndex(x=>new{x.ParticipantId,x.CategoryId}).IsUnique();
  b.Entity<RaceResult>().HasOne(x=>x.Enrollment).WithOne(x=>x.Result).HasForeignKey<RaceResult>(x=>x.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<RaceResult>().HasIndex(x=>x.EnrollmentId).IsUnique();
  b.Entity<EventCategory>().Property(x=>x.DistanceKm).HasPrecision(8,2);
  b.Entity<RaceResult>().Property(x=>x.Pace).HasPrecision(8,2);
 }
}
