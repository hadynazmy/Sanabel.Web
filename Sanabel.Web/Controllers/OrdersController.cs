using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sanabel.Web.Data;
using Sanabel.Web.Enum;
using Sanabel.Web.Implementation;
using Sanabel.Web.Models;
using Sanabel.Web.ViewModels;
using System.Security.Claims;

namespace Sanabel.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrdersController> _logger;
        private readonly IEmailService _emailSender;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<OrdersController> logger,
            IEmailService emailSender)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }


        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);


            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CreatedAt,TotalAmount,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CreatedAt,TotalAmount,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (System.Enum.TryParse<Sanabel.Web.Enum.OrderStatus>(status, out var parsedStatus))
            {
                order.Status = parsedStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(ConfirmOrderViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var order = new Order
            {
                UserId = userId,
                Location = model.Address,
                Notes = model.Notes,
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // إزالة عناصر السلة بعد تأكيد الطلب
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult GetTotalOrdersCount()
        {
            try
            {
                var count = _context.Orders.Count();
                return Content(count.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total orders count");
                return Content("0");
            }
        }

        [HttpPost]
        public IActionResult GetOrdersData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();

                var query = _context.Orders.Include(o => o.User).AsQueryable();

                int recordsTotal = query.Count();

                var data = query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip(start != null ? Convert.ToInt32(start) : 0)
                    .Take(length != null ? Convert.ToInt32(length) : 10)
                    .Select(o => new
                    {
                        id = o.Id,
                        user = new { o.User.FirstName, o.User.LastName },
                        createdAt = o.CreatedAt,
                        totalAmount = o.TotalAmount,
                        status = o.Status.ToString()
                    })
                    .ToList();

                return Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrdersData");
                return Json(new
                {
                    draw = 0,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var order = await _context.Orders.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
                if (order == null)
                {
                    _logger?.LogWarning("Order not found with ID: {OrderId}", id);
                    return NotFound();
                }

                // حفظ الحالة القديمة قبل التغيير
                var oldStatus = order.Status;

                // تغيير الحالة
                order.Status = order.Status == OrderStatus.Accepted
                    ? OrderStatus.Rejected
                    : OrderStatus.Accepted;

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Order status changed for ID: {OrderId}, New Status: {Status}",
                    id, order.Status);

                // إرسال الإيميل المناسب بناءً على الحالة الجديدة
                if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
                {
                    string subject, message;

                    if (order.Status == OrderStatus.Accepted)
                    {
                        subject = "تم قبول طلبك";
                        message = $"مرحباً {order.User.FullName}،<br/><br/>" +
                                  $"نود إعلامك أنه تم قبول طلبك رقم {order.Id}.<br/>" +
                                  "سيتم تجهيز طلبك وإبلاغك عند شحنه.<br/><br/>" +
                                  "شكراً لثقتك بنا!";
                    }
                    else
                    {
                        subject = "تم رفض طلبك";
                        message = $"مرحباً {order.User.FullName}،<br/><br/>" +
                                  $"نأسف لإبلاغك أنه تم رفض طلبك رقم {order.Id}.<br/>" +
                                  "للاستفسار عن سبب الرفض، يرجى التواصل مع خدمة العملاء.<br/><br/>" +
                                  "مع أطيب التحيات";
                    }

                    try
                    {
                        await _emailSender.SendEmailAsync(order.User.Email, subject, message);
                        _logger?.LogInformation("Email sent successfully for order ID: {OrderId}", id);
                    }
                    catch (Exception emailEx)
                    {
                        _logger?.LogError(emailEx, "Failed to send email for order ID: {OrderId}", id);
                        // يمكنك اختيار إرجاع خطأ أو الاستمرار رغم فشل الإيميل
                    }
                }

                return Ok(new
                {
                    newStatus = order.Status.ToString(),
                    buttonText = order.Status == OrderStatus.Accepted ? "مقبولة" : "مرفوضة",
                    buttonClass = order.Status == OrderStatus.Accepted ? "btn-success" : "btn-danger"
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error changing status for order ID: {OrderId}", id);
                return StatusCode(500, "حدث خطأ أثناء تغيير حالة الطلب");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Accepted; // وليس "مقبولة" كـ string

            await _context.SaveChangesAsync();

            

            return RedirectToAction("Index");
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
