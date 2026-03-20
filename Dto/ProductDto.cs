using System.ComponentModel.DataAnnotations;

namespace WebFM_Style.Dto
{
    public class ProductRequestDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        public double? Price { get; set; }

        public byte? Status { get; set; }

        [Required(ErrorMessage = "Loại sản phẩm là bắt buộc")]
        public int? ProductTypeId { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public byte? Status { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
