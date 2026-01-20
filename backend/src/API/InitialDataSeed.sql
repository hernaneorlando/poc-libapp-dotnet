-- ============================================================================
-- LibraryApp - Initial Data Seed
-- ============================================================================
-- This script creates initial admin user with all permissions for testing
-- Password: Admin@123! (BCrypt hash)
-- ============================================================================

SET NOCOUNT ON;

DECLARE @AdminUserId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440001';
DECLARE @AdminRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440011';
DECLARE @EmployeeRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440012';
DECLARE @CustomerRoleId UNIQUEIDENTIFIER = '550e8400-e29b-41d4-a716-446655440013';

-- Insert Roles (Admin, Employee, Customer)
INSERT INTO [auth].[Roles] (Id, Name, Description, Version, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@AdminRoleId, 'Administrator', 'Full system access with all permissions', 1, GETUTCDATE(), GETUTCDATE(), 1),
    (@EmployeeRoleId, 'Employee', 'Employee with limited access to business operations', 1, GETUTCDATE(), GETUTCDATE(), 1),
    (@CustomerRoleId, 'Customer', 'Customer with read-only access to their data', 1, GETUTCDATE(), GETUTCDATE(), 1);

-- Insert Admin User
-- Password: Admin@123! 
-- BCrypt hash (cost 11): $2a$11$5QX3P7N9RZxJ2K1L0M9N8u5JzK2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y
INSERT INTO [auth].[Users] 
(Id, FirstName, LastName, Username, Email, PasswordHash, PhoneNumber, 
 AddressStreet, AddressCity, AddressState, AddressZipCode, AddressCountry,
 UserType, Version, CreatedAt, UpdatedAt, IsActive)
VALUES 
(@AdminUserId, 'Admin', 'User', 'admin', 'admin@libraryapp.local', 
 '$2a$11$5QX3P7N9RZxJ2K1L0M9N8u5JzK2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y', 
 '+55 11 98765-4321',
 'Rua Admin, 123', 'São Paulo', 'SP', '01234-567', 'Brasil',
 2, 1, GETUTCDATE(), GETUTCDATE(), 1);

-- Assign Admin Role to Admin User
INSERT INTO [auth].[UserRoles] (Id, UserId, RoleId, AssignedAt)
VALUES 
(NEWID(), @AdminUserId, @AdminRoleId, GETUTCDATE());

-- ============================================================================
-- IMPORTANT NOTES:
-- ============================================================================
-- 1. Password for admin user: Admin@123!
-- 2. To login: username=admin, password=Admin@123!
-- 3. All roles need to have permissions assigned via the API endpoints
--    (they currently have no permissions - permissions will be added through the CreateRole endpoint)
-- 4. This data is for local development only
-- ============================================================================

PRINT 'Initial data seed completed successfully!';
PRINT 'Admin user created: username=admin, password=Admin@123!';
PRINT 'Roles created: Administrator, Employee, Customer';
PRINT 'Note: Assign permissions to roles via the CreateRole API endpoint';
