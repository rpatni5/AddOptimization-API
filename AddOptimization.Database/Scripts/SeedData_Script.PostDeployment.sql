
/* Adding Super User */

IF NOT EXISTS (SELECT 1 FROM ApplicationUsers Where Email= 'stein@addoptimization.com')
BEGIN 
    INSERT INTO ApplicationUsers(FirstName, LastName, Email, FullName,UserName,Password,IsActive,IsLocked,IsEmailsEnabled,CreatedAt)
    VALUES
        ('Stein','','stein@addoptimization.com','','','ADAot9CKJUhGJu0rG1je4A6BBiyURsKiQPCA2NvDYKhi61C998K5B8mdLMqK3F0DnA==',1,0,1,GETUTCDATE())
END

/* Adding Active status data in Customer Status */

IF NOT EXISTS (SELECT 1 FROM CustomerStatuses Where Name= 'Active')
BEGIN 
    INSERT INTO CustomerStatuses(Id,Name)
    VALUES
        ('17756728-9DE6-409F-9D23-B8B5BA253F0E','Active')
END

/* Adding Inactive status data in Customer Status */

IF NOT EXISTS (SELECT 1 FROM CustomerStatuses Where Name= 'Inactive')
BEGIN 
    INSERT INTO CustomerStatuses(Id,Name)
    VALUES
        ('4F71A57F-2135-4B17-AA60-BA40913FE69A','Inactive')
END

/* Adding Roles data in Roles */

IF NOT EXISTS (SELECT 1 FROM Roles Where Name= 'Super Admin')
BEGIN 
    INSERT INTO Roles(Id,IsDeleted,Name,CreatedAt)
    VALUES
       (NEWID(),0,'Super Admin',GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Roles Where Name= 'Customer')
BEGIN 
    INSERT INTO Roles(Id,IsDeleted,Name,CreatedAt)
    VALUES
       (NEWID(),0,'Customer',GETUTCDATE())
END

/* Adding Fields data in Fields */

IF NOT EXISTS (SELECT 1 FROM Fields Where FieldKey= 'delete')
BEGIN 
    INSERT INTO Fields(Id,Name,FieldKey)
    VALUES
       (NEWID(),'Delete/Deactivate','delete')
END

IF NOT EXISTS (SELECT 1 FROM Fields Where FieldKey= 'create')
BEGIN 
    INSERT INTO Fields(Id,Name,FieldKey)
    VALUES
       (NEWID(),'Add New','create')
END

IF NOT EXISTS (SELECT 1 FROM Fields Where FieldKey= 'read')
BEGIN 
    INSERT INTO Fields(Id,Name,FieldKey)
    VALUES
       (NEWID(),'View','read')
END

IF NOT EXISTS (SELECT 1 FROM Fields Where FieldKey= 'update')
BEGIN 
    INSERT INTO Fields(Id,Name,FieldKey)
    VALUES
       (NEWID(),'Update Existing','update')
END


/* Adding screen data in screens */

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'screens')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Screen','screens','/admin/screens')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'roles')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Roles','roles','/admin/roles')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'Permissions')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Permissions','permissions','/admin/permissions')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'users')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Users','users','/admin/users')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'customers')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Customers','customers','/admin/customers')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'view-license')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'View license','view-license','/admin/customers/license/{}')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'all_licenses')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'All Licenses','all_licenses','/admin/licenses')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'license-details')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Customer device details','license-details','/admin/customers/licensedetails/{}')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'add')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Add licence','add','/admin/licenses')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'my_timesheets')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'My timesheets','my_timesheets','/admin/timesheets/my-timesheets')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'timesheet_management')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Timesheet management','timesheet_management','/admin/timesheets/timesheets-management')
END

/* Adding data in role permissions */

GO
DECLARE @RoleId uniqueidentifier;
DECLARE @ScreenId uniqueidentifier;
DECLARE @FieldId uniqueidentifier;

SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Customer');
SET @ScreenId = (Select TOP 1 Id from Screens Where ScreenKey = 'license-details');
SET @FieldId = (Select TOP 1 Id from Fields Where FieldKey = 'update');

