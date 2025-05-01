using Microsoft.AspNetCore.Mvc;
using Sanabel.Web.Data;
using Sanabel.Web.Helpers;
using Sanabel.Web.Models;

namespace Sanabel.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        public IActionResult AddToCart(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = 1 });
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult UpdateCart(int productId, int quantity)
        {
            // جلب السلة من الـ Session
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;  // تحديث الكمية
            }

            // حفظ التحديثات في الـ Session
            HttpContext.Session.SetObject(CartSessionKey, cart);

            // تحديث عدد العناصر في السلة
            int cartCount = cart.Sum(x => x.Quantity);
            HttpContext.Session.SetInt32("CartCount", cartCount);  // تخزين العدد في الـ Session

            // إعادة العدد الجديد للعناصر في السلة
            return Json(new { cartCount });
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);  // حذف المنتج من السلة
            }

            // حفظ التحديثات في الـ Session
            HttpContext.Session.SetObject(CartSessionKey, cart);

            // تحديث عدد العناصر في السلة
            int cartCount = cart.Sum(x => x.Quantity);
            HttpContext.Session.SetInt32("CartCount", cartCount);  // تخزين العدد في الـ Session

            return Json(new { cartCount });
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index");
        }
    }
}