CREATE TABLE [dbo].[SchedulerEventDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[SchedulerEventId] [uniqueidentifier] NOT NULL,
	[Duration] [decimal](5, 2) NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[EventTypeId] [uniqueidentifier] NOT NULL,
	[Summary] [varchar](300) NULL,
	[UserId] [int] NOT NULL,
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

ALTER TABLE [dbo].[SchedulerEventDetails] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[SchedulerEventDetails] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[SchedulerEventDetails]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEventDetails]  WITH CHECK ADD FOREIGN KEY([EventTypeId])
REFERENCES [dbo].[SchedulerEventTypes] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEventDetails]  WITH CHECK ADD FOREIGN KEY([SchedulerEventId])
REFERENCES [dbo].[SchedulerEvents] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEventDetails]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SchedulerEventDetails]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

