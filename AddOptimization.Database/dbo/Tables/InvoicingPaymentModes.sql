
CREATE TABLE [dbo].[InvoicingPaymentModes](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[ModeKey] [nvarchar](200) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedByUserId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoicingPaymentModes] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[InvoicingPaymentModes]  WITH CHECK ADD  CONSTRAINT [FK_InvoicePayment_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoicingPaymentModes] CHECK CONSTRAINT [FK_InvoicePayment_CreatedByUser]
GO

ALTER TABLE [dbo].[InvoicingPaymentModes]  WITH CHECK ADD  CONSTRAINT [FK_InvoicePayment_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoicingPaymentModes] CHECK CONSTRAINT [FK_InvoicePayment_UpdatedByUser]
GO


