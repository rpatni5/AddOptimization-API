
CREATE TABLE [dbo].[Quotes](
	[CustomerId] [uniqueidentifier] NOT NULL,
	[CustomerAddress] [nvarchar](300) NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[QuoteDate] [datetime2](7) NOT NULL,
	[QuoteStatusId] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[Id] [bigint] NOT NULL,
	[QuoteNo] [bigint] NULL,
	[CompanyAddress] [varchar](400) NULL,
	[CompanyBankAddress] [varchar](400) NULL,
 CONSTRAINT [PK_Quotes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Quotes] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Quotes] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([QuoteStatusId])
REFERENCES [dbo].[QuoteStatuses] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([QuoteStatusId])
REFERENCES [dbo].[QuoteStatuses] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO