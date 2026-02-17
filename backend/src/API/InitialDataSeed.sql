-- ============================================================================
-- LibraryApp - Initial Data Seed
-- ============================================================================
-- This script creates initial admin user with all permissions for testing
-- Password: Admin@123!
-- ============================================================================

SET NOCOUNT ON;

DECLARE @AdminUserId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440001';
DECLARE @AdminRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440011';
DECLARE @EmployeeRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440012';
DECLARE @CustomerRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440013';

-- Insert Roles (Admin, Employee, Customer)
-- Only insert if roles don't already exist
IF NOT EXISTS (SELECT 1 FROM [auth].[Roles] WHERE Id = @AdminRoleId)
BEGIN
    INSERT INTO [auth].[Roles] (Id, Name, Description, Version, CreatedAt, UpdatedAt, IsActive)
    VALUES 
        (@AdminRoleId, 'Administrator', 'Full system access with all permissions', 1, GETUTCDATE(), GETUTCDATE(), 1);
    PRINT 'Administrator role created';
END
ELSE
BEGIN
    PRINT 'Administrator role already exists';
END

IF NOT EXISTS (SELECT 1 FROM [auth].[Roles] WHERE Id = @EmployeeRoleId)
BEGIN
    INSERT INTO [auth].[Roles] (Id, Name, Description, Version, CreatedAt, UpdatedAt, IsActive)
    VALUES 
        (@EmployeeRoleId, 'Employee', 'Employee with limited access to business operations', 1, GETUTCDATE(), GETUTCDATE(), 1);
    PRINT 'Employee role created';
END
ELSE
BEGIN
    PRINT 'Employee role already exists';
END

IF NOT EXISTS (SELECT 1 FROM [auth].[Roles] WHERE Id = @CustomerRoleId)
BEGIN
    INSERT INTO [auth].[Roles] (Id, Name, Description, Version, CreatedAt, UpdatedAt, IsActive)
    VALUES 
        (@CustomerRoleId, 'Customer', 'Customer with read-only access to their data', 1, GETUTCDATE(), GETUTCDATE(), 1);
    PRINT 'Customer role created';
END
ELSE
BEGIN
    PRINT 'Customer role already exists';
END

-- Insert Admin User
-- Password: Admin@123! 
-- BCrypt hash (cost 11): $2b$11$pzIFKtEjUmX/IOWJ4lWrdeUQ4kpXVwr.JP9g08VRCY1sQHT5QvDPy
-- Only insert if admin user doesn't already exist
IF NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE Id = @AdminUserId)
BEGIN
    INSERT INTO [auth].[Users] 
    (Id, FirstName, LastName, Username, Email, PasswordHash, PhoneNumber, 
     AddressStreet, AddressCity, AddressState, AddressZipCode, AddressCountry,
     UserType, Version, CreatedAt, UpdatedAt, IsActive)
    VALUES 
    (@AdminUserId, 'Admin', 'User', 'admin', 'admin@libraryapp.local', 
     '$2b$11$pzIFKtEjUmX/IOWJ4lWrdeUQ4kpXVwr.JP9g08VRCY1sQHT5QvDPy', 
     '+55 11 98765-4321',
     'Rua Admin, 123', 'São Paulo', 'SP', '01234-567', 'Brasil',
     2, 1, GETUTCDATE(), GETUTCDATE(), 1);
    PRINT 'Admin user created: username=admin, password=Admin@123!';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- Assign Admin Role to Admin User
-- Only assign if the relationship doesn't already exist
IF NOT EXISTS (SELECT 1 FROM [auth].[UserRoles] WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO [auth].[UserRoles] (Id, UserId, RoleId, AssignedAt)
    VALUES 
    (NEWID(), @AdminUserId, @AdminRoleId, GETUTCDATE());
    PRINT 'Administrator role assigned to admin user';
END
ELSE
BEGIN
    PRINT 'Administrator role already assigned to admin user';
END

-- ============================================================================
-- IMPORTANT NOTES:
-- ============================================================================
-- 1. Password for admin user: Admin@123!
-- 2. To login: username=admin, password=Admin@123!
-- 3. All roles need to have permissions assigned via the API endpoints
--    (they currently have no permissions - permissions will be added through the CreateRole endpoint)
-- 4. This data is for local development only
-- ============================================================================

PRINT '';
PRINT '============================================================';
PRINT 'Initial data seed completed successfully!';
PRINT '============================================================';
PRINT 'Admin credentials: username=admin, password=Admin@123!';
PRINT 'Note: Roles can be assigned permissions via the API endpoints';
PRINT '============================================================';
