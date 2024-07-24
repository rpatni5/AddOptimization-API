CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[Phone] [nvarchar](200) NULL,
	[Organizations] [nvarchar](2000) NULL,
	[BillingAddressId] [uniqueidentifier] NULL,
	[CustomerStatusId] [uniqueidentifier] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[ManagerName] [nvarchar](200) NULL,
	[IsApprovalRequired] [bit] NOT NULL,
	[CountryId] [uniqueidentifier] NULL,
	[VAT] [decimal](5, 2) NULL,
	[PaymentClearanceDays] [int] NULL,
	[PartnerName] [nvarchar](200) NULL,
	[PartnerBankName] [nvarchar](200) NULL,
	[PartnerBankAccountName] [nvarchar](200) NULL,
	[PartnerBankAccountNumber] [nvarchar](200) NULL,
	[PartnerAddress] [nvarchar](200) NULL,
	[PartnerDescriptions] [nvarchar](200) NULL,
	[PartnerCountryId] [uniqueidentifier] NULL,
	[Address] [nvarchar](200) NULL,
	[City] [nvarchar](200) NULL,
	[Country] [nvarchar](200) NULL,
	[ZipCode] [int] NULL,
	[PartnerCity] [nvarchar](200) NULL,
	[PartnerZipCode] [int] NULL,
	[PartnerAddress2] [varchar](400) NULL,
	[Address2] [varchar](400) NULL,
	[VATNumber] [varchar](400) NULL,
	[PartnerVATNumber] [varchar](400) NULL,
	[PartnerState] [varchar](400) NULL,
	[State] [varchar](400) NULL,
	[PartnerCompany] [varchar](400) NULL,
	[ManagerPhone] [varchar](400) NULL,
	[ManagerEmail] [varchar](400) NULL,
	[PartnerEmail] [varchar](400) NULL,
	[PartnerPhone] [varchar](400) NULL,
	[PartnerBankAddress] [varchar](400) NULL,
	[CountryCodeId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_CustomerStatusId]  DEFAULT ('17756728-9DE6-409F-9D23-B8B5BA253F0E') FOR [CustomerStatusId]
GO

ALTER TABLE [dbo].[Customers] ADD  DEFAULT ((0)) FOR [IsApprovalRequired]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD FOREIGN KEY([CountryCodeId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Addresses_BillingAddressId] FOREIGN KEY([BillingAddressId])
REFERENCES [dbo].[Addresses] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Addresses_BillingAddressId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_ApplicationUsers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_ApplicationUsers_CreatedByUserId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_ApplicationUsers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_ApplicationUsers_UpdatedByUserId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Countries_CountryId] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Countries_CountryId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Countries_PartnerCountryId] FOREIGN KEY([PartnerCountryId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Countries_PartnerCountryId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_CustomerStatuses_CustomerStatusId] FOREIGN KEY([CustomerStatusId])
REFERENCES [dbo].[CustomerStatuses] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_CustomerStatuses_CustomerStatusId]
GO

