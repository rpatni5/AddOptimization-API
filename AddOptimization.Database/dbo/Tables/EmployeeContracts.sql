﻿
CREATE TABLE [dbo].[EmployeeContracts](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoicingPaymentModeId] [uniqueidentifier] NOT NULL,
	[Hours] [int] NOT NULL,
	[ProjectFees] [decimal](10, 2) NOT NULL,
	[NoticePeriod] [int] NOT NULL,
	[ExpensePaymentFees] [decimal](10, 2) NULL,
	[SignedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[JobTitle] [nvarchar](255) NULL,
	[Address] [nvarchar](255) NULL,
	[EmployeeAssociationId] [uniqueidentifier] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[ProjectStartDate] [datetime2](7) NULL,
	[ProjectEndDate] [datetime2](7) NULL,
	[IsContractSigned] [bit] NOT NULL,
	[ProjectFeePaymentModeId] [uniqueidentifier] NOT NULL,
	[WorkMode] [varchar](50) NULL,
    [ContractName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EmployeeContracts] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[EmployeeContracts] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[EmployeeContracts] ADD  DEFAULT ((1)) FOR [IsContractSigned]
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD  CONSTRAINT [FK__EmployeeC__Custo__43F60EC8] FOREIGN KEY([EmployeeAssociationId])
REFERENCES [dbo].[CustomerEmployeeAssociations] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts] CHECK CONSTRAINT [FK__EmployeeC__Custo__43F60EC8]
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD  CONSTRAINT [FK__EmployeeC__Custo__44EA3301] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts] CHECK CONSTRAINT [FK__EmployeeC__Custo__44EA3301]
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD  CONSTRAINT [FK__EmployeeC__Emplo__45DE573A] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts] CHECK CONSTRAINT [FK__EmployeeC__Emplo__45DE573A]
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD FOREIGN KEY([InvoicingPaymentModeId])
REFERENCES [dbo].[InvoicingPaymentModes] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD FOREIGN KEY([ProjectFeePaymentModeId])
REFERENCES [dbo].[InvoicingPaymentModes] ([Id])
GO

ALTER TABLE [dbo].[EmployeeContracts]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO
