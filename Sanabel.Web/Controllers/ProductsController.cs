using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sanabel.Web.Data;
using Sanabel.Web.Models;

namespace Sanabel.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ProductController.cs

        public async Task<IActionResult> Index(string? searchTerm, int? subCategoryId, int page = 1)
        {
            int pageSize = 6;

            var productsQuery = _context.Products
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchTerm));
            }

            if (subCategoryId.HasValue && subCategoryId.Value != 0)
            {
                productsQuery = productsQuery.Where(p => p.SubCategoryId == subCategoryId);
            }

            // ترتيب النتائج لتجنب مشاكل Skip/Take
            productsQuery = productsQuery.OrderBy(p => p.Id);

            int totalProducts = await productsQuery.CountAsync(); // إجمالي عدد المنتجات
            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // تمرير البيانات إلى View
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentSubCategoryId = subCategoryId ?? 0;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.ProductsInPage = products.Count;

            ViewBag.SubCategories = await _context.SubCategories
                .Include(sc => sc.Category)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name");
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // حدد مكان الحفظ
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\products");
                    Directory.CreateDirectory(uploadsFolder); // يتأكد ان الفولدر موجود

                    // اسم الصورة هيكون مميز عشان مفيش صورة تغطي على الثانية
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // نحط اسم الصورة في قاعدة البيانات
                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,SubCategoryId")] Product product, IFormFile? ImageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // إذا تم رفع صورة جديدة
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        Directory.CreateDirectory(uploadsFolder); // للتأكد من وجود المجلد
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/images/" + fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
