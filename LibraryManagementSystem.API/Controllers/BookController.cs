using LibraryManagementSystem.API.Helpers;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {

                var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot";

                var books = await _context.Book.Select(book => new BookDetailDTO
                {
                    Id = book.Id,
                    Name = book.Name,
                    Description = book.Description,
                    Price = book.Price,
                    Author = book.Author,
                    Status = book.Status,
                    BookCreationDate = book.BookCreationDate,
                    ImagePath = !string.IsNullOrEmpty(book.ProfileImage)
                                ? $"{baseUrl}{book.ProfileImage}"
                                : null,
                    PdfFilePath = !string.IsNullOrEmpty(book.PdfFileName)
                                ? $"{baseUrl}{book.PdfFileName}"
                                : null,
                    QRCode = !string.IsNullOrEmpty(book.QRCode)
                                ? $"{baseUrl}{book.QRCode}"
                                : null,
                }).ToListAsync();

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
                    return NotFound(new { Message = $"Book with id {id} not found" });
                }


                var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot";


                var bookDetails = new BookDetailDTO
                {
                    Id = book.Id,
                    Name = book.Name,
                    Description = book.Description,
                    Price = book.Price,
                    Author = book.Author,
                    Status = book.Status,
                    BookCreationDate = book.BookCreationDate,
                    ImagePath = !string.IsNullOrEmpty(book.ProfileImage)
                                ? $"{baseUrl}{book.ProfileImage}"
                                : null,
                    PdfFilePath = !string.IsNullOrEmpty(book.PdfFileName)
                                ? $"{baseUrl}{book.PdfFileName}"
                                : null,
                    QRCode = !string.IsNullOrEmpty(book.QRCode)
                                ? $"{baseUrl}{book.QRCode}"
                                : null,
                };

                return Ok(bookDetails);
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
                string qrCodeImagePath = FileHelper.GenerateQrCodeAsync(bookDto.Name);
                var book = new Book
                {

                    Name = bookDto.Name,
                    Description = bookDto.Description,
                    Price = bookDto.Price,
                    Author = bookDto.Author,
                    Status = Status.Active,
                    BookAddedDate = DateTime.Now,
                    BookCreationDate = DateOnly.FromDateTime(DateTime.Now),
                    QRCode = qrCodeImagePath,


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

                var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot";

                var bookDetails = new BookDetailDTO
                {
                    Id = book.Id,
                    Name = book.Name,
                    Description = book.Description,
                    Price = book.Price,
                    Author = book.Author,
                    Status = book.Status,
                    BookCreationDate = book.BookCreationDate,
                    ImagePath = !string.IsNullOrEmpty(book.ProfileImage)
                                ? $"{baseUrl}{book.ProfileImage}"
                                : null,
                    PdfFilePath = !string.IsNullOrEmpty(book.PdfFileName)
                                ? $"{baseUrl}{book.PdfFileName}"
                                : null,
                    QRCode = !string.IsNullOrEmpty(book.QRCode)
                                ? $"{baseUrl}{book.QRCode}"
                                : null,
                };

                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, bookDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> UpdateBook(int id, UpdateBookDto bookDto)
        {
            var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound(new { Message = $"Book with id {id} not found" });

            if (bookDto.RemoveImage && !string.IsNullOrEmpty(book.ProfileImage))
            {
                FileHelper.DeleteFile(book.ProfileImage);
                book.ProfileImage = null;
            }

            if (bookDto.RemovePdf && !string.IsNullOrEmpty(book.PdfFileName))
            {
                FileHelper.DeleteFile(book.PdfFileName);
                book.PdfFileName = null;
            }

            if (!string.IsNullOrEmpty(book.QRCode))
            {
                FileHelper.DeleteFile(book.QRCode);
            }


            book.Name = bookDto.Name;
            book.Description = bookDto.Description;
            book.Price = bookDto.Price;
            book.Author = bookDto.Author;

            if (bookDto.Image != null)
            {
                if (!string.IsNullOrEmpty(book.ProfileImage))
                    FileHelper.DeleteFile(book.ProfileImage);

                book.ProfileImage = await FileHelper.SaveFileAsync(bookDto.Image, "images");
            }

            if (bookDto.PdfFile != null)
            {
                if (!string.IsNullOrEmpty(book.PdfFileName))
                    FileHelper.DeleteFile(book.PdfFileName);

                book.PdfFileName = await FileHelper.SaveFileAsync(bookDto.PdfFile, "pdfs");
            }


            _context.Book.Update(book);
            await _context.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot";


            var bookDetails = new BookDetailDTO
            {
                Id = book.Id,
                Name = book.Name,
                Description = book.Description,
                Price = book.Price,
                Author = book.Author,

                BookCreationDate = book.BookCreationDate,
                ImagePath = !string.IsNullOrEmpty(book.ProfileImage)
                               ? $"{baseUrl}{book.ProfileImage}"
                               : null,
                PdfFilePath = !string.IsNullOrEmpty(book.PdfFileName)
                               ? $"{baseUrl}{book.PdfFileName}"
                               : null,
                QRCode = !string.IsNullOrEmpty(book.QRCode)
                               ? $"{baseUrl}{book.QRCode}"
                               : null,
            };

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, bookDetails);
        }

        [HttpPatch("PatchBook/{id}")]
        public async Task<IActionResult> PatchBook(int id, PatchBookDTO bookDto)
        {
            var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound(new { Message = $"Book with id {id} not found" });


            if (bookDto.RemoveImage && !string.IsNullOrEmpty(book.ProfileImage))
            {
                FileHelper.DeleteFile(book.ProfileImage);
                book.ProfileImage = null;
            }
            if (bookDto.RemovePdf && !string.IsNullOrEmpty(book.PdfFileName))
            {
                FileHelper.DeleteFile(book.PdfFileName);
                book.PdfFileName = null;
            }


            if (!string.IsNullOrEmpty(book.QRCode))
            {
                FileHelper.DeleteFile(book.QRCode);
            }



            if (bookDto.Name != null) book.Name = bookDto.Name;
            if (bookDto.Description != null) book.Description = bookDto.Description;
            if (bookDto.Price.HasValue) book.Price = bookDto.Price.Value;
            if (bookDto.Author != null) book.Author = bookDto.Author;



            book.QRCode = FileHelper.GenerateQrCodeAsync(book.Name);



            if (bookDto.Image != null)
            {
                if (!string.IsNullOrEmpty(book.ProfileImage))
                    FileHelper.DeleteFile(book.ProfileImage);

                book.ProfileImage = await FileHelper.SaveFileAsync(bookDto.Image, "images");
            }

            if (bookDto.PdfFile != null)
            {
                if (!string.IsNullOrEmpty(book.PdfFileName))
                    FileHelper.DeleteFile(book.PdfFileName);

                book.PdfFileName = await FileHelper.SaveFileAsync(bookDto.PdfFile, "pdfs");
            }


            _context.Book.Update(book);
            await _context.SaveChangesAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot";


            var bookDetails = new BookDetailDTO
            {
                Id = book.Id,
                Name = book.Name,
                Description = book.Description,
                Price = book.Price,
                Author = book.Author,
                Status = book.Status,
                BookCreationDate = book.BookCreationDate,
                ImagePath = !string.IsNullOrEmpty(book.ProfileImage)
                               ? $"{baseUrl}{book.ProfileImage}"
                               : null,
                PdfFilePath = !string.IsNullOrEmpty(book.PdfFileName)
                               ? $"{baseUrl}{book.PdfFileName}"
                               : null,
                QRCode = !string.IsNullOrEmpty(book.QRCode)
                               ? $"{baseUrl}{book.QRCode}"
                               : null,
            };

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, bookDetails);
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
