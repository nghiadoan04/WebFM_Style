using AspNetCoreHero.ToastNotification.Abstractions;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using WebFM_Style.Models;
using WebFM_Style.Models.ViewModel.ViewModel;
using WebFM_Style.Services;

namespace WebFM_Style.Controllers
{
    public class OrdersController : Controller
    {
        private readonly FmStyleDbContext _context;
        public INotyfService _notyfService { get; }  private readonly IVnPayService _vnPayService;
        public OrdersController(FmStyleDbContext context, INotyfService notyfService,IVnPayService vnPayService)
        {

            _context = context;
            _notyfService = notyfService;
           _vnPayService = vnPayService;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string size, string color, int quantity)
        {
            try
            {
                var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");

                if (makhclaim == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var maKH = makhclaim.Value;

                var productColorSize = _context.ProductSizeColors.Include(x => x.Size)
                    .Include(x => x.Color).FirstOrDefault(x => x.ProductId == productId && x.Color.Color1 == color && x.Size.Size1 == size);
                if (productColorSize == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }
                var dondathang = await _context.Orders.FirstOrDefaultAsync(x => x.AccountId == int.Parse(maKH) && x.Status == 1);

                if (dondathang == null)
                {
                    dondathang = new Order
                    {
                        AccountId = int.Parse(maKH),
                        Status = 1,

                    };
                    _context.Orders.Add(dondathang);
                    await _context.SaveChangesAsync();
                }

                var chitietdonthang = await _context.OrderItems.FirstOrDefaultAsync(x => x.OrderId == dondathang.Id && x.ProductSizeColorId == productColorSize.Id);

                if (chitietdonthang == null)
                {
                    chitietdonthang = new OrderItem
                    {
                        OrderId = dondathang.Id,
                        ProductSizeColorId = productColorSize.Id,
                        Quantity = quantity,
                    };

                    _context.OrderItems.Add(chitietdonthang);
                    await _context.SaveChangesAsync();

                }
                else
                {
                    // Sách đã có trong chi tiết đơn hàng, tăng số lượng lên một đơn vị
                    chitietdonthang.Quantity++;
                    await _context.SaveChangesAsync();
                }
                _notyfService.Success("Thêm sản phẩm thành công ");

                return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public async Task<IActionResult> UpdateOrder(int id)
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập");
                return RedirectToAction("Login", "Accounts");
            }
            var maKH = int.Parse(makhclaim.Value);

            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.AccountId == maKH && x.Status == 1);

            if (order == null)
            {
                _notyfService.Error("Không tìm thấy đơn hàng");
                return RedirectToAction("Index", "Home");
            }

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(x => x.OrderId == order.Id && x.ProductSizeColorId == id);

            if (orderItem == null)
            {
                _notyfService.Error("Không tìm thấy sản phẩm trong giỏ hàng");
                return RedirectToAction("ShoppingCart", "Products");
            }

            // Giảm số lượng
            orderItem.Quantity--;

            // Nếu số lượng = 0, xóa item khỏi giỏ hàng
            if (orderItem.Quantity <= 0)
            {
                _context.OrderItems.Remove(orderItem);
                _notyfService.Success("Đã xóa sản phẩm khỏi giỏ hàng");
            }
            else
            {
                _notyfService.Success("Đã cập nhật số lượng");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ShoppingCart", "Products");
        }
        public IActionResult UpdateOrderAdd(int? id)
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
                return RedirectToAction("Index", "Home");
            }
            var maKH = makhclaim.Value;

            // Truy vấn đơn hàng chưa hoàn thành
            var dondathang = _context.OrderItems
                .Include(x => x.Oder)
                .Where(x => x.Oder.AccountId == int.Parse(maKH) && x.Oder.Status == 1);

            OrderItem? sanpham = dondathang.FirstOrDefault(x => x.ProductSizeColorId == id);


