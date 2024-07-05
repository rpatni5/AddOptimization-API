
CREATE TABLE [dbo].[InvoiceDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceId] [bigint] NOT NULL,
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
 CONSTRAINT [PK__InvoiceD__3214EC0751A59BBB] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceDetails] ADD  CONSTRAINT [DF__InvoiceDe__IsDel__7CD98669]  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[InvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK__InvoiceDe__Creat__7DCDAAA2] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoiceDetails] CHECK CONSTRAINT [FK__InvoiceDe__Creat__7DCDAAA2]
GO

ALTER TABLE [dbo].[InvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK__InvoiceDe__Invoi__7EC1CEDB] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoices] ([Id])
GO

ALTER TABLE [dbo].[InvoiceDetails] CHECK CONSTRAINT [FK__InvoiceDe__Invoi__7EC1CEDB]
GO

ALTER TABLE [dbo].[InvoiceDetails]  WITH CHECK ADD  CONSTRAINT [FK__InvoiceDe__Updat__7FB5F314] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoiceDetails] CHECK CONSTRAINT [FK__InvoiceDe__Updat__7FB5F314]
GO


