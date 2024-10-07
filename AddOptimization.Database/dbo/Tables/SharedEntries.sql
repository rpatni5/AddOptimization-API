
CREATE TABLE [dbo].[SharedEntries](
	[Id] [uniqueidentifier] NOT NULL,
	[EntryId] [uniqueidentifier] NOT NULL,
	[SharedWithId] [varchar](100) NOT NULL,
	[SharedWithType] [varchar](100) NULL,
	[PermissionLevel] [varchar](100) NOT NULL,
	[SharedByUserId] [int] NOT NULL,
	[SharedDate] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SharedEntries] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[SharedEntries]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SharedEntries]  WITH CHECK ADD FOREIGN KEY([EntryId])
REFERENCES [dbo].[TemplateEntries] ([Id])
GO

ALTER TABLE [dbo].[SharedEntries]  WITH CHECK ADD FOREIGN KEY([SharedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SharedEntries]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


