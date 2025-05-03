using Microsoft.EntityFrameworkCore;
using Sanabel.Web.Data;
using Sanabel.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanabel.Web.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartItem>> GetCartItems(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.ToList() ?? new List<CartItem>();
        }

        public void ClearCart(string userId)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart != null)
            {
                cart.Items.Clear();
                _context.SaveChanges();
            }
        }
    }
}
