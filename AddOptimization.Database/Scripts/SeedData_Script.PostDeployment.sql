
/* Adding Super User */

IF NOT EXISTS (SELECT 1 FROM ApplicationUsers Where Email= 'stein@addoptimization.com')
BEGIN 
    INSERT INTO ApplicationUsers(FirstName, LastName, Email, FullName,UserName,Password,IsActive,IsLocked,IsEmailsEnabled,CreatedAt)
    VALUES
        ('Stein','Geerinck','stein@addoptimization.com','Stein Geerinck','stein@addoptimization.com','ADAot9CKJUhGJu0rG1je4A6BBiyURsKiQPCA2NvDYKhi61C998K5B8mdLMqK3F0DnA==',1,0,0,GETUTCDATE())
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

/* Adding Timesheet EventType data in Scheduler Event Types */

IF NOT EXISTS (SELECT 1 FROM SchedulerEventTypes Where Name= 'Timesheet')
BEGIN 
    INSERT INTO SchedulerEventTypes(Id,Name)
    VALUES
        ('C20BA494-469D-4A1F-864F-472DA6351084','Timesheet')
END

/* Adding Overlap EventType data in Scheduler Event Types */

IF NOT EXISTS (SELECT 1 FROM SchedulerEventTypes Where Name= 'Overtime')
BEGIN 
    INSERT INTO SchedulerEventTypes(Id,Name)
    VALUES
        ('D4E69B78-10BF-4B9B-8072-8CEF20ECAAA3','Overtime')
END

/* Adding Absence Request EventType data in Scheduler Event Types */

IF NOT EXISTS (SELECT 1 FROM SchedulerEventTypes Where Name= 'Absence Request')
BEGIN 
    INSERT INTO SchedulerEventTypes(Id,Name)
    VALUES
        ('45F8C755-E930-4372-96C7-8DE02FDDFDF6','Absence Request')
END

/* Adding Pending for accountant approval Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Pending Account Approval')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('B39271A8-EB3A-42DC-A079-1C76F4A75668','Pending Account Approval','PENDING_ACCOUNT_ADMIN_APPROVAL')
END

/* Adding Draft Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Draft')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('905729F2-E9A6-4640-BC04-37A189D77628','Draft','DRAFT')
END

/* Adding Pending for invoicing Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Pending Invoicing')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('0AAD8336-A223-42F4-B6FE-4EDD97ADEDC3','Pending Invoicing','PENDING_INVOICING')
END

/* Adding Paid by customer Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Paid')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('F69D2DD9-0165-4CAA-BD6D-96FF663309AF','Paid','CUSTOMER_PAID')
END

/* Adding Pending for customer appproval Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Pending Customer Appproval')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('2DB8D005-90BB-40ED-B35E-ACC37B15787E','Pending Customer Appproval','PENDING_CUSTOMER_APPROVAL')
END



/* Adding Declined  Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Declined')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('7FCB2F89-750E-4B76-9B5F-6D944EB58AA2','Declined','DECLINED')
END

/* Adding Customer Declined Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Customer Declined')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('BA7AB70E-C985-4122-BDCA-F83FC29A9884','Customer Declined','CUSTOMER_DECLINED')
END

/* Adding Customer Approved  Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Customer Approved')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('92D6B0DB-F21B-4314-96E6-1C45E8E375C6','Customer Approved','CUSTOMER_APPROVED')
END

/* Adding Admin Approved  Scheduler Status data in Scheduler Status */

IF NOT EXISTS (SELECT 1 FROM SchedulerStatuses Where Name= 'Approved')
BEGIN 
    INSERT INTO SchedulerStatuses(Id,Name,StatusKey)
    VALUES
        ('CA404379-C54D-41B8-AE40-4609FFFDBC5F','Approved','ADMIN_APPROVED')
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

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'view_timesheet')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'View timesheet','view_timesheet','/admin/timesheets/view-timesheet/{}')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'view_quote')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'View quote','view_quote','/admin/quotes/view-quote/{}')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'add_quote')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Add Quote','add_quote','/admin/quote/quote-add/{}')
END

IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey= 'edit_quote')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
       (NEWID(),'Edit Quote','edit_quote','/admin/quotes/quotes-edit/{}')
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

--Start entry for leave statuses
IF NOT EXISTS (SELECT 1 FROM LeaveStatuses Where Name= 'Requested')
BEGIN 
    INSERT INTO LeaveStatuses(Id,Name,CreatedAt,CreatedByUserId,UpdatedAt,UpdatedByUserId,IsDeleted,IsActive)
    VALUES
        ('1','Requested',GETDATE(),1,null,null,0,1);
END
IF NOT EXISTS (SELECT 2 FROM LeaveStatuses Where Name= 'Approved')
BEGIN 
    INSERT INTO LeaveStatuses(Id,Name,CreatedAt,CreatedByUserId,UpdatedAt,UpdatedByUserId,IsDeleted,IsActive)
    VALUES
        ('2','Approved',GETDATE(),1,null,null,0,1);
END
IF NOT EXISTS (SELECT 3 FROM LeaveStatuses Where Name= 'Rejected')
BEGIN 
    INSERT INTO LeaveStatuses(Id,Name,CreatedAt,CreatedByUserId,UpdatedAt,UpdatedByUserId,IsDeleted,IsActive)
    VALUES
        ('3','Rejected',GETDATE(),1,null,null,0,1);
END
--End entry for leave statuses

