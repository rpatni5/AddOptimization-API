CREATE TABLE [dbo].[SchedulerStatuses]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NULL,
	CONSTRAINT [PK_SchedulerStatuses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SchedulerStatuses_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_SchedulerStatuses_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
)
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerStatuses_UpdatedByUserId]
    ON [dbo].[SchedulerStatuses]([UpdatedByUserId] ASC); 
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerStatuses_CreatedByUserId]
    ON [dbo].[SchedulerStatuses]([CreatedByUserId] ASC);
