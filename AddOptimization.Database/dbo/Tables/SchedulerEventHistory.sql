﻿	CREATE TABLE dbo.SchedulerEventHistory(
		Id uniqueidentifier PRIMARY KEY NOT NULL,
		SchedulerEventId uniqueidentifier NOT NULL,
		FOREIGN KEY (SchedulerEventId) REFERENCES SchedulerEvents(Id),
		UserStatusId uniqueidentifier NOT NULL,
		FOREIGN KEY (UserStatusId) REFERENCES SchedulerStatuses(Id),
		AdminStatusId uniqueidentifier NOT NULL,
		FOREIGN KEY (AdminStatusId) REFERENCES SchedulerStatuses(Id),
		Comment varchar(300)  NULL,
		UserId int NOT NULL,
		FOREIGN KEY (UserId) REFERENCES ApplicationUsers(Id),
		IsDeleted bit NOT NULL  DEFAULT 0,
		IsActive bit  NOT NULL  DEFAULT 1,
		CreatedAt datetime2(7) NULL,
		CreatedByUserId int NULL,
		FOREIGN KEY (CreatedByUserId) REFERENCES ApplicationUsers(Id),
		UpdatedAt datetime2(7) NULL,
		UpdatedByUserId int NULL,
		FOREIGN KEY (UpdatedByUserId) REFERENCES ApplicationUsers(Id),
		)
