using LibraryManagementSystem.API.Helpers;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DBModel;
using Models.DTOModel;
using Models.DTOModel.Book;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;

        }

        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _context.Book.ToListAsync();
                return Ok(books);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpGet("GetBookById/{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            try
            {
                var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == id);
                if (book == null)
                {
                    return NotFound(
                        new
                        {
                            Message = $"Book with id {id} not found"
                        }
                        );
                }
                return Ok(book);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPost("CreateBook")]

        public async Task<IActionResult> CreateBook(AddBookDTO bookDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var book = new Book
                {
                    Name = bookDto.Name,
                    Description = bookDto.Description,
                    Price = bookDto.Price,
                    Author = bookDto.Author,
                    Status = Status.Active,
                    BookAddedDate = DateTime.Now,
                    BookCreationDate = DateOnly.FromDateTime(DateTime.Now),
                    QRCode = bookDto.QRCode,


                };
                if (bookDto.Image != null)
                {
                    book.ProfileImage = await FileHelper.SaveFileAsync(bookDto.Image, "images");
                }
                if (bookDto.PdfFile != null)
                {
                    book.PdfFileName = await FileHelper.SaveFileAsync(bookDto.PdfFile, "pdfs");
                }
                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> UpdateBook(int id, UpdateBookDto bookDto)
        {
            var book = await _context.Book.Include(b => b.BookCategories).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound(new { Message = $"Book with id {id} not found" });


            book.Name = bookDto.Name;
            book.Description = bookDto.Description;
            book.Price = bookDto.Price;
            book.Author = bookDto.Author;
            book.Status = bookDto.Status;



            if (bookDto.Image != null)
            {
                book.ProfileImage = await FileHelper.SaveFileAsync(bookDto.Image, "images");
            }
            if (bookDto.PdfFile != null)
            {
                book.PdfFileName = await FileHelper.SaveFileAsync(bookDto.PdfFile, "pdfs");
            }

            _context.Book.Update(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("PatchBook/{id}")]
        public async Task<IActionResult> PatchBook(int id, PatchBookDTO bookDto)
        {
            var book = await _context.Book.Include(b => b.BookCategories).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound(new { Message = $"Book with id {id} not found" });

            if (bookDto.Name != null) book.Name = bookDto.Name;
            if (bookDto.Description != null) book.Description = bookDto.Description;
            if (bookDto.Price.HasValue) book.Price = bookDto.Price.Value;
            if (bookDto.Author != null) book.Author = bookDto.Author;
            if (bookDto.Status.HasValue) book.Status = bookDto.Status.Value;


            if (bookDto.Image != null)
            {
                book.ProfileImage = await FileHelper.SaveFileAsync(bookDto.Image, "images");
            }
            if (bookDto.PdfFile != null)
            {
                book.PdfFileName = await FileHelper.SaveFileAsync(bookDto.PdfFile, "pdfs");
            }

            _context.Book.Update(book);
            await _context.SaveChangesAsync();

            return Ok(book);
        }




        [HttpDelete("DeleteBookById/{id}")]
        public async Task<IActionResult> DeleteBookById(int id)
        {
            try
            {
                var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == id);
                if (book == null)
                {
                    return NotFound(
                    new
                    {
                        Message = $"Book with id {id} not found"
                    });
                }

                book.Status = Status.Blocked;
                _context.Book.Update(book);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Book deleted successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }




    }
}
