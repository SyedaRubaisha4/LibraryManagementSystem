using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.DBModel;
using Models.DTOModel;

namespace LibraryManagementSystem.Controllers
{
    public class CategoryController : Controller

    {
        private readonly ApplicationDbContext _context;
        private readonly ReportsRepository _reportsRepository;
        public CategoryController(ApplicationDbContext context, ReportsRepository reportsRepository)
        {
          _reportsRepository = reportsRepository;
            _context = context;
        }


        public IActionResult Index()
        {
            var books = _context.Category.ToList();
           

            return View(books);
        }
        [HttpGet]
        public IActionResult GetCategoryDetails(int id)
        {
            var user = _context.Category.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Json(new
            {
                name = user.Name,
            });
        }
        [HttpPost]
        public IActionResult AddCategory(string Name)
        {
            if (Name != null)
            {
                var Category = new Category { Name=Name };

                _context.Category.Add(Category);
                _context.SaveChanges();
                ViewData["CategoryAddes"] = "Category Added successfully";
            }
            else
            {
                ViewData["CategoryName"] = "Add a name";
            }
            return RedirectToAction("index");
        }

        public IActionResult Update(string Name,int Id)
        {
            var category = _context.Category.FirstOrDefault(b => b.Id == Id);
            if(category!=null)
            {
                category.Name = Name; _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult ViewCategory()
        {
           
            return View();
        }
        [HttpPost]
        public IActionResult Del(int id)
        {
            var person = _context.Category.Find(id);

            if (person != null)
            {
            
                _context.Category.Remove(person);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }


    }
}
