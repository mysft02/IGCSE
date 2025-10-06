using BusinessObject.Model;
using Microsoft.AspNetCore.Mvc;
using Repository.IRepositories;

namespace IGCSE.Controller
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;

        public CourseController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // GET: api/course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll()
        {
            var courses = await _courseRepository.GetAllAsync();
            return Ok(courses);
        }

        // GET: api/course/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<Course>> GetById(long id)
        {
            try
            {
                // BaseRepository.GetByIdAsync uses int; add overload logic here
                var course = (await _courseRepository.GetAllAsync()).FirstOrDefault(c => c.Id == id);
                if (course == null)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/course
        [HttpPost]
        public async Task<ActionResult<Course>> Create([FromBody] Course model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            model.CreatedAt = DateTime.UtcNow;
            var created = await _courseRepository.AddAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/course/{id}
        [HttpPut("{id:long}")]
        public async Task<ActionResult<Course>> Update(long id, [FromBody] Course model)
        {
            if (id != model.Id)
            {
                return BadRequest(new { message = "Id mismatch" });
            }
            model.UpdatedAt = DateTime.UtcNow;
            var updated = await _courseRepository.UpdateAsync(model);
            return Ok(updated);
        }

        // DELETE: api/course/{id}
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = (await _courseRepository.GetAllAsync()).FirstOrDefault(c => c.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            await _courseRepository.DeleteAsync(existing);
            return NoContent();
        }
    }
}


