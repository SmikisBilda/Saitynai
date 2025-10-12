using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class BuildingController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public BuildingController(SaitynaiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all buildings.
        /// </summary>
        /// <returns>List of buildings.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Building>))]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuilding()
        {
            return await _context.Building.ToListAsync();
        }

        /// <summary>
        /// Get a building by id.
        /// </summary>
        /// <param name="id">Building identifier.</param>
        /// <returns>The requested building.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Building>> GetBuilding(int id)
        {
            var building = await _context.Building.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            return building;
        }

        /// <summary>
        /// Get all points under a building.
        /// </summary>
        /// <param name="id">Building identifier.</param>
        /// <returns>List of points for the building (via floors).</returns>
        [HttpGet("{id:int}/points")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Point>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Point>>> GetBuildingPoints(int id)
        {
            var buildingExists = await _context.Building.AnyAsync(b => b.Id == id);
            if (!buildingExists)
            {
                return NotFound();
            }

            var points = await
                (from p in _context.Point
                 join f in _context.Floor on p.FloorId equals f.Id
                 where f.BuildingId == id
                 select p)
                .ToListAsync();

            return points;
        }

        /// <summary>
        /// Create a building.
        /// </summary>
        /// <param name="building">Building payload.</param>
        /// <returns>The created building.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<Building>> PostBuilding(Building building)
        {
            try
            {
                _context.Building.Add(building);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetBuilding", new { id = building.Id }, building);
            }
            // 23505: unique/primary key violation (duplicate id or other unique constraint)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type  = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A building with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }

        /// <summary>
        /// Update a building by id.
        /// </summary>
        /// <param name="id">Building identifier from URL.</param>
        /// <param name="building">Updated building payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBuilding(int id, Building building)
        {
            if (id != building.Id)
            {
                return BadRequest();
            }

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!BuildingExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a building by id.
        /// </summary>
        /// <param name="id">Building identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Building.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            _context.Building.Remove(building);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuildingExists(int id)
        {
            return _context.Building.Any(e => e.Id == id);
        }
    }
}
