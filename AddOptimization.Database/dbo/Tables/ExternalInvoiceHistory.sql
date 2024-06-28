
CREATE TABLE [dbo].[ExternalInvoiceHistory](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceId] [bigint] NOT NULL,
	[InvoiceStatusId] [uniqueidentifier] NOT NULL,
	[Comment] [varchar](300) NULL,
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

ALTER TABLE [dbo].[ExternalInvoiceHistory]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceHistory]  WITH CHECK ADD FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[ExternalInvoices] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceHistory]  WITH CHECK ADD FOREIGN KEY([InvoiceStatusId])
REFERENCES [dbo].[InvoiceStatuses] ([Id])
GO

ALTER TABLE [dbo].[ExternalInvoiceHistory]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO



