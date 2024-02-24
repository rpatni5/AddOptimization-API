USE [AddOptimization]
GO 
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Email] [nvarchar](200) NULL,
	[Company] [nvarchar](200) NULL,
	[Phone] [nvarchar](200) NULL,
	[Birthday] [nvarchar](200) NULL,
	[Color] [nvarchar](100) NULL,
	[Photos] [nvarchar](2000) NULL,
	[SocialProfiles] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[BillingAddressId] [uniqueidentifier] NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedByUserId] [int] NULL,
	[Notes] [nvarchar](4000) NULL, 
	[CustomerStatusId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Customers_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_CustomerStatusId]  DEFAULT ('17756728-9DE6-409F-9D23-B8B5BA253F0E') FOR [CustomerStatusId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_BillingAddressId] FOREIGN KEY([BillingAddressId])
REFERENCES [dbo].[Addresses] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_BillingAddressId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_CreatedByUserId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_CustomerStatusId] FOREIGN KEY([CustomerStatusId])
REFERENCES [dbo].[CustomerStatuses] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_CustomerStatusId]
GO

ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_UpdatedByUserId]
GO


