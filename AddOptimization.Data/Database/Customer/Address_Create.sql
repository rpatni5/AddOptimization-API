USE [AddOptimization]
GO
 
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Addresses](
	[Id] [uniqueidentifier] NOT NULL,
	[TargetType] [nvarchar](50) NULL,
	[TargetId] [int] NULL,
	[Name] [nvarchar](200) NULL,
	[Phone] [nvarchar](50) NULL,
	[Address1] [nvarchar](200) NULL,
	[Address2] [nvarchar](200) NULL,
	[City] [nvarchar](100) NULL,
	[Zip] [nvarchar](20) NULL,
	[Province] [nvarchar](100) NULL,
	[ProvinceCode] [nvarchar](10) NULL,
	[Country] [nvarchar](100) NULL,
	[CountryCode] [nvarchar](5) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL, 
	[ExternalId] [int] NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
	[CustomerId] [uniqueidentifier] NULL,
	[GPSCoordinates] [nvarchar](100) NULL,
 CONSTRAINT [PK_Addresses_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Addresses] ADD  CONSTRAINT [DF_Addresses_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[Addresses] ADD  CONSTRAINT [DF_Addresses_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO 

ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_CreatedByUserId]
GO

ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_CustomerId]
GO

ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_UpdatedByUserId]
GO


