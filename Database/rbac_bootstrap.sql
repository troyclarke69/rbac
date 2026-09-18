------------------------------------------------------------
-- RBAC SECURITY SCHEMA (Auth0-style)
-- This schema provides:
--   - Users
--   - Roles
--   - Permissions
--   - UserRoles (many-to-many)
--   - RolePermissions (many-to-many)
--   - UserMetadata (JSON)
--   - OAuthClients (M2M / SPA apps)
--   - RefreshTokens (long-lived sessions)
--
-- This is the same conceptual model used by Auth0, Okta,
-- Azure AD App Roles, and Keycloak.
------------------------------------------------------------

USE master;
GO

IF DB_ID('AuthDemo') IS NOT NULL
BEGIN
    ALTER DATABASE AuthDemo SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AuthDemo;
END
GO

CREATE DATABASE AuthDemo;
GO

USE AuthDemo;
GO

------------------------------------------------------------
-- USERS
------------------------------------------------------------
CREATE TABLE dbo.Users (
    Id              UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Email           NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash    VARBINARY(512) NOT NULL,
    PasswordSalt    VARBINARY(256) NOT NULL,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3) NULL
);
GO

------------------------------------------------------------
-- ROLES
------------------------------------------------------------
CREATE TABLE dbo.Roles (
    Id          UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);
GO

------------------------------------------------------------
-- PERMISSIONS
------------------------------------------------------------
CREATE TABLE dbo.Permissions (
    Id          UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Name        NVARCHAR(200) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);
GO

------------------------------------------------------------
-- ROLE PERMISSIONS (many-to-many)
------------------------------------------------------------
CREATE TABLE dbo.RolePermissions (
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),

    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(Id),

    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId)
        REFERENCES dbo.Permissions(Id)
);
GO

------------------------------------------------------------
-- USER ROLES (many-to-many)
------------------------------------------------------------
CREATE TABLE dbo.UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),

    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id),

    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(Id)
);
GO

------------------------------------------------------------
-- USER METADATA (JSON)
------------------------------------------------------------
CREATE TABLE dbo.UserMetadata (
    UserId   UNIQUEIDENTIFIER PRIMARY KEY,
    JsonData NVARCHAR(MAX) NULL,

    CONSTRAINT FK_UserMetadata_User FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id)
);
GO

------------------------------------------------------------
-- OAUTH CLIENTS (M2M / SPA / Native apps)
------------------------------------------------------------
CREATE TABLE dbo.OAuthClients (
    Id                UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ClientId          NVARCHAR(200) NOT NULL UNIQUE,
    ClientSecretHash  VARBINARY(512) NULL,
    AllowedScopes     NVARCHAR(MAX) NULL,
    RedirectUris      NVARCHAR(MAX) NULL,
    CreatedAt         DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

------------------------------------------------------------
-- REFRESH TOKENS
------------------------------------------------------------
CREATE TABLE dbo.RefreshTokens (
    Id           UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    TokenHash    VARBINARY(512) NOT NULL,
    ExpiresAt    DATETIME2(3) NOT NULL,
    RevokedAt    DATETIME2(3) NULL,
    CreatedAt    DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id)
);
GO

------------------------------------------------------------
-- INDEXES
------------------------------------------------------------
CREATE NONCLUSTERED INDEX IX_Users_Email
ON dbo.Users (Email);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_UserId
ON dbo.UserRoles (UserId);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_RoleId
ON dbo.UserRoles (RoleId);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId
ON dbo.RolePermissions (RoleId);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_PermissionId
ON dbo.RolePermissions (PermissionId);
GO

PRINT 'Auth RBAC bootstrap completed successfully.';
GO
