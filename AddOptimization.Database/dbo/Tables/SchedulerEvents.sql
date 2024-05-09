CREATE TABLE [dbo].[SchedulerEvents]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Duration] [decimal](5, 2) NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[Summary] [varchar](300) NULL,
	[EventTypeId] [uniqueidentifier] NOT NULL,
	[StatusId] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[IsDraft] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	CONSTRAINT [PK_SchedulerEvents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SchedulerEvents_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_SchedulerEvents_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
	CONSTRAINT [FK_SchedulerEvents_ApplicationUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_SchedulerEvents_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Clients] ([Id]),
	CONSTRAINT [FK_SchedulerEvents_SchedulerEventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [dbo].[SchedulerEventTypes] ([Id]),
	CONSTRAINT [FK_SchedulerEvents_SchedulerStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[SchedulerStatuses] ([Id]),
)
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_UpdatedByUserId]
    ON [dbo].[SchedulerEvents]([UpdatedByUserId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_CreatedByUserId]
    ON [dbo].[SchedulerEvents]([CreatedByUserId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_UserId]
    ON [dbo].[SchedulerEvents]([UserId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_EventTypeId]
    ON [dbo].[SchedulerEvents]([EventTypeId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_StatusId]
    ON [dbo].[SchedulerEvents]([StatusId] ASC);
GO
	CREATE NONCLUSTERED INDEX [IX_SchedulerEvents_ClientId]
    ON [dbo].[SchedulerEvents]([ClientId] ASC);