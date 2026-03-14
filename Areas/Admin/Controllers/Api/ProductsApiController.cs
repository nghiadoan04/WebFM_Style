using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFM_Style.Dto;
using WebFM_Style.Helper;
using WebFM_Style.Models;

namespace WebFM_Style.Areas.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly FmStyleDbContext _context;

        public ProductsController(FmStyleDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.Images)
                .Where(p => p.Status != 2) // Không lấy sản phẩm ẩn/xóa (tuỳ thiết kế của bạn, ở đây giả lập xoá là 2)
                .ToListAsync();

            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Status = p.Status,
                ProductTypeId = p.ProductTypeId,
                ProductTypeName = p.ProductType?.Name,
                ImageUrls = p.Images.Select(img => img.Url).ToList()
            }).ToList();

            return Ok(response);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            var p = await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (p == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm." });
            }

            var response = new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Status = p.Status,
                ProductTypeId = p.ProductTypeId,
                ProductTypeName = p.ProductType?.Name,
                ImageUrls = p.Images.Select(img => img.Url).ToList()
            };

            return Ok(response);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> PostProduct([FromForm] ProductRequestDto productDto, List<IFormFile> fAvatars)
        {
            if (_context.Products.Any(p => p.Name == productDto.Name))
            {
                return BadRequest(new { message = "Tên sản phẩm đã tồn tại." });
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Status = productDto.Status ?? 1,
                ProductTypeId = productDto.ProductTypeId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Handle images
            var listImages = new List<Image>();
            int i = 1;
            if (fAvatars != null && fAvatars.Count > 0)
            {
                foreach (var file in fAvatars)
                {
                    if (file.Length > 0)
                    {
                        var imagemodel = new Image();
                        string extennsion = Path.GetExtension(file.FileName);
                        string imgName = Utilities.ToUrlFriendly(product.Name + i) + extennsion;
                        imagemodel.Url = await Utilities.UploadFile(file, @"Product", imgName.ToLower());
                        imagemodel.ProductId = product.Id;
                        imagemodel.Status = true;
                        listImages.Add(imagemodel);
                        i++;
                    }
                }
                _context.Images.AddRange(listImages);
                await _context.SaveChangesAsync();
            }

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = product.Status,
                ProductTypeId = product.ProductTypeId,
                ImageUrls = listImages.Select(img => img.Url).ToList()
            };

            // return dto without cycle reference
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductRequestDto productDto, List<IFormFile> images)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm." });
            }

            if (_context.Products.Any(p => p.Name == productDto.Name && p.Id != id))
            {
                return BadRequest(new { message = "Tên sản phẩm đã đăng ký cho một sản phẩm khác." });
            }

            // Cập nhật thông tin
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            if (productDto.Status.HasValue) product.Status = productDto.Status.Value;
            product.ProductTypeId = productDto.ProductTypeId;

            _context.Products.Update(product);

            // Cập nhật hình ảnh nếu có
            if (images != null && images.Count > 0)
            {
                var imageDB = _context.Images.Where(x => x.ProductId == id).ToList();
                _context.Images.RemoveRange(imageDB);
                await _context.SaveChangesAsync();

                var listImages = new List<Image>();
                int i = 1;
                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        var imagemodel = new Image();
                        string extennsion = Path.GetExtension(file.FileName);
                        string imgName = Utilities.ToUrlFriendly(product.Name + i) + extennsion;
                        imagemodel.Url = await Utilities.UploadFile(file, @"Product", imgName.ToLower());
                        imagemodel.ProductId = product.Id;
                        imagemodel.Status = true;
                        listImages.Add(imagemodel);
                        i++;
                    }
                }
                _context.Images.AddRange(listImages);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm." });
            }

            // Tuỳ nghiệp vụ: Hard Delete (Remove) hoặc Soft Delete (Status = 2)
            // Trong code cũ dùng Status = 2
            product.Status = 2;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật trạng thái xóa sản phẩm thành công" });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
