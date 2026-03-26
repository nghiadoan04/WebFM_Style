# 👕 Hệ thống gợi ý sản phẩm thời trang ứng dụng AI

Ứng dụng web thương mại điện tử thời trang full-stack, tích hợp hệ thống gợi ý sản phẩm dựa trên AI nhằm cải thiện trải nghiệm tìm kiếm và khám phá sản phẩm của người dùng.

---

## 🚀 Tính năng chính

- 🔍 Gợi ý sản phẩm thông minh dựa trên độ tương đồng nội dung  
- 🧠 Ứng dụng AI với TF-IDF và cosine similarity  
- 🛒 Duyệt sản phẩm, lọc sản phẩm và quản lý đơn hàng  
- 📊 Dashboard quản trị theo dõi doanh thu và đơn hàng  

---

## 🤖 Hệ thống gợi ý

![Gợi ý](./images/goiy.png)

Triển khai phương pháp **Content-Based Filtering** để đề xuất sản phẩm phù hợp dựa trên đặc trưng nội dung như tên, danh mục và mô tả sản phẩm.

- Sử dụng **TF-IDF** để chuyển đổi dữ liệu văn bản thành vector  
- Áp dụng **cosine similarity** để tính toán độ tương đồng giữa các sản phẩm  
- Tạo ra các gợi ý sản phẩm phù hợp nhằm nâng cao trải nghiệm người dùng  

---

## 🔁 Sản phẩm tương tự

![Tương tự](./images/goiy1.png)

Hiển thị các sản phẩm tương tự dựa trên pipeline gợi ý:

**TF-IDF → Cosine Similarity → Content-Based Filtering**

- Giúp người dùng dễ dàng tìm thấy các sản phẩm liên quan  
- Cải thiện khả năng khám phá sản phẩm so với tìm kiếm truyền thống  

---

## 🖥️ Giao diện sản phẩm

![Sản phẩm](./images/phanloai.png)

- Xây dựng bằng **ASP.NET Core MVC + Bootstrap**  
- Giao diện responsive cho danh sách và lọc sản phẩm  
- Tối ưu trải nghiệm duyệt và tìm kiếm  

---

## 📊 Dashboard quản trị

![Thống kê](./images/thongke.png)

- Theo dõi doanh thu và số lượng đơn hàng  
- Hiển thị dữ liệu và hiệu suất hệ thống  

---

## 📦 Quản lý đơn hàng

![Đơn hàng](./images/donhang.png)

- Quản lý trạng thái và thông tin đơn hàng  
- Xây dựng bằng **ASP.NET Core MVC + SQL Server**  

---

## ⚙️ Công nghệ sử dụng

- **Backend:** ASP.NET Core MVC  
- **Frontend:** HTML, CSS, Bootstrap, JavaScript  
- **Database:** SQL Server  

---

## 🧠 Công nghệ cốt lõi

- Content-Based Filtering  
- TF-IDF (Term Frequency – Inverse Document Frequency)  
- Cosine Similarity  

---

## 📌 Điểm nổi bật

- Xây dựng hoàn chỉnh hệ thống thương mại điện tử tích hợp AI  
- Áp dụng các kỹ thuật xử lý văn bản (information retrieval) vào bài toán gợi ý thực tế  
- Cải thiện khả năng khám phá sản phẩm bằng gợi ý dựa trên độ tương đồng nội dung thay vì tìm kiếm từ khóa truyền thống  