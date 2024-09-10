CREATE TABLE [dbo].[Invoices](
	[Id] [bigint] NOT NULL,
	[InvoiceNumber] [varchar](250) NULL,
	[InvoiceDate] [datetime2](7) NOT NULL,
	[InvoiceStatusId] [uniqueidentifier] NOT NULL,
	[PaymentStatusId] [uniqueidentifier] NOT NULL,
	[CustomerAddress] [varchar](400) NULL,
	[CompanyAddress] [varchar](400) NULL,
	[CompanyBankDetails] [varchar](400) NULL,
	[VatValue] [decimal](10, 2) NULL,
	[TotalPriceIncludingVat] [decimal](10, 2) NULL,
	[TotalPriceExcludingVat] [decimal](10, 2) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[PaymentClearanceDays] [int] NULL,
	[DueAmount] [decimal](10, 2) NOT NULL,
	[Metadata] [varchar](max) NULL,
	[HasCreditNotes] [bit] NOT NULL,
	[CreditNoteNumber] [bigint] NULL,
	[HasInvoiceFinalized] [bit] NULL,
 CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Invoices] ADD  CONSTRAINT [DF__Invoices__IsDele__5006DFF2]  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ((0)) FOR [HasCreditNotes]
GO

ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ((0)) FOR [HasInvoiceFinalized]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_ApplicationUsers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_ApplicationUsers_CreatedByUserId]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_ApplicationUsers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_ApplicationUsers_UpdatedByUserId]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_Customers_CustomerId]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_InvoiceStatuses_InvoiceStatusId] FOREIGN KEY([InvoiceStatusId])
REFERENCES [dbo].[InvoiceStatuses] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_InvoiceStatuses_InvoiceStatusId]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_PaymentStatuses_PaymentStatusId] FOREIGN KEY([PaymentStatusId])
REFERENCES [dbo].[PaymentStatuses] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_PaymentStatuses_PaymentStatusId]
GO


