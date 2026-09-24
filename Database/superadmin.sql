/*
NOTE: The superadmin role is not created in this script or seed-data. 
As per the README. ==> User: superadmin will be created and perms assigned (through API).
This script will assign the required permissions to the superadmin role.
*/

DECLARE @superadmin UNIQUEIDENTIFIER =
    (SELECT Id FROM dbo.Roles WHERE Name = 'superadmin');

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @superadmin, Id
FROM dbo.Permissions
WHERE Name IN (
    'read:permissions',
    'create:permissions',
    'update:permissions',
    'delete:permissions'
);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @superadmin, Id
FROM dbo.Permissions
WHERE Name IN (
    'create:roles',
    'update:roles',
    'delete:roles'
);

