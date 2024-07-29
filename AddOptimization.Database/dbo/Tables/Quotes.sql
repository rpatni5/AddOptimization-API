CREATE TABLE [dbo].[Quotes](
	[Id] [bigint] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[CustomerAddress] [varchar](400) NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[QuoteDate] [datetime2](7) NOT NULL,
	[QuoteStatusId] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[QuoteNo] [bigint] NOT NULL,
	[CompanyAddress] [varchar](400) NULL,
	[CompanyBankAddress] [varchar](400) NULL,
	[TotalPriceExcVat] [decimal](10, 2) NULL,
	[TotalPriceIncVat] [decimal](10, 2) NULL,
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

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([QuoteStatusId])
REFERENCES [dbo].[QuoteStatuses] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([QuoteStatusId])
REFERENCES [dbo].[QuoteStatuses] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Quotes]  WITH NOCHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO