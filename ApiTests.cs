using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;
namespace RaceDay.Tests;
public class ApiTests
{
 private static RaceDayDbContext Db()=>new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
 private static T As<T>(T controller,int userId,string role) where T:ControllerBase
 {
  controller.ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,userId.ToString()),new Claim(ClaimTypes.Role,role)},"Test"))}};return controller;
 }
 private static AuthController Auth(RaceDayDbContext db)=>new(db,new TokenService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Jwt:Key",new string('a',48)}}).Build()));
 [Fact] public async Task Registration_CreatesParticipantWithHashedPassword()
 {
  using var db=Db();var response=await Auth(db).Register(new("Amy","Runner","amy@test.com","StrongPass1!","Participant"));
  Assert.IsType<ObjectResult>(response);Assert.Equal(201,((ObjectResult)response).StatusCode);Assert.NotEqual("StrongPass1!",db.Users.Single().PasswordHash);
 }
 [Fact] public async Task Registration_RejectsInvalidRole()
 {
  using var db=Db();Assert.IsType<BadRequestObjectResult>(await Auth(db).Register(new("A","B","a@b.com","StrongPass1!","Admin")));
 }
 [Fact] public async Task Login_RejectsIncorrectPassword()
 {
  using var db=Db();await Auth(db).Register(new("A","B","a@b.com","StrongPass1!","Participant"));
  Assert.IsType<UnauthorizedObjectResult>(await Auth(db).Login(new("a@b.com","wrong")));
 }
 [Fact] public async Task Login_SucceedsWithCorrectPassword()
 {
  using var db=Db();await Auth(db).Register(new("A","B","a@b.com","StrongPass1!","Participant"));
  Assert.IsType<OkObjectResult>(await Auth(db).Login(new("a@b.com","StrongPass1!")));
 }
 [Fact] public async Task Organiser_CanCreateEvent()
 {
  using var db=Db();db.Users.Add(new User{Id=1,Role="Organiser"});await db.SaveChangesAsync();
  var controller=As(new EventsController(db),1,"Organiser");
  Assert.IsType<CreatedAtActionResult>(await controller.Create(new("Soweto 10K","Fun run",DateTime.UtcNow.AddDays(10),"Soweto")));
  Assert.Single(db.Events);
 }
 [Fact] public async Task Organiser_CannotEditAnotherOrganisersEvent()
 {
  using var db=Db();db.Events.Add(new RaceEvent{Id=5,OrganiserId=2,EventName="Race"});await db.SaveChangesAsync();
  Assert.IsType<ForbidResult>(await As(new EventsController(db),1,"Organiser").Update(5,new("Changed","Details",DateTime.UtcNow,"Durban")));
 }
 [Fact] public void RoleAttributes_ProtectEventCreationAndEnrolment()
 {
  var create=typeof(EventsController).GetMethod(nameof(EventsController.Create))!;
  var createRole=Assert.Single(create.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),true).Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
  Assert.Equal("Organiser",createRole.Roles);
  var enrollRole=Assert.Single(typeof(EnrollmentsController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),true).Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
  Assert.Equal("Participant",enrollRole.Roles);
 }
 [Fact] public async Task Enrolment_CreatesEntryAndPreventsDuplicates()
 {
  using var db=Db();db.Categories.Add(new EventCategory{Id=10,EventId=3,Name="10 km"});await db.SaveChangesAsync();
  var c=As(new EnrollmentsController(db),7,"Participant");
  Assert.Equal(201,Assert.IsType<ObjectResult>(await c.Create(new(10))).StatusCode);
  Assert.IsType<ConflictObjectResult>(await c.Create(new(10)));
  Assert.Single(db.Enrollments);
 }
 [Fact] public async Task Results_RejectOtherOrganisers()
 {
  using var db=Db();db.Events.Add(new RaceEvent{Id=3,OrganiserId=9});db.Categories.Add(new EventCategory{Id=4,EventId=3});db.Enrollments.Add(new Enrollment{Id=5,CategoryId=4,ParticipantId=2});await db.SaveChangesAsync();
  var c=As(new ResultsController(db),1,"Organiser");Assert.IsType<ForbidResult>(await c.Create(new(5,TimeSpan.FromMinutes(45),1,4.5m)));
 }
}
