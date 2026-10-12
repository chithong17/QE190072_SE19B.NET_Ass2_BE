# TaskTrack Backend - PRN232 Assignment 1

Backend Web API cho hệ thống quản lý công việc **TaskTrack**, xây dựng trên nền tảng **.NET 9**, **Entity Framework Core**, và cơ sở dữ liệu **PostgreSQL**.

---

## 🏛️ Cấu trúc dự án (Architecture)

Dự án được thiết kế theo mô hình phân lớp rõ ràng (N-Tier Architecture):

```
QE190072_SE19B.NET_Ass1_BE/
├── TaskTrack.API/               # Presentation Layer
│   ├── Controllers/             # RESTful API Controllers (Departments, Projects, Tasks, Tags)
│   ├── Program.cs               # DI Container, CORS, DbContext, Swagger configuration
│   └── appsettings.json         # Cấu hình ứng dụng
├── TaskTrack.Service/           # Business Logic Layer
│   ├── DTOs/                    # Data Transfer Objects & Projection Mappings
│   ├── Interfaces/              # Service contracts (IDepartmentService, IProjectService, etc.)
│   └── Implementations/         # Business logic & constraint validations
└── TaskTrack.Repo/              # Data Access Layer
    ├── Models/                  # Entity Models & DbContext với Data Annotations
    └── Repositories/            # Generic Repository Pattern (IGenericRepository, GenericRepository)
```

---

## ✨ Tính năng chính (Key Features)

- **Departments Management**: CRUD, tìm kiếm theo tên, ràng buộc không cho xóa phòng ban đang có dự án liên kết.
- **Projects Management**: CRUD, lọc theo trạng thái/phòng ban, tính toán tiến độ, ràng buộc không cho xóa dự án có công việc liên kết.
- **Tasks Management**: CRUD, Soft-delete (`IsActive = false`), lọc đa điều kiện (Title, Status, Priority, Project, Tag).
- **Tags Management**: CRUD, gán nhiều tag cho công việc (Many-to-Many qua bảng trung gian `TaskTag`), ngăn xóa tag đang được sử dụng.
- **Validation chặt chẽ**:
  - Trả về mã lỗi HTTP 400 Bad Request kèm chi tiết lỗi từng trường (`Field-level Validation`) khi dữ liệu đầu vào không hợp lệ.
  - Ngăn xóa dữ liệu có ràng buộc khóa ngoại (Department có Project, Project có Task, Tag có Task).
- **OpenAPI / Swagger UI**: Tích hợp sẵn tài liệu tương tác API trực quan.

---

## ⚙️ Yêu cầu môi trường & Biến môi trường (Prerequisites & Environment)

### Yêu cầu:
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL Database (hoặc dịch vụ đám mây như Render PostgreSQL, Supabase, Neon)

### Biến môi trường (Environment Variables):
Tạo file `.env` hoặc cấu hình qua **User Secrets** / Environment Variables:

| Tên biến | Mô tả | Ví dụ |
| :--- | :--- | :--- |
| `DATABASE_URL` | Chuỗi kết nối PostgreSQL (dạng URL) | `postgresql://user:pass@host:5432/dbname` |
| `ConnectionStrings__DefaultConnection` | Chuỗi kết nối chuẩn .NET | `Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require` |
| `FRONTEND_URL` | URL của Frontend để cấu hình CORS | `https://your-app.vercel.app` hoặc `http://localhost:3000` |
| `ASPNETCORE_ENVIRONMENT` | Môi trường chạy | `Development` hoặc `Production` |

---

## 🚀 Hướng dẫn cài đặt và chạy Local

1. **Clone repository:**
   ```bash
   git clone https://github.com/chithong17/QE190072_SE19B.NET_Ass1_BE.git
   cd QE190072_SE19B.NET_Ass1_BE
   ```

2. **Cấu hình chuỗi kết nối Database (User Secrets):**
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=YOUR_HOST;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require" --project TaskTrack.API
   ```

3. **Biên dịch mã nguồn:**
   ```bash
   dotnet build
   ```

4. **Chạy Web API:**
   ```bash
   dotnet run --project TaskTrack.API --launch-profile http
   ```

5. **Truy cập Swagger UI:**
   - Swagger UI: [http://localhost:5093/swagger](http://localhost:5093/swagger)
   - API Base URL: `http://localhost:5093/api`

---

## 🌐 Triển khai (Deployment)

- **Nền tảng đề xuất:** [Render](https://render.com) hoặc [Railway](https://railway.app).
- **Cấu hình Render Web Service:**
  - **Docker (Khuyến nghị):** Render tự động nhận diện `Dockerfile` có sẵn trong root.
  - **Hoặc Native Build Command:** `dotnet publish -c Release -o out TaskTrack.API/TaskTrack.API.csproj`
  - **Start Command:** `dotnet out/TaskTrack.API.dll`
  - **Environment Variables:**
    - `DATABASE_URL`: Chuỗi kết nối PostgreSQL trên Render.
    - `ASPNETCORE_ENVIRONMENT`: `Production`
    - `FRONTEND_URL`: URL Frontend trên Vercel.

---

## 📊 Sơ đồ cơ sở dữ liệu (ERD - Entity Relationship Diagram)

```mermaid
erDiagram
    DEPARTMENT ||--o{ PROJECT : "has many"
    PROJECT ||--o{ TASK : "contains"
    TASK ||--o{ TASK_TAG : "has"
    TAG ||--o{ TASK_TAG : "categorizes"

    DEPARTMENT {
        int DepartmentID PK
        string DepartmentName
        string DepartmentDescription
        boolean IsActive
    }

    PROJECT {
        int ProjectID PK
        string ProjectName
        string Description
        date StartDate
        date EndDate
        int Status "0: Not Started, 1: In Progress, 2: Completed, 3: On Hold"
        int DepartmentID FK
        boolean IsActive
        datetime CreatedDate
    }

    TASK {
        int TaskID PK
        string Title
        string Description
        int Status "0: To Do, 1: In Progress, 2: Done, 3: Cancelled"
        int Priority "0: Low, 1: Medium, 2: High, 3: Critical"
        date DueDate
        int ProjectID FK
        boolean IsActive
        datetime CreatedDate
        datetime ModifiedDate
    }

    TAG {
        int TagID PK
        string TagName
        string Color
    }

    TASK_TAG {
        int TaskID PK, FK
        int TagID PK, FK
    }
```

---

## 🌟 Tính năng cộng điểm (Bonus Features)

- [x] **GitHub Actions CI/CD**: Tự động kiểm tra build trên mỗi commit và pull request (`.github/workflows/ci.yml`).
- [x] **ERD Diagram**: Tài liệu sơ đồ thực thể liên kết chuẩn xác trong `README.md`.
- [x] **Task Status & Priority Badges**: Trực quan hóa tiến độ bằng màu sắc chuyên nghiệp.
- [x] **Soft Delete**: Xóa mềm bảo toàn dữ liệu cho Task.
- [x] **Foreign Key Delete Prevention**: Chặn xóa khi có dữ liệu phụ thuộc với thông báo lỗi rõ ràng.

