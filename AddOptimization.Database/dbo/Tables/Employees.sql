CREATE TABLE [dbo].[Employees](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[IsExternal] [bit] NOT NULL,
	[IsNDASigned] [bit] NOT NULL,
	[Salary] [decimal](10, 2) NULL,
	[BankName] [nvarchar](200) NULL,
	[BankAccountName] [nvarchar](200) NULL,
	[BankAccountNumber] [nvarchar](200) NULL,
	[BillingAddress] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[CountryId] [uniqueidentifier] NULL,
	[State] [varchar](400) NULL,
	[ZipCode] [int] NULL,
	[VATNumber] [varchar](400) NULL,
	[JobTitle] [nvarchar](200) NULL,
	[City] [nvarchar](200) NULL,
	[CompanyName] [nvarchar](200) NULL,
	[Address] [nvarchar](200) NULL,
	[ExternalZipCode] [int] NULL,
	[ExternalCity] [nvarchar](200) NULL,
	[ExternalState] [nvarchar](200) NULL,
	[ExternalCountryId] [uniqueidentifier] NULL,
	[ExternalAddress] [nvarchar](200) NULL,
	[NdaSignDate] [datetime2](7) NULL,
	[BankAddress] [nvarchar](200) NULL,
	[BankState] [nvarchar](100) NULL,
	[BankCity] [nvarchar](100) NULL,
	[BankCountry] [nvarchar](100) NULL,
	[SwiftCode] [nvarchar](100) NULL,
	[BankPostalCode] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Employees] ADD  DEFAULT ((0)) FOR [IsExternal]
GO

ALTER TABLE [dbo].[Employees] ADD  DEFAULT ((0)) FOR [IsNDASigned]
GO

ALTER TABLE [dbo].[Employees]  WITH NOCHECK ADD FOREIGN KEY([CountryId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH NOCHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH NOCHECK ADD FOREIGN KEY([ExternalCountryId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH NOCHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH NOCHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO


