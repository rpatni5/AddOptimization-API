CREATE TABLE [dbo].[InvoiceStatuses](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NULL,
	[StatusKey] [nvarchar](200) NULL,
 CONSTRAINT [PK_InvoiceStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InvoiceStatuses]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceStatuses_ApplicationUsers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoiceStatuses] CHECK CONSTRAINT [FK_InvoiceStatuses_ApplicationUsers_CreatedByUserId]
GO

ALTER TABLE [dbo].[InvoiceStatuses]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceStatuses_ApplicationUsers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[InvoiceStatuses] CHECK CONSTRAINT [FK_InvoiceStatuses_ApplicationUsers_UpdatedByUserId]
GO

