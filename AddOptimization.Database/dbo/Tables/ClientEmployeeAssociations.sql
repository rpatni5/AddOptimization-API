CREATE TABLE [dbo].[CustomerEmployeeAssociations](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[ApproverId] [int] NOT NULL,
	[DailyWeightage] [decimal](10, 2) NOT NULL,
	[Overtime] [decimal](10, 2) NOT NULL,
	[PublicHoliday] [decimal](10, 2) NOT NULL,
	[Saturday] [decimal](10, 2) NOT NULL,
	[Sunday] [decimal](10, 2) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsAutoInvoicingEnabled] [bit] NULL,
	[JobTitle] [varchar](200) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[PublicHolidayCountryId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD FOREIGN KEY([ApproverId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations] ADD  DEFAULT ((1)) FOR [IsAutoInvoicingEnabled]
GO
ALTER TABLE [dbo].[CustomerEmployeeAssociations]  WITH CHECK ADD  CONSTRAINT [FK_CustomerEmployeeAssociations_Country] FOREIGN KEY([PublicHolidayCountryId])
REFERENCES [dbo].[Countries] ([Id])
GO

ALTER TABLE [dbo].[CustomerEmployeeAssociations] CHECK CONSTRAINT [FK_CustomerEmployeeAssociations_Country]
GO

