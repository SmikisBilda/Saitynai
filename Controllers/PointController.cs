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
    public class PointController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public PointController(SaitynaiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all points.
        /// </summary>
        /// <returns>List of points.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Point>))]
        public async Task<ActionResult<IEnumerable<Point>>> GetPoint()
        {
            return await _context.Point.ToListAsync();
        }

        /// <summary>
        /// Get a point by id.
        /// </summary>
        /// <param name="id">Point identifier.</param>
        /// <returns>The requested point.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Point))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Point>> GetPoint(int id)
        {
            var point = await _context.Point.FindAsync(id);

            if (point == null)
            {
                return NotFound();
            }

            return point;
        }

        /// <summary>
        /// Get all points for a floor.
        /// </summary>
        /// <param name="floorId">Floor identifier.</param>
        /// <returns>List of points for the specified floor.</returns>
        [HttpGet("floor/{floorId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Point>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Point>>> GetFloorsByFloor(int floorId)
        {
            var floorExists = await _context.Floor.AnyAsync(b => b.Id == floorId);
            if (!floorExists)
            {
                return NotFound();
            }

            var points = await _context.Point
                .Where(f => f.FloorId == floorId)
                .ToListAsync();

            return points;
        }

        /// <summary>
        /// Create a point.
        /// </summary>
        /// <param name="point">Point payload.</param>
        /// <returns>The created point.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Point))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<Point>> PostPoint(Point point)
        {
            try
            {
                _context.Point.Add(point);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPoint", new { id = point.Id }, point);
            }
            // 23505: unique/primary key violation (duplicate)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A point with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
            // 23503: foreign key violation (e.g., floor_id points to a missing Floor)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced floor_id does not exist."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }

        /// <summary>
        /// Delete a point by id.
        /// </summary>
        /// <param name="id">Point identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePoint(int id)
        {
            var point = await _context.Point.FindAsync(id);
            if (point == null)
            {
                return NotFound();
            }

            _context.Point.Remove(point);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PointExists(int id)
        {
            return _context.Point.Any(e => e.Id == id);
        }
    }
}
