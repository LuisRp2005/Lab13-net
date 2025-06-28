using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab12.Data;
using Lab12.Models;
using Lab12.Request;
using Lab12.Response;

namespace Lab12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly DemoContext _context;

        public StudentsController(DemoContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.StudentId)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent([FromBody] CreateStudentRequest request)
        {
            var grade = await _context.Grades.FindAsync(request.GradeId);
            if (grade == null)
            {
                return BadRequest("El grado especificado no existe.");
            }

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Email = request.Email,
                Grade = grade
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<ResponseEstudentv1>>> SearchStudents([FromBody] RequestStudentv1 request)
        {
            var query = _context.Students.Include(s => s.Grade).AsQueryable();

            if (!string.IsNullOrEmpty(request.FirstName))
                query = query.Where(s => s.FirstName.Contains(request.FirstName));

            if (!string.IsNullOrEmpty(request.LastName))
                query = query.Where(s => s.LastName.Contains(request.LastName));

            if (!string.IsNullOrEmpty(request.Email))
                query = query.Where(s => s.Email.Contains(request.Email));

            var result = await query
                .OrderByDescending(s => s.LastName)
                .Select(s => new ResponseEstudentv1
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Grade = s.Grade.Nombre
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("by-grade")]
        public async Task<ActionResult<IEnumerable<ResponseEstudentv1>>> GetStudentsByGrade([FromBody] RequestStudentv2 request)
        {
            var query = _context.Students.Include(s => s.Grade).AsQueryable();

            if (!string.IsNullOrEmpty(request.FirstName))
                query = query.Where(s => s.FirstName.Contains(request.FirstName));

            if (!string.IsNullOrEmpty(request.GradeName))
                query = query.Where(s => s.Grade.Nombre.Contains(request.GradeName));

            var result = await query
                .OrderByDescending(s => s.Grade.Nombre)
                .Select(s => new ResponseEstudentv1
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Grade = s.Grade.Nombre
                })
                .ToListAsync();

            return Ok(result);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}
