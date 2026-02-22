# 🔐 Hướng Dẫn Đăng Nhập & Đăng Ký

## ✅ Hệ Thống Đã Hoàn Thành

### 📦 Database
- ✅ Database `LaptopHub_DB` đã được tạo tự động
- ✅ Migrations đã được apply
- ✅ Auto migration khi khởi động app
- ✅ Retry policy cho SQL Server (5 retries, 30s delay)

### 👤 Tài Khoản Admin Mặc Định
```
Email: admin@laptophub.vn
Password: Admin@123
```

### 🚀 Các Tính Năng

#### 1. Đăng Nhập (`/Account/Login`)
- ✅ Đăng nhập bằng Email & Password
- ✅ Remember Me (ghi nhớ đăng nhập 7 ngày)
- ✅ Phân quyền tự động:
  - **Admin** → redirect về `/Admin/Dashboard`
  - **User** → redirect về trang chủ
- ✅ ReturnUrl support
- ✅ Validation & error messages

#### 2. Đăng Ký (`/Account/Register`)
- ✅ Đăng ký tài khoản mới
- ✅ Tự động thêm role "User"
- ✅ Auto login sau khi đăng ký
- ✅ Password validation:
  - Tối thiểu 6 ký tự
  - Phải có chữ số
  - Phải có chữ thường
  - Không bắt buộc chữ hoa
  - Không bắt buộc ký tự đặc biệt
- ✅ Email unique validation

#### 3. Quên Mật Khẩu (`/Account/ForgotPassword`)
- ✅ Generate password reset token
- ⚠️ TODO: Cần setup email service để gửi link reset

#### 4. Đăng Xuất (`/Account/Logout`)
- ✅ POST method với anti-forgery token
- ✅ Clear authentication cookie

#### 5. Access Denied (`/Account/AccessDenied`)
- ✅ Trang thông báo khi không có quyền truy cập

## 🧪 Test Ngay

### Bước 1: Chạy ứng dụng
```bash
dotnet run
```

### Bước 2: Test Đăng Nhập Admin
1. Truy cập: `https://localhost:5001/Account/Login`
2. Đăng nhập với:
   - Email: `admin@laptophub.vn`
   - Password: `Admin@123`
3. Sẽ redirect về `/Admin/Dashboard`

### Bước 3: Test Đăng Ký User Mới
1. Truy cập: `https://localhost:5001/Account/Register`
2. Điền thông tin:
   - Họ tên: Nguyễn Văn A
   - Email: user@test.com
   - Password: user123
   - Confirm: user123
3. Tự động login và redirect về trang chủ

### Bước 4: Test Logout
1. Nhấn nút Logout (có anti-forgery token)
2. Redirect về trang login

## 🔒 Bảo Mật

- ✅ Cookie authentication (7 ngày expiry)
- ✅ Anti-forgery tokens
- ✅ Password hashing (Identity default)
- ✅ Email confirmation (có thể bật/tắt)
- ✅ Role-based authorization
- ✅ Lockout support (hiện tại tắt)

## 📁 Cấu Trúc Files

```
WebsiteSellLaptop/
├── Controllers/
│   └── AccountController.cs          ← Controller chính
├── Views/
│   └── Account/
│       ├── Login.cshtml              ← Giao diện đăng nhập
│       ├── Register.cshtml           ← Giao diện đăng ký
│       ├── ForgotPassword.cshtml     ← Quên mật khẩu
│       └── AccessDenied.cshtml       ← Từ chối truy cập
├── Data/
│   └── AppDbContext.cs               ← Database context
├── Models/
│   └── Entities/
│       └── AppUser.cs                ← User entity
├── Program.cs                        ← Config & seeding
└── Migrations/                       ← EF Core migrations
```

## 🎨 UI/UX

- ✅ Modern, clean design
- ✅ Responsive (mobile-friendly)
- ✅ Bootstrap 5.3.3
- ✅ Bootstrap Icons
- ✅ Manrope font family
- ✅ Error messages hiển thị đẹp
- ✅ Success messages

## ⚡ Tính Năng Nâng Cao (TODO)

- [ ] Email verification
- [ ] Password reset via email
- [ ] Two-factor authentication (2FA)
- [ ] Social login (Google, Facebook)
- [ ] Account lockout after failed attempts
- [ ] Password strength meter
- [ ] Remember me security improvements
- [ ] Audit logging

## 🐛 Troubleshooting

### Lỗi: Cannot open database
**Giải pháp**: Database đã được tạo tự động. Nếu vẫn lỗi:
```bash
dotnet ef database update
```

### Lỗi: Admin account không tồn tại
**Giải pháp**: Chạy lại app, admin sẽ được tạo tự động trong `Program.cs`

### Lỗi: Login không redirect đúng
**Kiểm tra**: 
- User có role Admin không?
- ReturnUrl có đúng format không?

## 📞 Support

Nếu có vấn đề, kiểm tra:
1. Database connection string trong `appsettings.json`
2. Migration đã apply chưa
3. Build có lỗi không
4. Console logs

---
**Happy Coding!** 🚀
