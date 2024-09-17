CREATE TABLE [dbo].[SavedSearches](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[SearchData] [nvarchar](max) NOT NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedByUserId] [int] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[IsDefault] [bit] NULL,
	[SearchScreen] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SavedSearches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SavedSearches] ADD  CONSTRAINT [DF_SavedSearches_IsDefault]  DEFAULT ((0)) FOR [IsDefault]
GO

ALTER TABLE [dbo].[SavedSearches]  WITH CHECK ADD  CONSTRAINT [FK_SavedSearches_ApplicationUsers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SavedSearches] CHECK CONSTRAINT [FK_SavedSearches_ApplicationUsers_CreatedByUserId]
GO

ALTER TABLE [dbo].[SavedSearches]  WITH CHECK ADD  CONSTRAINT [FK_SavedSearches_ApplicationUsers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[SavedSearches] CHECK CONSTRAINT [FK_SavedSearches_ApplicationUsers_UpdatedByUserId]
GO


