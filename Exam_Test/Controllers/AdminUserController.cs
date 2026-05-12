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

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var permissions = _context.ExamPermissions.ToList();
            var requests = _context.ExamRequests.ToList();
            var profiles = _context.UserProfiles.ToList();

            ViewBag.Permissions = permissions;
            ViewBag.Requests = requests;
            ViewBag.Profiles = profiles;

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

        [HttpGet]
        public IActionResult GrantPermission(string id)
        {
            var existing = _context.ExamPermissions.FirstOrDefault(p => p.UserId == id);

            if (existing == null)
            {
                _context.ExamPermissions.Add(new ExamPermission
                {
                    UserId = id,
                    IsPermitted = true,
                    GrantedAt = DateTime.Now
                });
            }
            else
            {
                existing.IsPermitted = true;
                existing.GrantedAt = DateTime.Now;
            }

            var request = _context.ExamRequests.FirstOrDefault(r => r.UserId == id);
            if (request != null) request.Status = "Approved";

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult RevokePermission(string id)
        {
            var existing = _context.ExamPermissions.FirstOrDefault(p => p.UserId == id);
            if (existing != null) existing.IsPermitted = false;

            var request = _context.ExamRequests.FirstOrDefault(r => r.UserId == id);
            if (request != null) request.Status = "Rejected";

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}