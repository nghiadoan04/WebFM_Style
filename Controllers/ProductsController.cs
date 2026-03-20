using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Utilities;
using WebFM_Style.Models;
using WebFM_Style.Models.ViewModel;

namespace WebFM_Style.Controllers
{
    public class ProductsController : Controller
    {
        private FmStyleDbContext _context;
        private readonly IRecommendationService _recommendationService;
        public INotyfService _notyfService { get; }
        public ProductsController(FmStyleDbContext repo, INotyfService notyfService, IRecommendationService recommendationService)
        {
            _context = repo;
            _notyfService = notyfService;
            _recommendationService = recommendationService;
        }
        public async Task<IActionResult> Index(string? size, string? color, double? minPrice, double? maxPrice, string? sortOrder, string? category, string? search, int pageNumber = 1, int pageSize = 12)
        {
            var products = await _context.Products.Include(x => x.ProductType).ThenInclude(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.ProductSizeColors).ThenInclude(x => x.Color)
                .Include(x => x.ProductSizeColors).ThenInclude(x => x.Size).Where(x => x.Status == 1)
                .ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                             p.Id.ToString().Contains(search)).ToList();
            }

            if (!string.IsNullOrEmpty(size))
            {
                products = products.Where(p => p.ProductSizeColors != null &&
                                               p.ProductSizeColors.Any(psc => psc.Size != null && psc.Size.Size1 == size)).ToList();
            }
            if (!string.IsNullOrEmpty(color))
            {
                products = products.Where(p => p.ProductSizeColors != null &&
                                               p.ProductSizeColors.Any(psc => psc.Color != null && psc.Color.Color1?.Trim() == color.Trim())).ToList();
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
            }
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.ProductType.Category.Name == category).ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    break; // No sorting applied
            }

            // Phân trang
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            var pagedProducts = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Lưu thông tin vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.Sizes = await _context.Sizes.ToListAsync(); // Giả sử bạn có một bảng Sizes
            ViewBag.Category = _context.Categories.Include(x => x.ProductTypes).ThenInclude(x => x.Products).ThenInclude(x => x.ProductSizeColors).Take(6).ToList();  // sửa ở đây take là lấy 6 cái muốn lấy hết thì bỏ take
            return View(pagedProducts);
        }

public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.ProductSizeColors).ThenInclude(psc => psc.Size)
            .Include(p => p.ProductSizeColors).ThenInclude(psc => psc.Color)
            .Include(p => p.ProductType).ThenInclude(pt => pt.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _notyfService.Error("Sản phẩm không tồn tại.");
                return NotFound();
            }

            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                var view = new ProductView
                {
                    AccountId = userId.Value,
                    ProductId = id,
                    ViewTime = DateTime.Now,
                };

                _context.ProductViews.Add(view);
                await _context.SaveChangesAsync();
            }

            // 3. GỌI HÀM GỢI Ý (TF-IDF)
            // Lấy các sản phẩm có nội dung tương tự sản phẩm hiện tại
            var relatedProducts = await _recommendationService.GetProductDetailsRecommendations(id);
            
            // Truyền sang View bằng ViewBag
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        // Nếu bạn dùng AJAX để lấy chi tiết sản phẩm, cập nhật cả hàm này
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Images = p.Images.Select(img => img.Url).ToList(),
                    SizeColors = p.ProductSizeColors.Select(sc => new
                    {
                        Size = sc.Size,
                        Color = sc.Color,
                        Id = sc.Id
                    }).ToList()
                })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                var view = new ProductView
                {
                    AccountId = userId.Value,
                    ProductId = id,
                    ViewTime = DateTime.Now,
                };

                _context.ProductViews.Add(view);
                await _context.SaveChangesAsync();
            }

            // 4. GỌI HÀM GỢI Ý CHO API
            var relatedProducts = await _recommendationService.GetProductDetailsRecommendations(id);

            // Trả về JSON chứa cả thông tin sản phẩm và list gợi ý
            return Ok(new 
            { 
                product, 
                relatedProducts 
            });
        }
        public async Task<IActionResult> ShoppingCart()
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
                return RedirectToAction("Index", "Home");
            }
            var maKH = makhclaim.Value;
            var giohang = await _context.OrderItems.Include(x => x.ProductSizeColor)
                .ThenInclude(x => x.Product).ThenInclude(x => x.Images)
                .Include(x => x.ProductSizeColor).ThenInclude(x => x.Size)
                .ThenInclude(x => x.ProductSizeColors).ThenInclude(x => x.Color)
                .Include(x => x.Oder).ThenInclude(x => x.Account)
                .Where(x => x.Oder.AccountId == int.Parse(maKH) && x.Oder.Status == 1 && x.ProductSizeColorId != null).ToListAsync();
            if (giohang == null)
            {
                _notyfService.Warning("Bạn chưa có sản phẩm nào trong giỏ hàng");

                return RedirectToAction("Index", "Home");

            }
            var addresses = _context.Addresses
                .Where(x => x.AccountId == int.Parse(maKH))
                .Select(x => new AddressViewModel
                {
                    Id = x.Id,
                    Street = x.Street,
                    Ward = x.Ward,
                    District = x.District,
                    City = x.City,
                })
                .ToList();
            ViewData["AddressId"] = new SelectList(addresses, "Id", "FormattedAddress");
            ViewBag.Giohang = giohang;
            return View(giohang);
        }
        public IActionResult CheckOut(int id)
        {
            return View();
        }
        public IActionResult CheckOut(string pay, int Address)
        {

            return View();
        }
        private int? GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
