
CREATE TABLE [dbo].[ExternalInvoices](
	[Id] [bigint] NOT NULL,
	[InvoiceNumber] [bigint] NOT NULL,
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
	[ExpiryDate] [datetime2](7) NOT NULL,
	[PaymentClearanceDays] [int] NULL,
	[CompanyName] [varchar](400) NULL,
	[CompanyId] [uniqueidentifier] NULL,
	[EmployeeId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalInvoices] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Companies] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoices_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices] CHECK CONSTRAINT [FK_ExternalInvoices_CreatedByUser]
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoices_ExternalInvoiceStatus] FOREIGN KEY([InvoiceStatusId])
REFERENCES [dbo].[InvoiceStatuses] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices] CHECK CONSTRAINT [FK_ExternalInvoices_ExternalInvoiceStatus]
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoices_PaymentStatuses] FOREIGN KEY([PaymentStatusId])
REFERENCES [dbo].[PaymentStatuses] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices] CHECK CONSTRAINT [FK_ExternalInvoices_PaymentStatuses]
GO

ALTER TABLE [dbo].[ExternalInvoices]  WITH CHECK ADD  CONSTRAINT [FK_ExternalInvoices_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoices] CHECK CONSTRAINT [FK_ExternalInvoices_UpdatedByUser]
GO


