using Biblioteka.DataBaseContext;
using Biblioteka.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biblioteka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        private readonly TestApiDb _context;

        public BooksController(TestApiDb context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Books>> GetBooks()
        {
            var books = await _context.Books.ToListAsync();
            return Ok(books); 
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Books>> GetBook(int id)
        { 
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Книга не найдена." });
            }
            return Ok(book); 
        }   

        [HttpGet("genre")]
        public async Task<ActionResult<Books>> GetBooksByGenre(int id)
        {
            var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null)
            {
                return BadRequest($"not found genre with id {id}");
            }
            return Ok(_context.Books.Where(i => i.Genre_id == id));
        }

        [HttpGet("author")]
        public async Task<ActionResult<Books>> GetBookByAuthor(string author)
        {
            var nameParts = author.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var books = await _context.Books.Where(i => nameParts.All(part => i.Author.ToLower().Contains(part))).ToListAsync();

            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<Books>> PostBook([FromBody] Books book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(book, new ValidationContext(book), validationResults);
            if (!isValid)
            {
                return BadRequest(validationResults.Select(r => r.ErrorMessage).ToArray());
            }

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, [FromBody] Books book)
        {
            if (id != book.Id)
            {
                return BadRequest(new { Message = "ID книги не совпадает." });
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound(new { Message = "Книга не найдена." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при обновлении книги.");
            }

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Книга не найдена." });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        [HttpGet("{id}/availability")]
        public async Task<ActionResult<int>> GetAvailableCopies(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return book.AvailableCopies;
        }
    }
}
