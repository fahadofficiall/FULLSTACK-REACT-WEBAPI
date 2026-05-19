# Products Manager — Fullstack App

A fullstack CRUD application built with:

- **Frontend**: React 18 + React Router v6 + Axios + JWT
- **Backend**: .NET 8 Web API + ADO.NET (raw SQL, no ORM) + Repository Pattern
- **Database**: SQL Server (SSMS-compatible)
- **Auth**: JWT Bearer tokens + BCrypt password hashing

---

## Project Structure

```
fullstack-app/
├── database/
│   └── setup.sql                   # Run this first in SSMS
│
├── backend/
│   └── ProductsApi/
│       ├── ProductsApi.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   └── ProductsController.cs
│       ├── Models/
│       │   ├── User.cs
│       │   └── Product.cs
│       ├── DTOs/
│       │   └── ProductDtos.cs
│       ├── Interfaces/
│       │   ├── IUserRepository.cs
│       │   └── IProductRepository.cs
│       ├── Repositories/
│       │   ├── UserRepository.cs
│       │   └── ProductRepository.cs
│       └── Services/
│           └── JwtService.cs
│
└── frontend/
    ├── package.json
    ├── public/
    │   └── index.html
    └── src/
        ├── App.js
        ├── index.js
        ├── index.css
        ├── api/
        │   └── api.js               # Axios instance + interceptors
        ├── context/
        │   └── AuthContext.js       # JWT auth state
        ├── components/
        │   ├── Navbar.js
        │   ├── PrivateRoute.js
        │   ├── ProductModal.js
        │   └── ConfirmDialog.js
        └── pages/
            ├── LoginPage.js
            └── ProductsPage.js
```

---

## Step 1 — Database Setup (SSMS / SQL Server)

1. Open **SQL Server Management Studio (SSMS)**.
2. Connect to your SQL Server instance.
3. Open `database/setup.sql`.
4. Press **F5** (or click Execute) to run the script.

This will:
- Create the `ProductsDB` database
- Create `Users` and `Products` tables
- Insert 10 sample products
- Add indexes

> **Note**: User passwords are BCrypt hashes. Do **not** manually insert passwords as plain text.
> Use the seed endpoint (Step 3 below) to create test users with properly hashed passwords.

---

## Step 2 — Backend Setup (.NET 8)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server running locally (or update the connection string)

### Configure the connection string

Edit `backend/ProductsApi/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductsDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

For SQL Server with username/password (not Windows auth):
```json
"DefaultConnection": "Server=localhost;Database=ProductsDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

### Change the JWT secret

In `appsettings.json`, replace the `SecretKey` value with a strong random string (at least 32 characters):

```json
"JwtSettings": {
  "SecretKey": "Replace-This-With-A-Strong-Secret-Key-Min-32-Chars!",
  ...
}
```

### Run the backend

```bash
cd backend/ProductsApi
dotnet restore
dotnet run
```

The API will start at:
- **HTTPS**: https://localhost:7000
- **HTTP**: http://localhost:5000
- **Swagger UI**: https://localhost:7000/swagger

---

## Step 3 — Seed Test Users

After the backend is running, call the seed endpoint once:

```bash
curl -X POST https://localhost:7000/api/auth/seed -k
```

Or open Swagger UI at `https://localhost:7000/swagger` and execute `POST /api/auth/seed`.

This creates:

| Username | Password | Role  |
|----------|----------|-------|
| admin    | admin123 | Admin |
| john     | user123  | User  |

> Remove or disable the `/api/auth/seed` endpoint before deploying to production.

---

## Step 4 — Frontend Setup (React)

### Prerequisites
- [Node.js 18+](https://nodejs.org/)

### Install and run

```bash
cd frontend
npm install
npm start
```

The React app will start at **http://localhost:3000** and proxy API calls to `https://localhost:7000`.

> If your backend runs on a different port, edit the `"proxy"` field in `frontend/package.json`.

---

## API Endpoints

### Auth

| Method | Endpoint             | Auth | Description               |
|--------|----------------------|------|---------------------------|
| POST   | /api/auth/login      | ❌   | Login, returns JWT token  |
| POST   | /api/auth/register   | ❌   | Register a new user       |
| POST   | /api/auth/seed       | ❌   | Create test users (dev)   |

### Products

All product endpoints require `Authorization: Bearer <token>` header.

| Method | Endpoint             | Description                   |
|--------|----------------------|-------------------------------|
| GET    | /api/products        | Get all active products       |
| GET    | /api/products/{id}   | Get product by ID             |
| POST   | /api/products        | Create a new product          |
| PUT    | /api/products/{id}   | Update an existing product    |
| DELETE | /api/products/{id}   | Soft-delete a product         |

---

## Architecture Notes

### Repository Pattern

```
Controller → IRepository (interface) → Repository (ADO.NET) → SQL Server
```

All database access uses:
- `SqlConnection` — opens a connection
- `SqlCommand` — parameterized queries (prevents SQL injection)
- `SqlDataReader` — reads results row by row

No Entity Framework, Dapper, or any ORM is used.

### Soft Delete

Products are never physically deleted. `DELETE /api/products/{id}` sets `IsActive = 0`. All queries filter by `IsActive = 1`.

### JWT Flow

1. Client `POST /api/auth/login` → receives `{ token, username, role, expires }`
2. Token stored in `localStorage`
3. Axios interceptor attaches `Authorization: Bearer <token>` to every request
4. On 401 response, interceptor clears storage and redirects to `/login`
5. Backend validates token signature, issuer, audience, and expiry on every protected route

---

## Troubleshooting

**CORS error in browser**
- Ensure the backend is running before the frontend
- Check that `http://localhost:3000` is in the CORS policy in `Program.cs`

**SSL certificate error (HTTPS)**
- Run `dotnet dev-certs https --trust` once to trust the dev certificate

**Cannot connect to SQL Server**
- Verify SQL Server is running: `services.msc` → SQL Server (MSSQLSERVER)
- Check your connection string matches your server name and auth method

**BCrypt error on login**
- Run the `/api/auth/seed` endpoint — the SQL script inserts placeholder hashes
