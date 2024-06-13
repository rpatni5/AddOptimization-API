CREATE TABLE [dbo].[Invoices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceNumber] [bigint] NOT NULL,
	[InvoiceDate] [datetime2](7) NOT NULL,
	[InvoiceStatusId] [uniqueidentifier] NOT NULL,
	[PaymentStatusId] [uniqueidentifier] NOT NULL,
	[CustomerAddress] [varchar](400) NULL,
	[CompanyAddress] [varchar](400) NULL,
	[CompanyBankDetails] [varchar](400) NULL,
	[DueDate] [datetime2](7) NULL,
	[Vat] [decimal](10, 2) NULL,
	[TotalPriceIncludingVat] [decimal](10, 2) NULL,
	[TotalPriceExcludingVat] [decimal](10, 2) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,    
	CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Invoices_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE CASCADE
	);

	

GO
CREATE NONCLUSTERED INDEX [IX_Invoices_UpdatedByUserId]
    ON [dbo].[Invoices]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_CustomerId]
    ON [dbo].[Invoices]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_CreatedByUserId]
    ON [dbo].[Invoices]([CreatedByUserId] ASC);

GO 
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO 

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_PaymentStatuses_PaymentStatusId] FOREIGN KEY([PaymentStatusId])
REFERENCES [dbo].[PaymentStatuses] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_PaymentStatuses_PaymentStatusId]
GO

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_InvoiceStatuses_InvoiceStatusId] FOREIGN KEY([InvoiceStatusId])
REFERENCES [dbo].[InvoiceStatuses] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_InvoiceStatuses_InvoiceStatusId]
GO


