using Biblioteka.DataBaseContext;
using Biblioteka.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biblioteka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : Controller
    {
        private readonly TestApiDb _context;
        public RentalController(TestApiDb context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Rental>> GetRental()
        {
            var rental = await _context.Rental.ToListAsync();
            return Ok(rental);
        }

        [HttpGet("status")]
        public async Task<ActionResult<Rental>> GetBookByStatus(string status)
        {
            var nameParts = status.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var rental = await _context.Rental.Where(i => nameParts.All(part => i.status_rental.ToLower().Contains(part))).ToListAsync();

            return Ok(rental);
        }

        [HttpGet("reader")]
        public async Task<ActionResult<Rental>> GetBooksByReader(int id)
        {
            var rental = await _context.Readers.FirstOrDefaultAsync(g => g.Id == id);
            if (rental == null)
            {
                return BadRequest($"not found rental with id {id}");
            }
            return Ok(_context.Rental.Where(i => i.reader_id == id));
        }
        [HttpGet("book")]
        public async Task<ActionResult<Rental>> GetBooksByBook(int id)
        {
            var rental = await _context.Books.FirstOrDefaultAsync(g => g.Id == id);
            if (rental == null)
            {
                return BadRequest($"not found rental with id {id}");
            }
            return Ok(_context.Rental.Where(i => i.books_id== id));
        }

        [HttpPost]
        public async Task<ActionResult<Rental>> PostBook([FromBody] Rental rental)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(rental, new ValidationContext(rental), validationResults);
            if (!isValid)
            {
                return BadRequest(validationResults.Select(r => r.ErrorMessage).ToArray());
            }

            await _context.Rental.AddAsync(rental);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRental(int id, [FromBody] Rental rental)
        {
            if (id != rental.Id)
            {
                return BadRequest(new { Message = "ID аренды не совпадает." });
            }

            _context.Entry(rental).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RentalExists(id))
                {
                    return NotFound(new { Message = "Аренда не найдена." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при обновлении Аренды.");
            }

            return NoContent();
        }
        private bool RentalExists(int id)
        {
            return _context.Rental.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRental(int id)
        {

            var rental = await _context.Rental.FindAsync(id);
            if (rental == null)
            {
                return NotFound(new { Message = "Книга не найдена." });
            }

            _context.Rental.Remove(rental);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
