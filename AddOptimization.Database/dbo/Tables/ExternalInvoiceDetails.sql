

CREATE TABLE [dbo].[ExternalInvoiceDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[ExternalInvoiceId] [bigint] NOT NULL,
	[Description] [varchar](500) NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[VatPercent] [decimal](10, 2) NOT NULL,
	[TotalPriceIncludingVat] [decimal](10, 2) NOT NULL,
	[TotalPriceExcludingVat] [decimal](10, 2) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[Metadata] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoiceDetails_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails] CHECK CONSTRAINT [FK_ExternalInvoiceDetails_CreatedByUser]
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoiceDetails_ExternalInvoices] FOREIGN KEY([ExternalInvoiceId])
REFERENCES [dbo].[ExternalInvoices] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails] CHECK CONSTRAINT [FK_ExternalInvoiceDetails_ExternalInvoices]
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoiceDetails_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceDetails] CHECK CONSTRAINT [FK_ExternalInvoiceDetails_UpdatedByUser]
GO


