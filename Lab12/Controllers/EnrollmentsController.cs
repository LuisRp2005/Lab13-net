using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab12.Data;
using Lab12.Models;
using Lab12.Response;
using Lab12.Request;

namespace Lab12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly DemoContext _context;

        public EnrollmentsController(DemoContext context)
        {
            _context = context;
        }

        // GET: api/Enrollments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            return await _context.Enrollments.ToListAsync();
        }

        // GET: api/Enrollments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return enrollment;
        }

        // PUT: api/Enrollments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(int id, Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId)
            {
                return BadRequest();
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Enrollments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<Enrollment>> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            var course = await _context.Courses.FindAsync(request.CourseId);

            if (student == null || course == null)
                return BadRequest("Estudiante o curso no encontrado.");

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Date = request.Date
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.EnrollmentId }, enrollment);
        }

        // DELETE: api/Enrollments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("by-course")]
        public async Task<ActionResult<IEnumerable<ResponseEnrollment>>> GetEnrollmentsByCourse([FromBody] RequestEnrollmentv1 request)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.Student.Grade)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.CourseName))
                query = query.Where(e => e.Course.Nombre.Contains(request.CourseName));

            var result = await query
                .OrderBy(e => e.Course.Nombre)
                .ThenBy(e => e.Student.LastName)
                .Select(e => new ResponseEnrollment
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentFirstName = e.Student.FirstName,
                    StudentLastName = e.Student.LastName,
                    StudentEmail = e.Student.Email,
                    CourseName = e.Course.Nombre,
                    GradeName = e.Student.Grade.Nombre,
                    Date = e.Date
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("by-grade")]
        public async Task<ActionResult<IEnumerable<ResponseEnrollment>>> GetEnrollmentsByGrade([FromBody] RequestEnrollmentv2 request)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Grade)
                .Include(e => e.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.GradeName))
                query = query.Where(e => e.Student.Grade.Nombre.Contains(request.GradeName));

            var result = await query
                .OrderBy(e => e.Course.Nombre)
                .ThenBy(e => e.Student.LastName)
                .Select(e => new ResponseEnrollment
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentFirstName = e.Student.FirstName,
                    StudentLastName = e.Student.LastName,
                    StudentEmail = e.Student.Email,
                    CourseName = e.Course.Nombre,
                    GradeName = e.Student.Grade.Nombre,
                    Date = e.Date
                })
                .ToListAsync();

            return Ok(result);
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentId == id);
        }
    }
}
