
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GetAll: Search, filter, and pagination
        public async Task<IActionResult> GetAll(
            string? searchName,
            int? departmentId,
            int pageNumber = 1)
        {
            int pageSize = 5;

            var query = _context.Students
                .Include(s => s.Department)
                .AsQueryable();

            // Search by student name
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(s =>
                    s.Name.Contains(searchName));
            }

            // Filter by department
            if (departmentId.HasValue)
            {
                query = query.Where(s =>
                    s.DepartmentId == departmentId.Value);
            }

            int totalStudents = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalStudents / (double)pageSize);

            // Keep page number within valid bounds
            if (totalPages == 0)
                pageNumber = 1;
            else
                pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            var students = await query
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchName = searchName;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "Id", "Name", departmentId);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(students);
        }

        // 2. GetById: Show one student
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // 3. Add: Display the form
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await LoadDepartments();
            return View();
        }

        // Add: Save the new student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Student student)
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartments(student.DepartmentId);
                return View(student);
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetAll));
        }

        // 4. Edit: Display the existing student
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            await LoadDepartments(student.DepartmentId);
            return View(student);
        }

        // Edit: Save the changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadDepartments(student.DepartmentId);
                return View(student);
            }

            var existingStudent = await _context.Students.FindAsync(id);

            if (existingStudent == null)
                return NotFound();

            existingStudent.Name = student.Name;
            existingStudent.Age = student.Age;
            existingStudent.DepartmentId = student.DepartmentId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetAll));
        }

        // 5. Delete: Display the warning page first
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // Delete: Remove only after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetAll));
        }

        // Helper method to load departments into a dropdown
        private async Task LoadDepartments(int? selectedDepartmentId = null)
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "Id",
                "Name",
                selectedDepartmentId);
        }
    }
}
