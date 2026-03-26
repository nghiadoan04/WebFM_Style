# 👕 Hệ thống gợi ý sản phẩm thời trang ứng dụng AI

Ứng dụng web thương mại điện tử thời trang full-stack, tích hợp hệ thống gợi ý sản phẩm nhằm cải thiện trải nghiệm tìm kiếm và khám phá sản phẩm.  
Hệ thống sử dụng phương pháp Content-Based Filtering kết hợp với dữ liệu hành vi người dùng để đưa ra gợi ý phù hợp.

---

## 🚀 Tính năng chính

- 🔍 Gợi ý sản phẩm thông minh dựa trên nội dung sản phẩm  
- 🧠 Ứng dụng TF-IDF và cosine similarity để tính độ tương đồng  
- 👤 Sử dụng hành vi người dùng (mua hàng, xem sản phẩm) để cá nhân hóa gợi ý  
- 🛒 Duyệt sản phẩm, lọc sản phẩm và quản lý đơn hàng  
- 📊 Dashboard quản trị theo dõi doanh thu và đơn hàng  

---

## 🤖 Sản phẩm bạn có thể thích

![Gợi ý](./images/goiy.png)

Hệ thống gợi ý được xây dựng theo hướng **Content-Based Filtering**, kết hợp với hành vi người dùng để tạo ra danh sách sản phẩm phù hợp.

### 🔄 Quy trình hoạt động:

1. Thu thập **seed products** từ hành vi người dùng:
   - Sản phẩm đã mua  
   - Sản phẩm đã xem  

2. Biểu diễn dữ liệu sản phẩm:
   - Sử dụng **TF-IDF** để chuyển đổi mô tả, danh mục thành vector  

3. Tính toán độ tương đồng:
   - Áp dụng **cosine similarity** giữa các sản phẩm  

4. Sinh gợi ý:
   - Lấy các sản phẩm có độ tương đồng cao nhất với seed products  

---

## 🔁 Sản phẩm tương tự

![Tương tự](./images/goiy1.png)

Hiển thị các sản phẩm tương tự dựa trên nội dung của sản phẩm hiện tại:

- Sử dụng chính pipeline gợi ý (TF-IDF + Cosine Similarity)  
- Không phụ thuộc vào hành vi người dùng  
- Giúp người dùng khám phá nhanh các sản phẩm liên quan  

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
- **Machine Learning:** ML.NET (TF-IDF Featurization)  

---

## 🧠 Công nghệ cốt lõi

- Content-Based Filtering  
- TF-IDF (Term Frequency – Inverse Document Frequency)  
- Cosine Similarity  

---

## 📌 Điểm nổi bật

- Xây dựng hoàn chỉnh hệ thống thương mại điện tử tích hợp gợi ý sản phẩm  
- Kết hợp **hành vi người dùng + nội dung sản phẩm** để tăng độ chính xác  
- Áp dụng kỹ thuật **information retrieval** vào bài toán thực tế  
- Cải thiện khả năng khám phá sản phẩm so với tìm kiếm truyền thống  

---

## 🚧 Hướng phát triển

- Kết hợp thêm **Collaborative Filtering**  
- Xây dựng hệ thống gợi ý lai (Hybrid Recommendation System)  
- Tối ưu hiệu năng và độ chính xác của thuật toán  