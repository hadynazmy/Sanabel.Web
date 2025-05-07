// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Sanabel.Web.Models;

namespace Sanabel.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        // تعريف الخصائص اللازمة لإدارة تسجيل الدخول والمستخدمين وإرسال البريد الإلكتروني وتسجيل الأحداث.
        private readonly SignInManager<ApplicationUser> _signInManager; // مسؤول عن إدارة تسجيل الدخول.
        private readonly UserManager<ApplicationUser> _userManager; // مسؤول عن إدارة المستخدمين.
        private readonly IUserStore<ApplicationUser> _userStore; // مسؤول عن تخزين بيانات المستخدم.
        private readonly IUserEmailStore<ApplicationUser> _emailStore; // مسؤول عن تخزين البريد الإلكتروني.
        private readonly ILogger<RegisterModel> _logger; // لتسجيل الأحداث.
        private readonly IEmailSender _emailSender; // لإرسال رسائل البريد الإلكتروني.

        // المُنشئ لتلقي الكائنات الضرورية من خلال حقن التبعيات.
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager; // تهيئة كائن UserManager.
            _userStore = userStore; // تهيئة كائن UserStore.
            _emailStore = GetEmailStore(); // استدعاء وظيفة استرجاع EmailStore.
            _signInManager = signInManager; // تهيئة كائن SignInManager.
            _logger = logger; // تهيئة كائن Logger.
            _emailSender = emailSender; // تهيئة كائن EmailSender.
        }

        // النموذج الذي يحتوي على بيانات الإدخال التي يدخلها المستخدم.
        [BindProperty]
        public InputModel Input { get; set; }

        // رابط إعادة التوجيه بعد التسجيل.
        public string ReturnUrl { get; set; }

        // قائمة بأنظمة تسجيل الدخول الخارجية (مثل Google أو Facebook).
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // تعريف الحقول اللازمة لتسجيل المستخدم.
        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        // يتم استدعاؤها عند تحميل صفحة التسجيل.
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl; // تعيين رابط الإعادة إذا تم تحديده.
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // جلب خيارات تسجيل الدخول الخارجية.
        }

        // يتم استدعاؤها عند إرسال بيانات التسجيل من قبل المستخدم.
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    FullName = Input.FullName,
                    PhoneNumber = Input.PhoneNumber,
                    EmailConfirmed = true // تأكيد البريد الإلكتروني تلقائيًا
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "User");

                    // تسجيل الدخول تلقائيًا بعد التسجيل
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        // دالة لإنشاء كائن مستخدم جديد.
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>(); // محاولة إنشاء الكائن.
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // جلب التخزين الخاص بالبريد الإلكتروني.
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail) // التحقق من دعم البريد الإلكتروني.
            {
                throw new NotSupportedException("The default UI requires a user store with email support."); // إذا لم يكن البريد الإلكتروني مدعومًا.
            }
            return (IUserEmailStore<ApplicationUser>)_userStore; // إرجاع التخزين الخاص بالبريد الإلكتروني.
        }
    }
}