IF NOT EXISTS (SELECT 1 FROM RolePermissions Where RoleId = @RoleId AND ScreenId =@ScreenId AND FieldId = @FieldId)
BEGIN
    Insert into RolePermissions(Id,RoleId,ScreenId,FieldId,CreatedAt) 
    values (NEWID(),@RoleId,@ScreenId,@FieldId,GETUTCDATE())
END
GO

GO
DECLARE @RoleId uniqueidentifier;
DECLARE @ScreenId uniqueidentifier;
DECLARE @FieldId uniqueidentifier;

SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Customer');
SET @ScreenId = (Select TOP 1 Id from Screens Where ScreenKey = 'license-details');
SET @FieldId = (Select TOP 1 Id from Fields Where FieldKey = 'delete');
IF NOT EXISTS (SELECT 1 FROM RolePermissions Where RoleId = @RoleId AND ScreenId =@ScreenId AND FieldId = @FieldId)
BEGIN
    Insert into RolePermissions(Id,RoleId,ScreenId,FieldId,CreatedAt) 
    values (NEWID(),@RoleId,@ScreenId,@FieldId,GETUTCDATE())
END
GO

GO
DECLARE @RoleId uniqueidentifier;
DECLARE @ScreenId uniqueidentifier;
DECLARE @FieldId uniqueidentifier;

SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Customer');
SET @ScreenId = (Select TOP 1 Id from Screens Where ScreenKey = 'license-details');
SET @FieldId = (Select TOP 1 Id from Fields Where FieldKey = 'read');

IF NOT EXISTS (SELECT 1 FROM RolePermissions Where RoleId = @RoleId AND ScreenId =@ScreenId AND FieldId = @FieldId)
BEGIN
    Insert into RolePermissions(Id,RoleId,ScreenId,FieldId,CreatedAt) 
    values (NEWID(),@RoleId,@ScreenId,@FieldId,GETUTCDATE())
END
GO

GO
DECLARE @RoleId uniqueidentifier;
DECLARE @ScreenId uniqueidentifier;
DECLARE @FieldId uniqueidentifier;

SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Customer');
SET @ScreenId = (Select TOP 1 Id from Screens Where ScreenKey = 'license-details');
SET @FieldId = (Select TOP 1 Id from Fields Where FieldKey = 'create');

IF NOT EXISTS (SELECT 1 FROM RolePermissions Where RoleId = @RoleId AND ScreenId =@ScreenId AND FieldId = @FieldId)
BEGIN
    Insert into RolePermissions(Id,RoleId,ScreenId,FieldId,CreatedAt) 
    values (NEWID(),@RoleId,@ScreenId,@FieldId,GETUTCDATE())
END
GO

GO
DECLARE @RoleId uniqueidentifier;
DECLARE @ScreenId uniqueidentifier;
DECLARE @FieldId uniqueidentifier;

SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Customer');
SET @ScreenId = (Select TOP 1 Id from Screens Where ScreenKey = 'all_licenses');
SET @FieldId = (Select TOP 1 Id from Fields Where FieldKey = 'read');
IF NOT EXISTS (SELECT 1 FROM RolePermissions Where RoleId = @RoleId AND ScreenId =@ScreenId AND FieldId = @FieldId)
BEGIN
    Insert into RolePermissions(Id,RoleId,ScreenId,FieldId,CreatedAt) 
    values (NEWID(),@RoleId,@ScreenId,@FieldId,GETUTCDATE())
END
GO


/*Assign super admin role to default user*/
GO
DECLARE @UserId int;
DECLARE @RoleId uniqueidentifier;
SET @UserId = (Select TOP 1 Id from ApplicationUsers Where Email = 'stein@addoptimization.com');
SET @RoleId = (Select TOP 1 Id from Roles Where Name = 'Super Admin');
Select @UserId
Select @RoleId
IF NOT EXISTS (SELECT 1 FROM UserRoles Where RoleId = @RoleId AND UserId =@UserId)
BEGIN
    Insert into UserRoles(Id,UserId,RoleId,CreatedAt)
    values (NEWID(),@UserId,@RoleId,GETUTCDATE())
END
GO