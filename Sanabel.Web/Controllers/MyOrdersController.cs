using Microsoft.AspNetCore.Mvc;
using Sanabel.Web.Models;
using Microsoft.AspNetCore.Identity;
using Sanabel.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Sanabel.Web.Controllers
{
    public class MyOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyOrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // صفحة لعرض طلبات المستخدم
        public async Task<IActionResult> Index()
        {
            // الحصول على المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);

            // جلب جميع الطلبات للمستخدم الحالي
            var orders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items) // تحميل العناصر المرتبطة بكل طلب
                .ThenInclude(i => i.Product) // تحميل المنتج المرتبط بكل عنصر
                .ToList();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            // جلب الطلب مع عناصره
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderDetails(IFormFile PaymentImage, int OrderId, decimal ShippingCost, decimal DepositAmount)
        {
            var order = await _context.Orders.FindAsync(OrderId);
            if (order == null) return NotFound();

            if (PaymentImage != null && PaymentImage.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PaymentImage.CopyToAsync(memoryStream);
                    order.PaymentImage = memoryStream.ToArray();

                    // تنظيف اسم الملف
                    order.PaymentImageName = WebUtility.UrlEncode(Path.GetFileName(PaymentImage.FileName));

                    // التأكد من وجود نوع محتوى صالح
                    order.PaymentImageType = !string.IsNullOrWhiteSpace(PaymentImage.ContentType) ?
                        PaymentImage.ContentType : "image/jpeg";
                }
            }

            order.ShippingCost = ShippingCost;
            order.DepositAmount = DepositAmount;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

            //public async Task<IActionResult> GetPaymentImage(int id)
            //{
            //    var order = await _context.Orders.FindAsync(id);
            //    if (order == null || order.PaymentImage == null)
            //        return NotFound();

            //    return File(order.PaymentImage, order.PaymentImageType ?? "image/jpeg", order.PaymentImageName);
            //}


    }
}
