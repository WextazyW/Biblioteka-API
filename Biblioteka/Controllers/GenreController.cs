using Biblioteka.DataBaseContext;
using Biblioteka.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biblioteka.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : Controller
    {
        private readonly TestApiDb _context;

        public GenreController(TestApiDb context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Genre>> GetGenre()
        {
            var genres = await _context.Genres.ToListAsync();
            return Ok(genres); 
        }

        [HttpPost]
        public async Task<ActionResult<Genre>> PostBook([FromBody] Genre genre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(genre, new ValidationContext(genre), validationResults);
            if (!isValid)
            {
                return BadRequest(validationResults.Select(r => r.ErrorMessage).ToArray());
            }

            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, [FromBody] Genre genre)
        {
            if (id != genre.Id)
            {
                return BadRequest(new { Message = "ID жанра не совпадает." });
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(id))
                {
                    return NotFound(new { Message = "Жанр не найден." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при обновлении жанра.");
            }

            return NoContent();
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new { Message = "Жанр не найден." });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
