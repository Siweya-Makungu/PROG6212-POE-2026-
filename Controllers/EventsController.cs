using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[Route("api/events")]
public class EventsController(RaceDayDbContext db):ApiControllerBase
{
 /// <summary>Browse all RaceDay events and their categories.</summary>
 [HttpGet]
 public async Task<IActionResult> All()=>Ok(await db.Events.Include(e=>e.Categories).AsNoTracking().Select(e=>new{e.Id,e.EventName,e.Description,e.EventDate,e.Location,e.OrganiserId,Categories=e.Categories.Select(c=>new{c.Id,c.Name,c.DistanceKm})}).ToListAsync());
 /// <summary>View one event by ID.</summary>
 [HttpGet("{id:int}")]
 public async Task<IActionResult> ById(int id)
 {
  var e=await db.Events.Include(x=>x.Categories).AsNoTracking().Where(x=>x.Id==id).Select(x=>new{x.Id,x.EventName,x.Description,x.EventDate,x.Location,x.OrganiserId,Categories=x.Categories.Select(c=>new{c.Id,c.Name,c.DistanceKm})}).FirstOrDefaultAsync();return e==null?NotFound():Ok(e);
 }
 /// <summary>Create an event (Organiser only).</summary>
 [Authorize(Roles="Organiser"),HttpPost]
 public async Task<IActionResult> Create(EventRequest request)
 {
  var e=new RaceEvent{EventName=request.EventName,Description=request.Description,EventDate=request.EventDate,Location=request.Location,OrganiserId=CurrentUserId};
  db.Events.Add(e);await db.SaveChangesAsync();return CreatedAtAction(nameof(ById),new{id=e.Id},new{e.Id,e.EventName,e.Description,e.EventDate,e.Location});
 }
 /// <summary>Edit your own event (Organiser only).</summary>
 [Authorize(Roles="Organiser"),HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id,EventRequest request)
 {
  var e=await db.Events.FindAsync(id);if(e==null)return NotFound();if(e.OrganiserId!=CurrentUserId)return Forbid();
  e.EventName=request.EventName;e.Description=request.Description;e.EventDate=request.EventDate;e.Location=request.Location;await db.SaveChangesAsync();return Ok(e);
 }
 /// <summary>Delete your own event if it has no enrolments.</summary>
 [Authorize(Roles="Organiser"),HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
  var e=await db.Events.FindAsync(id);if(e==null)return NotFound();if(e.OrganiserId!=CurrentUserId)return Forbid();
  if(await db.Enrollments.AnyAsync(x=>x.Category!.EventId==id))return Conflict("Event has enrolments and cannot be deleted.");
  db.Events.Remove(e);await db.SaveChangesAsync();return NoContent();
 }
 /// <summary>Add a distance category to your event.</summary>
 [Authorize(Roles="Organiser"),HttpPost("{id:int}/categories")]
 public async Task<IActionResult> AddCategory(int id,CategoryRequest request)
 {
  var e=await db.Events.FindAsync(id);if(e==null)return NotFound();if(e.OrganiserId!=CurrentUserId)return Forbid();
  var c=new EventCategory{EventId=id,Name=request.Name,DistanceKm=request.DistanceKm};db.Categories.Add(c);await db.SaveChangesAsync();return StatusCode(201,new{c.Id,c.EventId,c.Name,c.DistanceKm});
 }
 /// <summary>View the categories for an event.</summary>
 [HttpGet("{id:int}/categories")]
 public async Task<IActionResult> Categories(int id)
 {
  if(!await db.Events.AnyAsync(e=>e.Id==id))return NotFound();
  return Ok(await db.Categories.Where(c=>c.EventId==id).Select(c=>new{c.Id,c.Name,c.DistanceKm}).ToListAsync());
 }
 /// <summary>Update a category belonging to your event.</summary>
 [Authorize(Roles="Organiser"),HttpPut("{id:int}/categories/{categoryId:int}")]
 public async Task<IActionResult> UpdateCategory(int id,int categoryId,CategoryRequest request)
 {
  var c=await db.Categories.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==categoryId && x.EventId==id);if(c==null)return NotFound();if(c.Event!.OrganiserId!=CurrentUserId)return Forbid();
  c.Name=request.Name;c.DistanceKm=request.DistanceKm;await db.SaveChangesAsync();return Ok(new{c.Id,c.EventId,c.Name,c.DistanceKm});
 }
 /// <summary>Delete an unused category belonging to your event.</summary>
 [Authorize(Roles="Organiser"),HttpDelete("{id:int}/categories/{categoryId:int}")]
 public async Task<IActionResult> DeleteCategory(int id,int categoryId)
 {
  var c=await db.Categories.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==categoryId&&x.EventId==id);if(c==null)return NotFound();if(c.Event!.OrganiserId!=CurrentUserId)return Forbid();
  if(await db.Enrollments.AnyAsync(x=>x.CategoryId==categoryId))return Conflict("Category already has enrolments.");db.Categories.Remove(c);await db.SaveChangesAsync();return NoContent();
 }
 /// <summary>View published results for an event.</summary>
 [HttpGet("{id:int}/results")]
 public async Task<IActionResult> Results(int id)
 {
  if(!await db.Events.AnyAsync(e=>e.Id==id))return NotFound();
  var results=await db.Results.Where(r=>r.Enrollment!.Category!.EventId==id).OrderBy(r=>r.Position).Select(r=>new{r.Id,r.EnrollmentId,r.Position,r.FinishTime,r.Pace,Participant=r.Enrollment!.Participant!.FirstName+" "+r.Enrollment.Participant.LastName,Category=r.Enrollment.Category!.Name}).ToListAsync();return Ok(results);
 }
 /// <summary>View enrolments for your event (Organiser only).</summary>
 [Authorize(Roles="Organiser"),HttpGet("{id:int}/enrollments")]
 public async Task<IActionResult> Enrollments(int id)
 {
  var e=await db.Events.FindAsync(id);if(e==null)return NotFound();if(e.OrganiserId!=CurrentUserId)return Forbid();
  return Ok(await db.Enrollments.Where(x=>x.Category!.EventId==id).Select(x=>new{x.Id,x.ParticipantId,Participant=x.Participant!.FirstName+" "+x.Participant.LastName,x.CategoryId,Category=x.Category!.Name,x.EnrolledAt}).ToListAsync());
 }
}
