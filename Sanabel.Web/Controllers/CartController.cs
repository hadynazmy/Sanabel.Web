﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanabel.Web.Data;
using Sanabel.Web.Helpers;
using Sanabel.Web.Models;
using Sanabel.Web.Enum;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Sanabel.Web.Services;

namespace Sanabel.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;


        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService, 
            ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return View(new List<CartItem>());

            return View(cart.Items.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    _context.Carts.Add(cart);
                }

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UserId = userId  // أضف هذا السطر
                    });

                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UserId = userId
                    });
                }

                HttpContext.Session.SetObject("Cart", cart);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // إضافة هذا الإكشن للحصول على عدد العناصر في السلة
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var count = cart?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            int cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(new { cartCount });
        }

        public async Task<IActionResult> ClearCart()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                cart.Items.Clear();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _cartService.GetCartItems(userId); // استخدام CartService هنا
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart"); // إذا كانت السلة فارغة، أعده إلى السلة
            }

            var order = new Order
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                TotalAmount = cartItems.Sum(x => x.Quantity * x.Product.Price),
                Status = OrderStatus.Pending, // استخدام OrderStatus بدلاً من string
                CreatedAt = DateTime.Now,
                Items = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _cartService.ClearCart(userId); // تنظيف السلة بعد تأكيد الطلب

            return RedirectToAction("Index", "Orders"); // التوجيه إلى صفحة الطلبات بعد الحفظ
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int productId, int quantity)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // تحميل السلة مع العناصر والمنتجات المرتبطة
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product) // هذه هي السطر الحاسم
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return Json(new { success = false, message = "السلة غير موجودة" });
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    return Json(new { success = false, message = "المنتج غير موجود في السلة" });
                }

                item.Quantity = quantity;
                await _context.SaveChangesAsync();

                int cartCount = cart.Items.Sum(i => i.Quantity);

                // حساب الإجمالي بعد التأكد من تحميل المنتجات
                decimal totalPrice = cart.Items
                    .Where(i => i.Product != null) // تأكد أن المنتج غير null
                    .Sum(i => i.Quantity * i.Product.Price);

                return Json(new
                {
                    success = true,
                    cartCount,
                    totalPrice = totalPrice.ToString("0.00")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحديث السلة");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحديث السلة",
                    error = ex.Message // إضافة رسالة الخطأ للتصحيح
                });
            }
        }
    }
}