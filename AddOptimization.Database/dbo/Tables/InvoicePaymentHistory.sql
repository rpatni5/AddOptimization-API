
CREATE TABLE [dbo].[InvoicePaymentHistory](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceId] [bigint] NOT NULL,
	[InvoiceStatusId] [uniqueidentifier] NOT NULL,
	[PaymentStatusId] [uniqueidentifier] NOT NULL,
	[PaymentDate] [datetime2](7) NULL,
	[Amount] [decimal](10, 2) NULL,
	[TransactionId] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoicePaymentHistory] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[InvoicePaymentHistory] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[InvoicePaymentHistory]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoicePaymentHistory]  WITH CHECK ADD FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoices] ([Id])
GO

ALTER TABLE [dbo].[InvoicePaymentHistory]  WITH CHECK ADD FOREIGN KEY([InvoiceStatusId])
REFERENCES [dbo].[InvoiceStatuses] ([Id])
GO

ALTER TABLE [dbo].[InvoicePaymentHistory]  WITH CHECK ADD FOREIGN KEY([PaymentStatusId])
REFERENCES [dbo].[PaymentStatuses] ([Id])
GO

ALTER TABLE [dbo].[InvoicePaymentHistory]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


