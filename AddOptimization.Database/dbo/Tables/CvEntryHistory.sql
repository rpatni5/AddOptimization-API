CREATE TABLE [dbo].[CvEntryHistory](
	[Id] [uniqueidentifier] NOT NULL,
	[CVEntryId] [uniqueidentifier] NOT NULL,
	[EntryData] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[CvEntryHistory]  WITH CHECK ADD FOREIGN KEY([CVEntryId])
REFERENCES [dbo].[CvEntries] ([Id])
GO




