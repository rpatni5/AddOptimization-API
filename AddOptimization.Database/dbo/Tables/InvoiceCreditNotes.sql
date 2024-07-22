CREATE TABLE [dbo].[InvoiceCreditNotes](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceId] [bigint] NOT NULL,
	[PaymentDate] [datetime2](7) NULL,
	[Description] [nvarchar](200) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[Quantity] [decimal](10, 2) NULL,
	[VatPercent] [decimal](10, 2) NULL,
	[TotalPriceIncludingVat] [decimal](10, 2) NULL,
	[TotalPriceExcludingVat] [decimal](10, 2) NULL,
	[UnitPrice] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceCreditNotes] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[InvoiceCreditNotes] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[InvoiceCreditNotes]  WITH NOCHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoiceCreditNotes]  WITH NOCHECK ADD FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoices] ([Id])
GO

ALTER TABLE [dbo].[InvoiceCreditNotes]  WITH NOCHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


