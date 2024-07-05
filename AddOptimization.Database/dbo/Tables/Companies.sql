CREATE TABLE [dbo].[Companies](
	[Id] [uniqueidentifier] NOT NULL,
	[CompanyName] [nvarchar](200) NULL,
	[Email] [nvarchar](100) NULL,
	[Website] [nvarchar](100) NULL,
	[MobileNumber] [nvarchar](100) NULL,
	[BankName] [nvarchar](200) NULL,
	[BankAccountName] [nvarchar](200) NULL,
	[BankAccountNumber] [nvarchar](200) NULL,
	[BankAddress] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[Address] [nvarchar](200) NULL,
	[City] [nvarchar](200) NULL,
	[Country] [nvarchar](200) NULL,
	[ZipCode] [int] NULL,
	[SwiftCode] [varchar](100) NULL,
	[State] [varchar](100) NULL,
	[TaxNumber] [varchar](100) NULL,
	[DialCodeId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Companies]  WITH CHECK ADD FOREIGN KEY([DialCodeId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Companies]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Companies]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


