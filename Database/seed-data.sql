/* NOTE:
This does not insert any users into the database. It does create default roles
and permissions. You can use the AuthDemo API to create users and assign roles/permissions.
*/

USE AuthDemo;
GO

------------------------------------------------------------
-- ROLES
------------------------------------------------------------
INSERT INTO dbo.Roles (Id, Name, Description)
VALUES 
    ('00000000-0000-0000-0000-000000000001', 'user', 'Standard user role'),
    ('00000000-0000-0000-0000-000000000002', 'auditor', 'Audit-only role'),
    ('00000000-0000-0000-0000-000000000003', 'manager', 'Manager role'),
    ('00000000-0000-0000-0000-000000000004', 'superadmin', 'System administrator role');
GO

------------------------------------------------------------
-- PERMISSIONS
------------------------------------------------------------
INSERT INTO dbo.Permissions (Id, Name, Description)
VALUES
    ('10000000-0000-0000-0000-000000000001', 'read:users', 'Read user list'),
    ('10000000-0000-0000-0000-000000000002', 'write:users', 'Create or modify users'),
    ('10000000-0000-0000-0000-000000000003', 'read:roles', 'Read roles'),
    ('10000000-0000-0000-0000-000000000004', 'write:roles', 'Create or modify roles'),
    ('10000000-0000-0000-0000-000000000005', 'read:permissions', 'Read permissions'),
    ('10000000-0000-0000-0000-000000000006', 'audit:logs', 'Read audit logs');
GO

------------------------------------------------------------
-- ROLE → PERMISSION MAPPINGS
------------------------------------------------------------
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
VALUES
    -- user role
    ('00000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001'),

    -- auditor role
    ('00000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000006'),

    -- manager role
    ('00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001'),
    ('00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000002'),
    ('00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003'),
    ('00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000004');
GO

------------------------------------------------------------
-- Create default permissions required to manage permissions and roles for superadmin
------------------------------------------------------------
INSERT INTO dbo.Permissions (Id, Name, Description)
VALUES 
  (NEWID(), 'read:permissions', 'Allows viewing all permissions'),
  (NEWID(), 'create:permissions', 'Allows creating new permissions'),
  (NEWID(), 'update:permissions', 'Allows editing existing permissions'),
  (NEWID(), 'delete:permissions', 'Allows deleting permissions');
  (NEWID(), 'create:roles', 'Allows creating roles'),
  (NEWID(), 'update:roles', 'Allows editing roles'),
  (NEWID(), 'delete:roles', 'Allows deleting roles');
