
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WafiSolution_Assignment.Data;
using WafiSolution_Assignment.Models;

namespace WafiSolution_Assignment.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeController(AppDbContext context, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Index(string sortOrder, string searchName, string searchEmail, 
            string searchMobile, DateTime? searchDOB, int? pageNumber)
        {
            // Store search parameters in ViewData for use in the view
            ViewData["searchName"] = searchName;
            ViewData["searchEmail"] = searchEmail;
            ViewData["searchMobile"] = searchMobile;
            ViewData["searchDOB"] = searchDOB;
            ViewData["CurrentSort"] = sortOrder;
            // this is develop branch
            //var employees = from e in _context.Employees select e;
            var employees = _context.Employees.AsQueryable();

            // Filtering by name, email, mobile, and date of birth
            if (!string.IsNullOrEmpty(searchName))
            {
                employees = employees.Where(e => e.FirstName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchEmail))
            {
                employees = employees.Where(e => e.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchMobile))
            {
                employees = employees.Where(e => e.Mobile.Contains(searchMobile));
            }

            if (searchDOB.HasValue)
            {
                employees = employees.Where(e => e.DateOfBirth == searchDOB.Value);
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "name_desc":
                    employees = employees.OrderByDescending(e => e.FirstName);
                    break;
                case "email":
                    employees = employees.OrderBy(e => e.Email);
                    break;
                case "email_desc":
                    employees = employees.OrderByDescending(e => e.Email);
                    break;
                case "dob":
                    employees = employees.OrderBy(e => e.DateOfBirth);
                    break;
                case "dob_desc":
                    employees = employees.OrderByDescending(e => e.DateOfBirth);
                    break;
                default:
                    employees = employees.OrderBy(e => e.FirstName);
                    break;
            }

            int pageSize = 10;
            return View(await PagingModel<Employee>.CreateAsync(employees.AsNoTracking(), pageNumber ?? 1, pageSize));
        }




        // GET: Create Employee
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee em)
        {
            if (ModelState.IsValid)
            {
                // Handle photo upload
                if (em.PhotoFile != null)
                {
                    // Ensure "wwwroot/images" directory exists
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Create a unique file name for the uploaded file
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + em.PhotoFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the uploaded file to the wwwroot/images folder
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await em.PhotoFile.CopyToAsync(fileStream);
                    }

                    // Save the relative path to the PhotoPath field (for later retrieval)
                    em.PhotoPath = "/images/" + uniqueFileName;
                }

                // Save the employee data to the database
                _context.Add(em);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(em);
        }


        // GET: Edit Employee
        public async Task<IActionResult> Edit(int id)
        {
            var em = await _context.Employees.FindAsync(id);
            if (em == null)
            {
                return NotFound();
            }
            return View(em);
        }

        // POST: Edit Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee em)
        {
            if (id != em.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (em.PhotoFile != null)
                    {
                        string uniqueFileName = UploadPhoto(em);
                        em.PhotoPath = "/images/" + uniqueFileName;
                    }

                    _context.Update(em);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(em.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(em);
        }

        // GET: Delete Employee
        public async Task<IActionResult> Delete(int id)
        {
            var em = await _context.Employees.FindAsync(id);
            if (em == null)
            {
                return NotFound();
            }

            return View(em);
        }

        // POST: Delete Employee
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var em = await _context.Employees.FindAsync(id);
            if (em != null)
            {
                if (em.PhotoPath != null)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", em.PhotoPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    else
                    {
                        Console.WriteLine("Do not Work");
                    }
                }
                _context.Employees.Remove(em);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("Not found");
            }

            return RedirectToAction(nameof(Index));
        }

        // Utility Method: Upload Photo
        private string UploadPhoto(Employee em)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + em.PhotoFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                em.PhotoFile.CopyTo(fileStream);
            }

            return uniqueFileName;
        }

        // Check if Employee Exists
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
