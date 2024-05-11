CREATE TABLE [dbo].[SchedulerEvents](
	[Id] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[ApprovarId] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserStatusId] [uniqueidentifier] NOT NULL,
	[AdminStatusId] [uniqueidentifier] NOT NULL,
	[IsDraft] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SchedulerEvents] ADD  DEFAULT ((1)) FOR [IsDraft]
GO

ALTER TABLE [dbo].[SchedulerEvents] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[SchedulerEvents] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([AdminStatusId])
REFERENCES [dbo].[SchedulerStatuses] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([ApprovarId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEvents]  WITH CHECK ADD FOREIGN KEY([UserStatusId])
REFERENCES [dbo].[SchedulerStatuses] ([Id])
GO

