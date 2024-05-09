CREATE TABLE [dbo].[SchedulerEventTypes]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NULL,
	CONSTRAINT [PK_SchedulerEventTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SchedulerEventTypes_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_SchedulerEventTypes_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
)

GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEventTypes_UpdatedByUserId]
    ON [dbo].[SchedulerEventTypes]([UpdatedByUserId] ASC); 
GO
CREATE NONCLUSTERED INDEX [IX_SchedulerEventTypes_CreatedByUserId]
    ON [dbo].[SchedulerEventTypes]([CreatedByUserId] ASC);