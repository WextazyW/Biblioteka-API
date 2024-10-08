using Biblioteka.DataBaseContext;
using Biblioteka.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biblioteka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReaderController : Controller
    {
        private readonly TestApiDb _context;
        public ReaderController(TestApiDb context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Readers>> GetReader()
        {
            var readers = await _context.Readers.ToListAsync();
            return Ok(readers);
        }

        [HttpPost]
        public async Task<ActionResult<Readers>> PostReader(Readers readers)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(readers, new ValidationContext(readers), validationResults);
            if (!isValid)
            {
                return BadRequest(validationResults.Select(r => r.ErrorMessage).ToArray());
            }

            await _context.Readers.AddAsync(readers);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReader), new { id = readers.Id }, readers);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Readers>> GetReader(int id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
            {
                return NotFound(new { Message = "Читатель не найден." });
            }
            return Ok(reader);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Readers reader)
        {
            if (id != reader.Id)
            {
                return BadRequest(new { Message = "ID читателя не совпадает." });
            }

            _context.Entry(reader).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReaderExists(id))
                {
                    return NotFound(new { Message = "Читатель не найден." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при обновлении читателя.");
            }

            return NoContent();
        }
        private bool ReaderExists(int id)
        {
            return _context.Readers.Any(e => e.Id == id);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReader(int id)
        {

            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
            {
                return NotFound(new { Message = "Читатель не найден." });
            }

            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("books")]
        public async Task<ActionResult<Books>> GetBooksByReader(int id)
        {
            var book = await _context.Readers.FirstOrDefaultAsync(g => g.Id == id);
            if (book == null)
            {
                return BadRequest($"not found book with id {id}");
            }
            return Ok(_context.Books.Where(i => i.Readers_id == id));
        }
    }
}
