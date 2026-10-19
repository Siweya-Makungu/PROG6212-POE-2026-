using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[Authorize(Roles="Participant"),Route("api/enrollments")]
public class EnrollmentsController(RaceDayDbContext db):ApiControllerBase
{
 /// <summary>Enrol in a category (Participant only, no duplicate entries).</summary>
 [HttpPost]
 public async Task<IActionResult> Create(EnrollmentRequest request)
 {
  if(!await db.Categories.AnyAsync(c=>c.Id==request.CategoryId))return NotFound("Category not found.");
  if(await db.Enrollments.AnyAsync(e=>e.CategoryId==request.CategoryId&&e.ParticipantId==CurrentUserId))return Conflict("Already enrolled in this category.");
  var entry=new Enrollment{ParticipantId=CurrentUserId,CategoryId=request.CategoryId};db.Enrollments.Add(entry);await db.SaveChangesAsync();return StatusCode(201,new{entry.Id,entry.ParticipantId,entry.CategoryId,entry.EnrolledAt});
 }
 /// <summary>See your own enrolments and any recorded results.</summary>
 [HttpGet("me")]
 public async Task<IActionResult> Mine()=>Ok(await db.Enrollments.Where(e=>e.ParticipantId==CurrentUserId).Select(e=>new{e.Id,e.CategoryId,Category=e.Category!.Name,Event=e.Category.Event!.EventName,e.EnrolledAt,Result=e.Result==null?null:new{e.Result.Position,e.Result.FinishTime,e.Result.Pace}}).ToListAsync());
}
