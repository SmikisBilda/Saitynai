using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;
using Saitynai.Authorization; 
using Microsoft.AspNetCore.Authorization; 

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class FloorController : ControllerBase
    {
        private readonly PostgresContext _context;

        public FloorController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Upload a floor plan image.
        /// </summary>
        /// <param name="id">Floor identifier.</param>
        /// <param name="file">The image file.</param>
        /// <returns>The updated floor with the new path.</returns>
        [HttpPost("{id}/upload-plan")]
        [PermissionAuthorize("edit", "Floor")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Floor))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Floor>> UploadFloorPlan(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var floor = await _context.Floor.FindAsync(id);
            if (floor == null)
            {
                return NotFound();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only images are allowed.");
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "floorplans");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileName = $"floor_{id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Delete old file if exists
            if (!string.IsNullOrEmpty(floor.FloorPlanPath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), floor.FloorPlanPath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update floor with new path
            floor.FloorPlanPath = $"/uploads/floorplans/{fileName}";
            await _context.SaveChangesAsync();

            return floor;
        }

        /// <summary>
        /// Get all floors.
        /// </summary>
        /// <returns>List of floors.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Floor>))]
        public async Task<ActionResult<IEnumerable<Floor>>> GetFloor()
        {
            return await _context.Floor.ToListAsync();
        }

        /// <summary>
        /// Get a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier.</param>
        /// <returns>The requested floor.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Floor))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Floor>> GetFloor(int id)
        {
            var floor = await _context.Floor.FindAsync(id);

            if (floor == null)
            {
                return NotFound();
            }

            return floor;
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="floor">Floor payload.</param>
        /// <returns>The created floor.</returns>
        [HttpPost]
        [PermissionAuthorize("create", "Floor")] 
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Floor))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<Floor>> PostFloor(Floor floor)
        {
            try
            {
                _context.Floor.Add(floor);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetFloor", new { id = floor.Id }, floor);
            }
            // Unique violation => 409 Conflict
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A floor with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
            // FK violation => 409 Conflict
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced building_id does not exist."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }

        /// <summary>
        /// Update a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier from URL.</param>
        /// <param name="floor">Updated floor payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [PermissionAuthorize("edit", "Floor")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutFloor(int id, Floor floor)
        {
            var existingFloor = await _context.Floor.FindAsync(id);
            if (existingFloor == null)
            {
                return NotFound();
            }

            // Update only the properties provided in the request
            if (floor.BuildingId != 0)
            {
                existingFloor.BuildingId = floor.BuildingId;
            }
            if (floor.FloorNumber != 0)
            {
                existingFloor.FloorNumber = floor.FloorNumber;
            }
            if (floor.FloorPlanPath != null)
            {
                existingFloor.FloorPlanPath = floor.FloorPlanPath;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FloorExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Get all floors for a building.
        /// </summary>
        /// <param name="buildingId">Building identifier.</param>
        /// <returns>List of floors for the specified building.</returns>
        [HttpGet("building/{buildingId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Floor>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Floor>>> GetFloorsByBuilding(int buildingId)
        {
            var buildingExists = await _context.Building.AnyAsync(b => b.Id == buildingId);
            if (!buildingExists)
            {
                return NotFound();
            }

            var floors = await _context.Floor
                .Where(f => f.BuildingId == buildingId)
                .ToListAsync();

            return floors;
        }

        /// <summary>
        /// Delete a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [PermissionAuthorize("delete", "Floor")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var floor = await _context.Floor.FindAsync(id);
            if (floor == null)
            {
                return NotFound();
            }

            _context.Floor.Remove(floor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FloorExists(int id)
        {
            return _context.Floor.Any(e => e.Id == id);
        }
    }
}
