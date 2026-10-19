using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[Authorize(Roles="Organiser"),Route("api/results")]
public class ResultsController(RaceDayDbContext db):ApiControllerBase
{
 /// <summary>Record a finish time, position and pace for a participant in your event.</summary>
 [HttpPost]
 public async Task<IActionResult> Create(ResultRequest request)
 {
  if(request.FinishTime<=TimeSpan.Zero)return BadRequest("FinishTime must be positive.");
  var entry=await db.Enrollments.Include(e=>e.Category).FirstOrDefaultAsync(e=>e.Id==request.EnrollmentId);if(entry==null)return NotFound("Enrollment not found.");
  if(!await db.Events.AnyAsync(e=>e.Id==entry.Category!.EventId&&e.OrganiserId==CurrentUserId))return Forbid();
  if(await db.Results.AnyAsync(r=>r.EnrollmentId==entry.Id))return Conflict("Result already recorded.");
  var result=new RaceResult{EnrollmentId=entry.Id,FinishTime=request.FinishTime,Position=request.Position,Pace=request.Pace};db.Results.Add(result);await db.SaveChangesAsync();return StatusCode(201,new{result.Id,result.EnrollmentId,result.FinishTime,result.Position,result.Pace});
 }
}
