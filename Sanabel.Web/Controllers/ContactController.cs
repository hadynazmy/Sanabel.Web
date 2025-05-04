using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sanabel.Web.Data;
using Sanabel.Web.Implementation;
using Sanabel.Web.Models;

namespace Sanabel.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailSender;
        private readonly ApplicationDbContext _context;

        public ContactController(IEmailService emailSender, ApplicationDbContext context)
        {
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(Contact model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // حفظ الرسالة في قاعدة البيانات
                    _context.Contacts.Add(model);
                    await _context.SaveChangesAsync();

                    // إرسال البريد الإلكتروني
                    var emailBody = $@"
                        <h2>رسالة جديدة من نموذج الاتصال</h2>
                        <p><strong>الاسم:</strong> {model.Name}</p>
                        <p><strong>البريد الإلكتروني:</strong> {model.Email}</p>
                        <p><strong>الهاتف:</strong> {model.Phone}</p>
                        <p><strong>الرسالة:</strong></p>
                        <p>{model.Message}</p>
                    ";

                    await _emailSender.SendEmailAsync(
                        "info@sanabel.com",
                        "رسالة جديدة من نموذج الاتصال",
                        emailBody);

                    TempData["SuccessMessage"] = "تم إرسال رسالتك بنجاح، شكراً لتواصلك معنا!";
                    return RedirectToAction(nameof(ContactUs));
                }
                catch
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء محاولة إرسال الرسالة. يرجى المحاولة مرة أخرى.");
                }
            }

            return View(model);
        }

        // في الـ Controller
        [Authorize(Roles = "Admin")]
        public IActionResult Messages()
        {
            var messages = _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return View(messages);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Contacts.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الرسالة بنجاح";
            return RedirectToAction(nameof(Messages));
        }
    }
}