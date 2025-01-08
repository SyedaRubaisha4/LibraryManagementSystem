using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.DBModel;
using Models.DTOModel;
using QRCoder;
using Stripe;
using Stripe.Checkout;
using System.Reflection.Metadata;
using QRCoder;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;


namespace LibraryManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripeSettings;
        private readonly ReportsRepository _reportsRepository;
        public BookController(ApplicationDbContext context, ReportsRepository reportsRepository, IOptions<StripeSettings> stripeSettings)
        {
            _context = context;
            _stripeSettings = stripeSettings.Value;
            _reportsRepository = reportsRepository;
        }
        [HttpPost]
        public IActionResult Generate(string Name)
        {
            if (string.IsNullOrEmpty(Name))
            {
                ModelState.AddModelError("Name", "Name is required to generate a QR code.");
                return View();
            }

            try
            {
                // Generate the QR code
                string qrCodeImage = GenerateQrCodeAsync(Name);

                // Pass the QR code image as a model or ViewBag variable
                ViewBag.QrCodeImage = qrCodeImage;

                return View("BookView"); // Redirect to the same view or another view to show the QR code
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error generating QR Code: {ex.Message}");
                return View(); // Return the view with error
            }
        }
        private string GenerateQrCodeAsync(string data)
        {
            // Create a QR code generator instance
            using (QRCodeGenerator qrCodeGenerator = new QRCodeGenerator())
            {
                // Create QR code data from the input string
                using (QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                {

                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20);


                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BookQRCodes");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Generate a unique file name for the QR code
                        var qrCodeFileName = Guid.NewGuid().ToString() + ".png";
                        var qrCodePath = Path.Combine(directoryPath, qrCodeFileName);

                        System.IO.File.WriteAllBytesAsync(qrCodePath, qrCodeImage);

                        return Convert.ToBase64String(qrCodeImage); ; // Return the file name to store in the database
                    }
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest request)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            int id = int.Parse(request.Id);
            var price = _context.Book
                         .Where(b => b.Id == id)
                         .Select(b => b.Price)
                         .FirstOrDefault();
            long p = (long)price;


            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = p,
                    Currency = "usd",
                    PaymentMethod = request.PaymentMethodId,
                    Confirm = true,
                    ReturnUrl = "https://localhost:7085/Book/BookView"
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                Console.WriteLine(paymentIntent);
                if (paymentIntent.Status == "succeeded")
                {
                    var userId = HttpContext.Session.GetString("UserId");
                    int ids = int.Parse(userId);
                    var userBook = _context.ReservedBook.FirstOrDefault(ub => ub.BookId == int.Parse(request.Id) && ub.UserId == ids);

                    UserPayment userPayment = new UserPayment
                    {
                        PaymentIntentId = paymentIntent.Id,
                        PaymentAmount = p,
                        PaymentDate = DateTime.UtcNow,
                        UserBookId = userBook.Id,
                        AdminProfit = Convert.ToDecimal(0.05 * p),
                    };


                    _context.UserPayments.Add(userPayment);
                    _context.SaveChanges();
                    return Json(new { success = true });

                }

                return Json(new { success = false, error = "Payment failed or requires further action." });
            }
            catch (StripeException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult SaveCategories([FromBody] CategorySelectionDto selection)
        {
            if (selection == null || selection.CategoryIds == null || !selection.CategoryIds.Any())
            {
                return BadRequest(new { message = "No categories selected." });
            }

            var latestBook = _context.Book.OrderByDescending(b => b.BookAddedDate).FirstOrDefault();

            //foreach (var categoryId in selection.CategoryIds)
            //{
            //    var bookCategory = new BookCategories
            //    {
            //        BookId = latestBook.Id + 1,
            //        CategoryId = categoryId
            //    };

            //    _context.Category.Add(bookCategory);
            //}

            _context.SaveChanges();

            return Ok(new { message = "Categories saved successfully!" });
        }
        [HttpPost]
        public IActionResult Add(string Name, string Description, decimal Price, string Author, DateTime BookCreationDate, IFormFile? Image, IFormFile? PdfFile, List<string> SelectedCategories)
        {
            var book = new Book
            {
                Name = Name,
                Description = Description,
                Price = Price,
                Author = Author,
                BookCreationDate = DateOnly.FromDateTime(BookCreationDate),
                BookAddedDate = DateTime.Now,
                QRCode = GenerateQrCodeAsync(Name + "\n" + Author + "\n" + Description),
            };


            if (Image != null && Image.Length > 0)
            {
                string imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                Directory.CreateDirectory(imageFolder);

                string imageFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Image.FileName);
                string imagePath = Path.Combine(imageFolder, imageFileName);

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    Image.CopyTo(fileStream);
                }

                book.ProfileImage = imageFileName;
            }



            if (PdfFile != null && PdfFile.Length > 0)
            {
                string pdfFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");
                Directory.CreateDirectory(pdfFolder);

                string pdfFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PdfFile.FileName);
                string pdfPath = Path.Combine(pdfFolder, pdfFileName);

                using (var fileStream = new FileStream(pdfPath, FileMode.Create))
                {
                    PdfFile.CopyTo(fileStream);
                }

                book.PdfFileName = pdfFileName;
            }

            _context.Book.Add(book);
            _context.SaveChanges();
            var latestBook = _context.Book.OrderByDescending(b => b.BookAddedDate).FirstOrDefault();

            if (SelectedCategories != null && SelectedCategories.Any())
            {
                foreach (var categoryId in SelectedCategories)
                {
                    var bookCategory = new BookCategories
                    {
                        BookId = book.Id,  
                        CategoryId = int.Parse(categoryId) 
                    };

                    _context.BookCategory.Add(bookCategory); 
                }

                _context.SaveChanges();
            }

            return RedirectToAction("ViewBook");
        }
        public IActionResult GetBooksBasedOnReservedCategory()
        {
            var userId = HttpContext.Session.GetString("UserId");


            int ids = int.Parse(userId);
            var books = _context.Book
         .FromSqlRaw("EXEC GetReservedBooksBasedOnCategory @UserId = {0}", ids)
         .ToList();

            // Map the result to a list of BookDTO
            var bookDTOs = books.Select(b => new BookDTO
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                ProfileImage = b.ProfileImage // Assuming ProfileImage is stored as a file name
            }).ToList();
            ViewData["Books"] = bookDTOs;
            // Pass the DTO list to the view
            return View(bookDTOs);

        }

        [HttpGet]
        public IActionResult GetUserDetails(int id)
        {
            var user = _context.Book.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Json(new
            {
                name = user.Name,
                description = user.Description,
                author = user.Author,
                bookCreationDate = user.BookCreationDate.ToString("yyyy-MM-dd"),
                price = user.Price,
                profileImage = user.ProfileImage != null ? Url.Content("~/Image/" + user.ProfileImage) : null,
                pdfFileName = user.PdfFileName != null ? Url.Content("~/Files/" + user.PdfFileName) : null
            });
        }
        [HttpGet]
        public IActionResult GetBookCategories(int id)
        {
            var bookCategories = _context.BookCategory
                .Where(bc => bc.BookId == id)
                .Select(bc => bc.CategoryId) // Get Category IDs for the book
                .ToList();

             var allCategories = _context.Category
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Selected = bookCategories.Contains(c.Id) // Mark categories already assigned to the book
                })
                .ToList();

            return Json(allCategories);
        }

        public IActionResult UserBooks([FromBody] ReserveBookRequesting request)
        {
            if (request == null || string.IsNullOrEmpty(request.Id))
            {
                return Json(new { success = false, error = "Invalid Book ID" });
            }
            var userId = HttpContext.Session.GetString("UserId");
            int ids = int.Parse(userId);
            int bookId = int.Parse(request.Id);
            ReservedBooks b = new ReservedBooks
            {
                UserId = int.Parse(userId),
                BookId = bookId,
                BookDateTime = DateTime.Now,

                PaymentStatus = "Paid"
            };

            _context.ReservedBook.Add(b);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        public class ReserveBookRequesting
        {
            public string Id { get; set; }
        }
        [HttpPost]
        public IActionResult UnreserveBook(int bookId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["SessionExpired"] = "No book reserved";
                return RedirectToAction("Login");
            }

            int ids = int.Parse(userId);

            var userBook = _context.ReservedBook.FirstOrDefault(ub => ub.BookId == bookId && ub.UserId == ids);
            if (userBook != null)
            {
                _context.ReservedBook.Remove(userBook);
                _context.SaveChanges();
            }

            return RedirectToAction("Show");
        }
        public IActionResult ViewBooks(int pageNumber = 1, int pageSize = 10)
        {
            var books = _reportsRepository.GetBookCategories(pageNumber, pageSize);
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(books);
        }
        public IActionResult ViewBook(int page = 1, int pageSize = 3, string searchQuery = "", string sortBy = "Name", bool isAscending = true)
        {

            var categories = _context.Category.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            ViewData["category"] = categories;




            var query = _context.Book
        .Include(b => b.BookCategories)        
        .ThenInclude(bc => bc.Category)         
        .AsQueryable();

            Console.Write(query);

            // Apply search query filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(b => b.Name.Contains(searchQuery) || b.Author.Contains(searchQuery));
            }


            query = sortBy switch
            {
                "Name" => isAscending ? query.OrderBy(b => b.Name) : query.OrderByDescending(b => b.Name),
                "Author" => isAscending ? query.OrderBy(b => b.Author) : query.OrderByDescending(b => b.Author),
                "Price" => isAscending ? query.OrderBy(b => b.Price) : query.OrderByDescending(b => b.Price),
                _ => query.OrderByDescending(b => b.Id), // Default sorting by Name
            };

            // Get total count for pagination
            var totalCount = query.Count();

            // Get the paged data
            var books = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Set the ViewBag values for the view
            ViewBag.Books = books;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.TotalCount = totalCount;
            ViewBag.SortBy = sortBy;
            ViewBag.IsAscending = isAscending;

            var viewModel = new BookViewModal
            {
                Books = books,
                Book = new Book()
            };

            return View(viewModel);
        }
        public IActionResult Userpayments()
        {
            var paymentInfo = from up in _context.UserPayments
                              join rb in _context.ReservedBook on up.UserBookId equals rb.Id
                              join b in _context.Book on rb.BookId equals b.Id
                              join u in _context.User on rb.UserId equals u.Id
                              select new PaymentInfoViewModel
                              {
                                  Firstname = u.FirstName,
                                  BookName = b.Name,
                                  PaymentStatus = rb.PaymentStatus,
                                  PaymentAmount = up.PaymentAmount,
                                  PaymentDate = up.PaymentDate,
                                  AdminPayments = up.AdminProfit
                              };

            // Pass the data to the view
            return View(paymentInfo.ToList());
        }
        public IActionResult Show()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["SessionExpired"] = "No book reserved.";
                return View(new List<BookDetails>());
            }

            int ids = int.Parse(userId);

            var reservedBooks = _context.ReservedBook
           .Include(ub => ub.Book)
           .Where(ub => ub.UserId == ids)
           .Select(ub => new BookDetails
           {
               Id = ub.BookId,
               BookName = ub.Book.Name,
               AuthorName = ub.Book.Author,
               Description = ub.Book.Description,
               BookCreationDate = ub.Book.BookCreationDate,
               CreationDate = ub.BookDateTime,
               IsReserved = true
           })
           .ToList();


            return View(reservedBooks);
        }
        public List<BookDetails> GetBooksForUserAsync(int userId)
        {
            var books = _context.ReservedBook
              .Include(ub => ub.Book)
              .Where(ub => ub.UserId == userId)
              .Select(ub => new BookDetails
              {
                  BookName = ub.Book.Name,
                  CreationDate = DateTime.Now,
                  AuthorName = ub.Book.Author,
                  Description = ub.Book.Description,

                  BookCreationDate = ub.Book.BookCreationDate
              })
             .ToList();

            return books;
        }
        public IActionResult ReserveBook(int BookId, int UserId)
        {
            ReservedBooks b = new ReservedBooks
            {
                UserId = UserId,
                BookId = BookId,
                BookDateTime = DateTime.Now,
            };

            _context.ReservedBook.Add(b);
            _context.SaveChanges();

            return RedirectToAction("show");
        }
        [HttpPost]
        public IActionResult ReserveBook([FromBody] ReserveBookRequest request)
        {
            if (request == null || request.BookId <= 0 || request.UserId <= 0)
            {
                return BadRequest("Invalid data.");
            }


            var success = ReserveBook(request.BookId, request.UserId);
            if (success != null)
            {
                return Ok(new { message = "Book reserved successfully." });
            }

            return StatusCode(500, "Failed to reserve book. Try again later.");
        }
        public IActionResult BookList()
        {
            var books = _context.Book.ToList();
            var users = _context.User.ToList();

            var viewModel = new BookListViewModel
            {
                Books = books,
                Users = users
            };

            return View(viewModel);
        }
        [HttpGet]
        public JsonResult GetUsers()
        {
            var users = _context.User.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName
            }).ToList();

            return Json(users, new JsonSerializerOptions { PropertyNamingPolicy = null });

        }
        [HttpPost]
        public IActionResult bookid(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int ids = int.Parse(userId);

            ReservedBooks b = new ReservedBooks
            {
                UserId = int.Parse(userId),
                BookId = id,
                BookDateTime = DateTime.Now,
            };

            _context.ReservedBook.Add(b);
            _context.SaveChanges();

            return View();
        }
        [HttpPost]
        public void DeleteFile(string fileName)
        {

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");

            string filePath = Path.Combine(uploadFolder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

            }

        }

        public IActionResult Update(string Name, string Description, string Author, DateOnly BookCreationDate, decimal Price, int Id, IFormFile? profileImage, IFormFile? PdfFile, List<string> SelectedCategories)
        {
            var book = _context.Book.FirstOrDefault(b => b.Id == Id);
            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }


             book.Name = Name;
            book.Description = Description;
            book.Author = Author;
            book.BookCreationDate = BookCreationDate;
            book.Price = Price;

             if (profileImage != null && profileImage.Length > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profileImage.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(fileStream);
                }

                book.ProfileImage = uniqueFileName;
            }
            DeleteFile(book.PdfFileName);
            // Handle PDF file upload
            if (PdfFile != null && PdfFile.Length > 0)
            {
                string pdfFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");
                Directory.CreateDirectory(pdfFolder);

                string uniquePdfFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PdfFile.FileName);
                string pdfFilePath = Path.Combine(pdfFolder, uniquePdfFileName);

                using (var fileStream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    PdfFile.CopyTo(fileStream);
                }

                book.PdfFileName = uniquePdfFileName;
            }

            _context.SaveChanges();

            return RedirectToAction("ViewBook");
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReserveBooks([FromBody] ReserveBookRequest request)
        {
            long amount = 2000;

            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = amount,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Book Reservation"
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = "https://yourdomain.com/success", // Redirect after success
                CancelUrl = "https://yourdomain.com/cancel"  // Redirect after cancel
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(sessionOptions);

            return Json(new { sessionId = session.Id });
        }
        [HttpPost]
        public IActionResult ToggleReservation(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You need to log in to reserve or unreserve books.";
                return RedirectToAction("Login");
            }

            int userIdInt = int.Parse(userId);
            var userBook = _context.ReservedBook
                .FirstOrDefault(ub => ub.BookId == id && ub.UserId == userIdInt);

            _context.ReservedBook.Remove(userBook);
            _context.SaveChangesAsync();
            TempData["Success"] = "Book unreserved successfully.";


            return RedirectToAction("Show");
        }
        public IActionResult BookView()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["SessionExpired"] = "No book reserved";
                return View(new List<BookDetails>());
            }

            int ids = int.Parse(userId);


            var booking = _context.Book
                .FromSqlRaw("EXEC GetReservedBooksBasedOnCategory @UserId = {0}", ids)
                .ToList();
            var bookDTOs = booking.Select(b => new BookDTO
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                ProfileImage = b.ProfileImage // Ensure ProfileImage is available
            }).ToList();

            ViewData["Books"] = bookDTOs;

            var books = _context.Book
                .Select(book => new BookDetails
                {
                    Id = book.Id,
                    BookName = book.Name,
                    CreationDate = book.BookAddedDate,
                    AuthorName = book.Author,
                    Description = book.Description,
                    Price = book.Price,
                    BookCreationDate = book.BookCreationDate,
                    IsReserved = _context.ReservedBook.Any(ub => ub.BookId == book.Id && ub.UserId == ids),
                    IsFavorite = _context.FavoriteBooks.Any(fb => fb.BookId == book.Id && fb.UserId == ids && fb.IsFavorite)
                   
                })
                .ToList();

            return View(books);
        }


        [HttpPost]
        public IActionResult Del(int id)
        {
            var person = _context.Book.Find(id);

            if (person != null)
            {
                person.Status = Status.Blocked;
                _context.Attach(person);
                _context.Entry(person).State = EntityState.Modified;
                _context.SaveChanges();
            }
            return RedirectToAction("ViewBook");
        }

        [HttpGet]
        public IActionResult FavoriteBooks()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["SessionExpired"] = "Please log in to add favorites.";
                return RedirectToAction("Index");
            }
            int ids = int.Parse(userId);


            var favoriteBooks = _context.FavoriteBooks
                    .Where(fb => fb.UserId == ids && fb.IsFavorite)
                    .Select(fb => new BookDetails
                    {
                        Id = fb.Book.Id,
                        BookName = fb.Book.Name,
                        Rating = fb.Rating,
                        BookFavoritedAt = fb.BookFavoritedDate,
                        IsFavorite = fb.IsFavorite
                    }).ToList();



            return View(favoriteBooks);
        }

        [HttpPost]
        public IActionResult FavoriteBook([FromBody] FavoriteBookDto favoriteBookDto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["SessionExpired"] = "Please log in to add favorites.";
                return RedirectToAction("Index");
            }
            int ids = int.Parse(userId);


            var favoriteBook = _context.FavoriteBooks
                .FirstOrDefault(fb => fb.UserId == ids && fb.BookId == favoriteBookDto.BookId);

            if (favoriteBook == null)
            {
                // Add a new favorite book with a rating
                favoriteBook = new FavoriteBook
                {
                    UserId = ids,
                    BookId = favoriteBookDto.BookId,
                    Rating = favoriteBookDto.Rating,
                    IsFavorite = true
                };
                _context.FavoriteBooks.Add(favoriteBook);
            }
            else
            {

                favoriteBook.IsFavorite = !favoriteBook.IsFavorite;

                if (favoriteBookDto.Rating != 0)
                {
                    favoriteBook.Rating = favoriteBookDto.Rating;
                }
            }

            _context.SaveChanges();
            return Ok();
        }





    }
}
