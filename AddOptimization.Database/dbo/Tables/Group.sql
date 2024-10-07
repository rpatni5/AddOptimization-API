
CREATE TABLE [dbo].[Groups](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](100) NOT NULL,
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

ALTER TABLE [dbo].[Groups] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Groups]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Groups]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