--Start entry for screens
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='gui_versions')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('59B6217B-3408-47D8-524F-08DC629F0CF6','GUI Versions','gui_versions','/admin/gui-versions')
END
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='public-holiday')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('2A87C897-040F-448F-3E68-08DC645010CA','public-holiday','public-holiday','/admin/public-holiday')
END
 
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='admin_gui_versions')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('58A0D4D0-386B-4599-F65E-08DC6A8B7B32','Admin GUI Versions','admin_gui_versions','/admin/gui-versions')
END
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='user_time_sheet_calendar')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('795A1FAF-EDCE-4B8F-F7F2-08DC6DAD177C','User time sheet calendar','user_time_sheet_calendar','/admin/timesheets/time-sheets-calendar/{}')
END
 
 
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='absence_request')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('F2E60144-4F95-455E-FD2B-08DC6F1A7E9F','Absence Request','absence_request','/admin/timesheets/absence-request')
END
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='admin_time_sheet_review_calendar')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('7817CDA7-F6CB-4A02-4F5E-08DC6F38BC18','Admin time sheet review calendar','admin_time_sheet_review_calendar','/admin/timesheets/time-sheets-review-calendar/{}')
END
 
 
 
IF NOT EXISTS (SELECT 1 FROM Screens Where ScreenKey ='absence_approval')
BEGIN 
    INSERT INTO Screens(Id,Name,ScreenKey,Route)
    VALUES
        ('B39271A8-EB3A-42DC-A079-1C76F4A75668','Absence Approval','absence_approval','/admin/timesheets/absence-approval')
END
--End entry for screens


--adding entry for  roles 
IF NOT EXISTS (SELECT 1 FROM Roles Where Name= 'External Employee')
BEGIN 
    INSERT INTO Roles(Id,IsDeleted,Name)
    VALUES
       (NEWID(),0,'External Employee')
END

-- Invoicing seed data starts

/* Adding UNPAID status data in Payment Status */

IF NOT EXISTS (SELECT 1 FROM PaymentStatuses Where StatusKey= 'UNPAID')
BEGIN 
    INSERT INTO PaymentStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Unpaid','UNPAID')
END

/* Adding PARTIAL_PAID status data in Payment Status */

IF NOT EXISTS (SELECT 1 FROM PaymentStatuses Where StatusKey= 'PARTIAL_PAID')
BEGIN 
    INSERT INTO PaymentStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'PartialPaid','PARTIAL_PAID')
END

/* Adding PAID status data in Payment Status */

IF NOT EXISTS (SELECT 1 FROM PaymentStatuses Where StatusKey= 'PAID')
BEGIN 
    INSERT INTO PaymentStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Paid','PAID')
END

/* Adding DRAFT status data in Invoice Status */

IF NOT EXISTS (SELECT 1 FROM InvoiceStatuses Where StatusKey= 'DRAFT')
BEGIN 
    INSERT INTO InvoiceStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Draft','DRAFT')
END

/* Adding SEND_TO_CUSTOMER status data in Invoice Status */

IF NOT EXISTS (SELECT 1 FROM InvoiceStatuses Where StatusKey= 'SEND_TO_CUSTOMER')
BEGIN 
    INSERT INTO InvoiceStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Send To Customer','SEND_TO_CUSTOMER')
END

/* Adding CLOSED status data in Invoice Status */

IF NOT EXISTS (SELECT 1 FROM InvoiceStatuses Where StatusKey= 'CLOSED')
BEGIN 
    INSERT INTO InvoiceStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Closed','CLOSED')
END


/* Adding DECLINED status data in Invoice Status */

IF NOT EXISTS (SELECT 1 FROM InvoiceStatuses Where StatusKey= 'DECLINED')
BEGIN 
    INSERT INTO InvoiceStatuses(Id,Name,StatusKey)
    VALUES
        (NEWID(),'Declined','DECLINED')
END
-- Invoicing seed data ends


/* Adding Draft Quote Status data in Quote Status */

IF NOT EXISTS (SELECT 1 FROM QuoteStatuses Where StatusKey= 'DRAFT')
BEGIN 
    INSERT INTO QuoteStatuses(Id,Name,StatusKey)
    VALUES
        ('6BC0B3F8-74FA-4B59-8A95-4DDB851C2A15','Draft','DRAFT')
END

/* Adding Cancel Quote Status data in Quote Status */

IF NOT EXISTS (SELECT 1 FROM QuoteStatuses Where StatusKey= 'CANCEL')
BEGIN 
    INSERT INTO QuoteStatuses(Id,Name,StatusKey)
    VALUES
        ('B4D1B75D-96B4-4B2B-923B-49564866E3E1','Cancel','CANCEL')
END

/* Adding Finalized Quote Status data in Quote Status */

IF NOT EXISTS (SELECT 1 FROM QuoteStatuses Where StatusKey= 'FINALIZED')
BEGIN 
    INSERT INTO QuoteStatuses(Id,Name,StatusKey)
    VALUES
        ('7F7EBBE2-C4E9-49D5-9AB5-689EAF6652FB','Finalized','FINALIZED')
END

/* Adding Send To Customer Quote Status data in Quote Status */

IF NOT EXISTS (SELECT 1 FROM QuoteStatuses Where StatusKey= 'SEND_TO_CUSTOMER')
BEGIN 
    INSERT INTO QuoteStatuses(Id,Name,StatusKey)
    VALUES
        ('44B14DD6-62DB-4001-A11B-8355F683D758','Send To Customer','SEND_TO_CUSTOMER')
END
