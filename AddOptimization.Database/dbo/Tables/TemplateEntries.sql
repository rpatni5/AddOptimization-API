
CREATE TABLE [dbo].[TemplateEntries](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[TemplateId] [uniqueidentifier] NOT NULL,
	[FolderId] [uniqueidentifier] NULL,
	[EntryData] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
	[Title] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TemplateEntries] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[TemplateEntries]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[TemplateEntries]  WITH CHECK ADD FOREIGN KEY([FolderId])
REFERENCES [dbo].[TemplateFolders] ([Id])
GO

ALTER TABLE [dbo].[TemplateEntries]  WITH CHECK ADD FOREIGN KEY([TemplateId])
REFERENCES [dbo].[Templates] ([Id])
GO

ALTER TABLE [dbo].[TemplateEntries]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[TemplateEntries]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


