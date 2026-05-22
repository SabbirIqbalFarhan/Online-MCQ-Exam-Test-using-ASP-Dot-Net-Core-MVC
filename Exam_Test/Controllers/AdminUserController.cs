using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminUserController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string searchStudentId = "", string searchName = "")
        {
            int pageSize = 10;
            var allUsers = _userManager.Users.ToList();
            var profiles = _context.UserProfiles.ToList();

            // Get all admin user IDs
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminUserIds = adminUsers.Select(u => u.Id).ToList();

            // Apply search filters (match against profiles)
            if (!string.IsNullOrWhiteSpace(searchStudentId))
            {
                var matchedIds = profiles
                    .Where(p => p.StudentId != null && p.StudentId.Contains(searchStudentId, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.UserId)
                    .ToHashSet();
                allUsers = allUsers.Where(u => matchedIds.Contains(u.Id)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var matchedIds = profiles
                    .Where(p => p.FullName != null && p.FullName.Contains(searchName, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.UserId)
                    .ToHashSet();
                allUsers = allUsers.Where(u => matchedIds.Contains(u.Id)).ToList();
            }

            int totalUsers = allUsers.Count;
            var users = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var permissions = _context.ExamPermissions.ToList();
            var requests = _context.ExamRequests.ToList();

            ViewBag.Permissions = permissions;
            ViewBag.Requests = requests;
            ViewBag.Profiles = profiles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            ViewBag.SearchStudentId = searchStudentId;
            ViewBag.SearchName = searchName;
            ViewBag.AdminUserIds = adminUserIds;

            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            var permission = _context.ExamPermissions.FirstOrDefault(p => p.UserId == id);
            ViewBag.Permission = permission;

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == id);
            ViewBag.Profile = profile;

            return View(user);
        }

        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.UserId = id;
            ViewBag.UserEmail = user.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                ViewBag.Message = "Password reset successfully!";
                ViewBag.Success = true;
            }
            else
            {
                ViewBag.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                ViewBag.Success = false;
            }

            ViewBag.UserId = id;
            ViewBag.UserEmail = user.Email;
            return View();
        }

        public async Task<IActionResult> AssignUserRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> AssignStudentId(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == id);

            ViewBag.UserId = id;
            ViewBag.UserEmail = user.Email;
            ViewBag.CurrentStudentId = profile?.StudentId;
            ViewBag.CurrentFullName = profile?.FullName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudentId(string id, string studentId, string fullName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == id);

            if (profile == null)
            {
                _context.UserProfiles.Add(new UserProfile
                {
                    UserId = id,
                    StudentId = studentId,
                    FullName = fullName
                });
            }
            else
            {
                profile.StudentId = studentId;
                profile.FullName = fullName;
            }

            _context.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GrantPermission(string id)
        {
            var existing = _context.ExamPermissions.FirstOrDefault(p => p.UserId == id);

            if (existing == null)
            {
                _context.ExamPermissions.Add(new ExamPermission
                {
                    UserId = id,
                    IsPermitted = true,
                    GrantedAt = DateTime.UtcNow.AddHours(6)
                });
            }
            else
            {
                existing.IsPermitted = true;
                existing.GrantedAt = DateTime.UtcNow.AddHours(6);
            }

            var request = _context.ExamRequests.FirstOrDefault(r => r.UserId == id);
            if (request != null) request.Status = "Approved";

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevokePermission(string id)
        {
            var existing = _context.ExamPermissions.FirstOrDefault(p => p.UserId == id);
            if (existing != null) existing.IsPermitted = false;

            var request = _context.ExamRequests.FirstOrDefault(r => r.UserId == id);
            if (request != null) request.Status = "Rejected";

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // NEW: Delete User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Admin accounts cannot be deleted.";
                return RedirectToAction("Index");
            }

            // Remove related data
            var perms = _context.ExamPermissions.Where(p => p.UserId == id).ToList();
            _context.ExamPermissions.RemoveRange(perms);

            var reqs = _context.ExamRequests.Where(r => r.UserId == id).ToList();
            _context.ExamRequests.RemoveRange(reqs);

            var profiles = _context.UserProfiles.Where(p => p.UserId == id).ToList();
            _context.UserProfiles.RemoveRange(profiles);

            var results = _context.Results.Where(r => r.UserId == id).ToList();
            _context.Results.RemoveRange(results);

            // Fix #3: Also delete UserAnswers
            var answers = _context.UserAnswers.Where(a => a.UserId == id).ToList();
            _context.UserAnswers.RemoveRange(answers);

            _context.SaveChanges();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}