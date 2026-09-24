# **RBAC Identity Service (Auth0‑Style)**  

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)
![Language](https://img.shields.io/badge/Language-C%23-239120)
![Auth](https://img.shields.io/badge/Auth-JWT%20RS256-yellow)
![Security](https://img.shields.io/badge/Security-RBAC-critical)
![Database](https://img.shields.io/badge/SQL%20Server-2022-blue)
![Platform](https://img.shields.io/badge/Platform-Linux%20%7C%20Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![CI](https://github.com/troyclarke69/rbac/actions/workflows/ci.yml/badge.svg)
![Coverage](https://img.shields.io/badge/Coverage-80%25-brightgreen)

A standalone, production‑ready identity service implementing:

- Full RBAC (Users, Roles, Permissions)  
- JWT issuance (RS256)  
- Permission‑based authorization middleware  
- PBKDF2 password hashing  
- Dapper-based repositories  
- Clean, modern .NET API architecture  

This project is intentionally designed as a **lightweight Auth0 replacement** suitable for internal systems, demos, and future expansion into OAuth2/OIDC if desired.

---

## **📁 Project Structure**

```
/rbac
│
├── private.key
├── public.key
│
├── appsettings.json
├── appsettings.Development.json
│
├── Program.cs
│
├── /Controllers
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── RolesController.cs
│   ├── PermissionsController.cs
│
├── /Services
│   ├── AuthService.cs
│   ├── RbacQueryService.cs
│   ├── JwtIssuer.cs
│
├── /Repositories
│   ├── UserRepository.cs
│   ├── RoleRepository.cs
│   ├── PermissionRepository.cs
│   ├── RbacQueryRepository.cs
│
├── /Middleware
│   ├── RequirePermissionAttribute.cs
│   ├── PermissionAuthorizationMiddleware.cs
│
├── /Models
│   ├── User.cs
│   ├── Role.cs
│   ├── Permission.cs
│   ├── UserRole.cs
│   ├── RolePermission.cs
│   ├── UserMetadata.cs
│   ├── OAuthClient.cs
│   ├── RefreshToken.cs
│
├── /Utils
│   ├── PasswordHasher.cs
│   ├── SqlConnectionFactory.cs
│
└── /Database
    ├── auth-bootstrap.sql
    ├── seed-data.sql
```

---

## **🧱 Core Features**

### **RBAC Schema (Auth0‑Style)**  
Implements:

- Users  
- Roles  
- Permissions  
- UserRoles (M2M)  
- RolePermissions (M2M)  
- UserMetadata (JSON)  
- OAuthClients  
- RefreshTokens  

This schema mirrors Auth0, Okta, Keycloak, and Azure AD App Roles.

---

### **JWT Issuer (RS256)**  
The service issues signed JWTs containing:

- `sub` (user ID)  
- `role` claims  
- `permission` claims  
- `iss`, `aud`, `exp`, `jti`  

Tokens are signed using **private.key** and validated using **public.key**.

---

### **Permission‑Based Authorization Middleware**

```csharp
[RequirePermission("read:users")]
```

The middleware enforces:

- **401 Unauthorized** → no token  
- **403 Forbidden** → missing permissions  

---

### **PBKDF2 Password Hashing**

- 32‑byte salt  
- 32‑byte hash  
- 100,000 iterations  
- SHA‑256  

Matches modern security standards and ASP.NET Identity defaults.

---

### **Dapper Repositories**

Fast, lightweight data access for:

- Users  
- Roles  
- Permissions  
- RBAC queries  

---

## **🚀 Running the Service**

### **1. Create the database**

Run:

- `/Database/auth-bootstrap.sql`  
- `/Database/seed-data.sql`  

### **2. Generate RSA keys**

```
openssl genrsa -out private.key 2048
openssl rsa -in private.key -pubout -out public.key
```

Place both files in the project root.

### **3. Configure appsettings.json**

Set:

- Connection string  
- JWT issuer  
- JWT audience  
- Key paths  

### **4. Run the API**

```
dotnet run
```

### **5. Login**

```
POST /auth/login
{
  "email": "admin@example.com",
  "password": "your-password"
}
```

Response:

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### **6. Call protected endpoints**

```
Authorization: Bearer <token>
```

---

## **📌 Build Sequence (Permanent Reference)**

Follow this order for clean builds and future enhancements.

### **1. Models**  
C# classes mapped directly to SQL schema.

### **2. Repositories**  
Dapper-based data access.

### **3. Services**  
- AuthService  
- RbacQueryService  
- JwtIssuer  

### **4. Middleware**  
- RequirePermissionAttribute  
- PermissionAuthorizationMiddleware  

### **5. Controllers**  
Auth, Users, Roles, Permissions.

### **6. Utilities**  
PasswordHasher, SqlConnectionFactory.

### **7. Program.cs**  
DI registration, JWT validation, middleware pipeline.

### **8. appsettings.json**  
Final configuration for DB + JWT.

---

# **DB Setup**

### **Start Local SQL Server (Docker)**

```bash
docker run -e "ACCEPT_EULA=Y" \
           -e "MSSQL_SA_PASSWORD=Swed9999!" \
           -p 1433:1433 \
           --name smed-local-sql \
           -d mcr.microsoft.com/mssql/server:2022-latest
```

This provides:

- Host: `localhost`  
- Port: `1433`  
- User: `sa`  
- Password: `Swed9999!`  
- Database: empty (bootstrap next)

---

### **Connect to SQL Server**

```bash
docker run -it --rm \
  --network host \
  mcr.microsoft.com/mssql-tools \
  /opt/mssql-tools/bin/sqlcmd -S localhost,1433 -U sa -P 'Swed9999!'
```

---

### **Run Bootstrap & Seed Scripts**

```bash
sqlcmd -S localhost,1433 -U sa -P Swed9999! -i Database/rbac_bootstrap.sql
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -i Database/seed-data.sql

NOTE: This will assign additional roles/perms AFTER the superadmin user is created via API below.
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -i Database/superadmin.sql
```

---

### **Validation Commands (examples)**

```bash
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "SELECT name FROM sys.databases;"
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "SELECT name FROM sys.tables;"
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "SELECT Email, PasswordHash FROM dbo.Users;"
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "SELECT Name FROM dbo.Permissions;"
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "INSERT INTO dbo.Permissions (Id, Name, Description)
VALUES (NEWID(), 'read:permissions', 'Allows viewing the permissions list');"

sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -q "EXEC sp_help 'Users';"
```

---

# ⭐ **📁 cURL Test Suite**

Assumptions:

- API running at `http://localhost:5000`  
- SQL Server at `localhost,1433`  

Use the **superadmin token** for privileged tests.

---

# **Admin Setup for RBAC Testing**

## **1️⃣ Create the “No‑Role Admin” (403 Test User)**

```bash
curl -X POST http://localhost:5000/users/bootstrap-admin
```

Creates:

- `admin@example.com`  
- PBKDF2 password  
- **No roles**  
- **No token**

This user is expected to fail RBAC checks.

---

## **2️⃣ Login (Get JWT)**

```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
        "email": "admin@example.com",
        "password": "Swed9999!"
      }'
```

Token contains:

- `sub`, `iss`, `aud`, `nbf`, `exp`  
- **No roles**  
- **No permissions**

---

## **3️⃣ Test RBAC Denial (403 Forbidden)**

```bash
curl -X GET http://localhost:5000/users \
  -H "Authorization: Bearer <token>"
```

Expected:

```
403 Forbidden
```

Reason:

- No roles → no permissions → no access.

---

## **4️⃣ Create the Privileged Admin (superadmin)**

```bash
curl -X POST http://localhost:5000/users/bootstrap-admin-with-role
```

Creates:

- `superadmin@example.com`  
- Assigns `manager` role  
- Returns `{ id, token }`

This user can access all protected endpoints.

## Important - superadmin requires additional role/perms setup - run this:
sqlcmd -S localhost,1433 -U sa -P Swed9999! -d AuthDemo -i Database/superadmin.sql

---

# **User Endpoints**

## **Get User by ID**

```bash
curl -X GET http://localhost:5000/users/USER_GUID_HERE \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## **Create User**  
Requires: `write:users`

```bash
curl -X POST http://localhost:5000/users \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
        "email": "newuser@example.com",
        "password": "StrongPassword123!"
      }'
```

---

# **Role Endpoints**

## **Get All Roles**  
Requires: `read:roles`

```bash
curl -X GET http://localhost:5000/roles \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## **Create Role**  
Requires: `write:roles`

```bash
curl -X POST http://localhost:5000/roles \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
        "name": "auditor",
        "description": "Audit-only role"
      }'
```

---

# **Permission Endpoints**

## **Get All Permissions**  
Requires: `read:permissions`

```bash
curl -X GET http://localhost:5000/permissions \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## **Create Permission**  
Requires: `write:roles`

```bash
curl -X POST http://localhost:5000/permissions \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
        "name": "audit:logs",
        "description": "Read audit logs"
      }'
```

---
