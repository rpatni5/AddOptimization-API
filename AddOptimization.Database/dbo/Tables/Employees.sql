CREATE TABLE [dbo].[Employees](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[IsExternal] [bit] NOT NULL,
	[Salary] [decimal](10, 2) NULL,
	[BankName] [nvarchar](200) NULL,
	[BankAccountName] [nvarchar](200) NULL,
	[BankAccountNumber] [nvarchar](200) NULL,
	[BillingAddress] [nvarchar](500) NULL,
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

ALTER TABLE [dbo].[Employees] ADD  DEFAULT ((0)) FOR [IsExternal]
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[Employees]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO
