using CSharpGradingSystem.Data;
using GradingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpGradingSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TeacherController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // ✅ View Grades Page
        public IActionResult Grades()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            // Get teacher
            var teacher = _context.Teachers
                .Include(t => t.UserAccount)
                .FirstOrDefault(t => t.UserAccount != null && t.UserAccount.Email == email);

            if (teacher == null)
                return RedirectToAction("Dashboard");

            // Get subjects assigned to teacher
            var subjects = _context.Subjects
                .Where(s => s.AssignedTeacherId == teacher.Id)
                .ToList();

            // Prepare model: Subject -> List<Student>
            var model = new Dictionary<Subject, List<Student>>();

            foreach (var subject in subjects)
            {
                var students = _context.StudentSubjectAssignments
                    .Include(ssa => ssa.Student)
                    .Where(ssa => ssa.SubjectId == subject.Id)
                    .Select(ssa => ssa.Student!)
                    .ToList();

                // Preload existing grades into ViewData
                foreach (var student in students)
                {
                    var grade = _context.Grades
                        .FirstOrDefault(g => g.StudentId == student.Id && g.SubjectId == subject.Id);

                    ViewData[$"Grade_{subject.Id}_{student.Id}"] = grade;
                }

                model[subject] = students;
            }

            return View(model);
        }

        // ✅ Save or Update Grades
        [HttpPost]
        public IActionResult SaveGrades(int subjectId, List<int> studentIds, List<double?> prelim, List<double?> midterm, List<double?> semifinal, List<double?> final)
        {
            for (int i = 0; i < studentIds.Count; i++)
            {
                var studentId = studentIds[i];

                var existingGrade = _context.Grades.FirstOrDefault(g => g.StudentId == studentId && g.SubjectId == subjectId);

                if (existingGrade == null)
                {
                    existingGrade = new Grade
                    {
                        StudentId = studentId,
                        SubjectId = subjectId
                    };
                    _context.Grades.Add(existingGrade);
                }

                existingGrade.Prelim = prelim[i];
                existingGrade.Midterm = midterm[i];
                existingGrade.SemiFinal = semifinal[i];
                existingGrade.Final = final[i];

                // Calculate FinalGrade if all components exist
                if (prelim[i].HasValue && midterm[i].HasValue && semifinal[i].HasValue && final[i].HasValue)
                {
                    existingGrade.FinalGrade = Math.Round((prelim[i].Value + midterm[i].Value + semifinal[i].Value + final[i].Value) / 4, 2);
                }
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Grades saved successfully!";
            return RedirectToAction("Grades");
        }

        // ✅ Teacher Profile
        public IActionResult TeacherProfile()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var teacher = _context.Teachers
                .Include(t => t.UserAccount)
                .Include(t => t.Subjects)
                .FirstOrDefault(t => t.UserAccount != null && t.UserAccount.Email == email);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found.";
                return RedirectToAction("Dashboard");
            }

            return View(teacher);
        }

        // ✅ Upload Profile Picture
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile profilePic)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email) || profilePic == null)
                return RedirectToAction("TeacherProfile");

            var teacher = _context.Teachers
                .Include(t => t.UserAccount)
                .FirstOrDefault(t => t.UserAccount != null && t.UserAccount.Email == email);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found.";
                return RedirectToAction("Dashboard");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{profilePic.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePic.CopyToAsync(fileStream);
            }

            teacher.ProfilePicturePath = "/uploads/profile/" + uniqueFileName;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profile photo updated successfully.";
            return RedirectToAction("TeacherProfile");
        }

        // ✅ My Students
        public IActionResult Students()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var teacher = _context.Teachers
                .Include(t => t.UserAccount)
                .FirstOrDefault(t => t.UserAccount != null && t.UserAccount.Email == email);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Dashboard");
            }

            // Subjects assigned to this teacher
            var subjects = _context.Subjects
                .Where(s => s.AssignedTeacherId == teacher.Id)
                .ToList();

            var subjectStudentsDict = new Dictionary<Subject, List<Student>>();

            foreach (var subject in subjects)
            {
                var students = _context.StudentSubjectAssignments
                    .Where(ssa => ssa.SubjectId == subject.Id)
                    .Include(ssa => ssa.Student)
                    .Select(ssa => ssa.Student)
                    .Where(st => st != null)
                    .ToList();

                subjectStudentsDict.Add(subject, students!);
            }

            return View(subjectStudentsDict);
        }
        public IActionResult SubjectList()
        {
            // Example: get logged-in teacher (replace with your logic)
            int teacherId = 1; // e.g., from session or authentication

            var subjects = _context.Subjects
                .Include(s => s.AssignedTeacher)
                .Where(s => s.AssignedTeacherId == teacherId)
                .ToList();

            return View(subjects);
        }

        // GET: View Students in a Subject
        public IActionResult ViewStudents(int subjectId)
        {
            var subject = _context.Subjects
                .Include(s => s.AssignedTeacher)
                .FirstOrDefault(s => s.Id == subjectId);

            if (subject == null)
                return NotFound();

            // Get students who have grades for this subject
            var students = _context.Grades
                .Where(g => g.SubjectId == subjectId)
                .Include(g => g.Student)
                .Select(g => g.Student)
                .Distinct()
                .ToList();

            ViewBag.SubjectName = $"{subject.SubjectCode} - {subject.SubjectName}";
            return View(students);
        }

        // GET: Manage Grades for a Subject
        public IActionResult ManageGrades(int subjectId)
        {
            var subject = _context.Subjects
                .Include(s => s.AssignedTeacher)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                .FirstOrDefault(s => s.Id == subjectId);

            if (subject == null)
                return NotFound();

            return View(subject);
        }

        // POST: Update Grades
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateGrades(List<Grade> grades)
        {
            if (grades == null || !grades.Any())
                return RedirectToAction("MySubjects");

            foreach (var grade in grades)
            {
                var existingGrade = _context.Grades.FirstOrDefault(g => g.Id == grade.Id);
                if (existingGrade != null)
                {
                    existingGrade.Prelim = grade.Prelim;
                    existingGrade.Midterm = grade.Midterm;
                    existingGrade.SemiFinal = grade.SemiFinal;
                    existingGrade.Final = grade.Final;
                    existingGrade.FinalGrade = CalculateFinalGrade(
                        grade.Prelim, grade.Midterm, grade.SemiFinal, grade.Final);
                }
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Grades updated successfully!";
            return RedirectToAction("ManageGrades", new { subjectId = grades.First().SubjectId });
        }

        // Helper method: Calculate final grade
        private double? CalculateFinalGrade(double? prelim, double? midterm, double? semiFinal, double? final)
        {
            var total = 0.0;
            var count = 0;

            if (prelim.HasValue) { total += prelim.Value; count++; }
            if (midterm.HasValue) { total += midterm.Value; count++; }
            if (semiFinal.HasValue) { total += semiFinal.Value; count++; }
            if (final.HasValue) { total += final.Value; count++; }

            if (count == 0) return null;
            return Math.Round(total / count, 2);
        }

    }
}

