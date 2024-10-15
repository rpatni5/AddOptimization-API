
CREATE TABLE [dbo].[SharedFolders](
	[Id] [uniqueidentifier] NOT NULL,
	[FolderId] [uniqueidentifier] NOT NULL,
	[SharedWithId] [varchar](100) NOT NULL,
	[SharedWithType] [varchar](100) NULL,
	[PermissionLevel] [varchar](100) NOT NULL,
	[SharedByUserId] [int] NOT NULL,
	[SharedDate] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SharedFolders]  WITH CHECK ADD FOREIGN KEY([FolderId])
REFERENCES [dbo].[TemplateFolders] ([Id])
GO


