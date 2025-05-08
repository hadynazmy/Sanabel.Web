using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanabel.Web.Data;
using Sanabel.Web.Models;
using Sanabel.Web.ViewModels;
using System.Security.Claims;

namespace Sanabel.Web.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize] // يتطلب تسجيل الدخول
        public IActionResult Create()
        {
            return View(new FeedbackViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                byte[] profilePicture = user.ProfilePicture;

                // لو المستخدم مش حاطط صورة، نقرأ صورة افتراضية من wwwroot/images/avatar.png
                if (profilePicture == null)
                {
                    var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar.png");

                    if (System.IO.File.Exists(avatarPath))
                    {
                        profilePicture = await System.IO.File.ReadAllBytesAsync(avatarPath);
                    }
                }

                var feedback = new Feedback
                {
                    UserId = user.Id,
                    Message = model.Message,
                    FullName = user.FullName ?? $"{user.FirstName} {user.LastName}",
                    ProfilePicture = profilePicture,
                    CreatedAt = DateTime.Now
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "شكراً لك على تقييمك!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ التقييم. يرجى المحاولة مرة أخرى.");
                return View(model);
            }
        }

        [AllowAnonymous] // يسمح للزوار بمشاهدة التقييمات
        public IActionResult Index()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .Take(10)
                .ToList();

            return View(feedbacks);
        }
    }
}