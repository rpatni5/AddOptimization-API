
CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Email] [nvarchar](200) NULL,
	[Phone] [nvarchar](200) NULL,
	[Birthday] [nvarchar](200) NULL,
	[ContactInfo] [nvarchar](500) NULL,
	[Organizations] [nvarchar](2000) NULL,
	[BillingAddressId] [uniqueidentifier] NULL,
	[CustomerStatusId] [uniqueidentifier] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[ExternalId] [int] NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[CountryCode] [nvarchar](50) NULL,
	[ManagerName] [nvarchar](200) NULL,
	[IsApprovalRequired] [bit] NOT NULL,
	[CountryId] [uniqueidentifier] NULL,
	[VAT] [decimal](5, 2) NULL,
	[PaymentClearanceDays] [int] NULL,
	[PartnerName] [nvarchar](200) NULL,
	[PartnerBankName] [nvarchar](200) NULL,
	[PartnerBankAccountName] [nvarchar](200) NULL,
	[PartnerBankAccountNumber] [nvarchar](200) NULL,
	[PartnerPostalCode] [int] NULL,
	[PartnerAddress] [nvarchar](200) NULL,
	[PartnerDescriptions] [nvarchar](200) NULL,
	[PartnerCountryId] [uniqueidentifier] NULL,
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