            if (sanpham != null)
            {
                sanpham.Quantity = sanpham.Quantity + 1;
                _context.SaveChanges(); // Lưu thay đổi số lượng vào cơ sở dữ liệu
            }
            return RedirectToAction("ShoppingCart", "Products");
        }
        public IActionResult RemoveCart()
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
                return RedirectToAction("Index", "Home");
            }
            var maKH = makhclaim.Value;

            // Truy vấn đơn hàng chưa hoàn thành
            var dondathang = _context.OrderItems
                .Include(x => x.Oder)
                .Where(x => x.Oder.AccountId == int.Parse(maKH)
                && x.Oder.Status == 1);
            // Xóa hết các mục trong danh sách đơn đặt hàng
            _context.OrderItems.RemoveRange(dondathang);
            _context.SaveChanges();
            _notyfService.Success("Xóa giỏ hàng thành công");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult RemoveProduct(int id)
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
                return RedirectToAction("Index", "Home");
            }
            var maKH = makhclaim.Value;

            // Truy vấn đơn hàng chưa hoàn thành
            var dondathang = _context.OrderItems
                .Include(x => x.Oder)
                .Where(x => x.Oder.AccountId == int.Parse(maKH) && x.Oder.Status == 1);

            OrderItem? sanpham = dondathang.FirstOrDefault(x => x.ProductSizeColorId == id);

            if (sanpham != null)
            {
                _context.OrderItems.Remove(sanpham);
                _context.SaveChanges();
                return RedirectToAction("ShoppingCart", "Products");
            }
            if (dondathang == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("ShoppingCart", "Products");
        }


        public async Task<IActionResult> CheckOut(int pay, int Address, decimal total)
        {
            var makhclaim = User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (makhclaim == null)
            {
                _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
                return RedirectToAction("Index", "Home");
            }
            var maKH = makhclaim.Value;
            var giohang = await _context.Orders
                .Include(x => x.Account).ThenInclude(x => x.Addresses)
                .FirstOrDefaultAsync(x => x.AccountId == int.Parse(maKH) && x.Status == 1);

            if (giohang == null)
            {
                _notyfService.Warning("Bạn chưa có sản phẩm nào trong giỏ hàng");

                return RedirectToAction("Index", "Home");

            }
            var orderItems = _context.OrderItems
               .Include(x => x.ProductSizeColor)
                   .ThenInclude(x => x.ProductInventory).Include(x => x.ProductSizeColor)
                   .ThenInclude(x => x.Product)
               .Where(x => x.OrderId == giohang.Id)
               .ToList();

            foreach (var item in orderItems)
            {
                if ((item.ProductSizeColor.ProductInventory.QuantitySold + item.Quantity) > item.ProductSizeColor.ProductInventory.Quantity)
                {
                    var soluongsp = item.ProductSizeColor.ProductInventory.Quantity - item.ProductSizeColor.ProductInventory.QuantitySold;
                    _notyfService.Error($"{item.ProductSizeColor.Product.Name} chỉ còn {soluongsp} sản phẩm");
                    return RedirectToAction("ShoppingCart", "Products");
                }

                item.ProductSizeColor.ProductInventory.QuantitySold += item.Quantity;
            }
            await _context.SaveChangesAsync();



            giohang.Status = 2;
            giohang.CreateDay = DateTime.Now;
            giohang.AddressId = Address;
            giohang.Total = total;

            Payment payment = new Payment
            {
                OrdersId = giohang.Id,
                PaymentMethodsId = pay,
                Amount = giohang.Total,
                PaymentDate = DateTime.Now,
                Status = 1
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            if (pay == 2)
            {
                var VnPayModel = new VnPaymentRequestModel
                {
                    Amount =(double)(giohang.Total),
                    CreatedDate = DateTime.Now,
                    Description = $"{giohang.Account.FullName}",
                    FullName = giohang.Account.FullName,
                    OrderId = giohang.Id

                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, VnPayModel));

            }

            _notyfService.Success("Đặt hàng thành công");
            var address = _context.Addresses.FirstOrDefault(x => x.Id == Address);
            // Gửi email thông báo đặt hàng thành công
            var email = giohang.Account.Email; // Lấy email từ tài khoản
            var emailSubject = "Đặt hàng thành công";
            var emailBody = $@"
                <h1>Cảm ơn bạn đã đặt hàng!</h1>
                <p>Đơn hàng của bạn đã được đặt thành công với thông tin như sau:</p>
                <p><strong>Tổng cộng:</strong> {total} VNĐ</p>
                <p><strong>Địa chỉ giao hàng:</strong> {address.Street} -{address.Ward} -{address.District} -{address.City}-{address.Country} </p>
                <p>Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.</p>
                <p>Trân trọng,<br>Đội ngũ hỗ trợ khách hàng</p>
            ";

            await SendEmailAsync(email, emailSubject, emailBody); 

            return RedirectToAction("Index", "Home");
        }

        // Phương thức gửi email
        private async Task SendEmailAsync(string email, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AdminDotnet", "admin@example.com"));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            message.Body = new TextPart("html") // Sử dụng định dạng HTML
            {
                Text = body
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("khangchannel19@gmail.com", "jnal cnyl mlit izco");
            await smtp.SendAsync(message);
            smtp.Disconnect(true);
        }


          [Authorize]
  public async Task< IActionResult> PaymentCallBack()
  {
      var response = _vnPayService.PaymentExecute(Request.Query);
      if (response == null || response.VnPayResponseCode !=  "00")
      {
          var makhClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");

          if (makhClaim == null)
          {
              _notyfService.Error("Vui lòng đăng nhập trước khi mua hàng");
              return RedirectToAction("Index", "Home");
          }

          var maKH = makhClaim.Value;
          var giohang = await _context.Orders
              .Include(x => x.Account).Include(x => x.Payments).OrderByDescending(x => x.CreateDay)
              .FirstOrDefaultAsync(x => x.AccountId == int.Parse(maKH) && x.Status == 2);
          giohang.Total = null;
          giohang.CreateDay = null;
          giohang.AddressId = null;
          giohang.Discount = null;
          giohang.Status = 1;
        
          _context.Payments.RemoveRange(giohang.Payments);
          await _context.SaveChangesAsync();
          _notyfService.Error($"Lỗi thanh toán VNPAY: {response.VnPayResponseCode}");
          return RedirectToAction("Index", "Home");
      }

      _notyfService.Success("Thanh toán thành công");
      return RedirectToAction("Index", "Home");
  }
    }
}
