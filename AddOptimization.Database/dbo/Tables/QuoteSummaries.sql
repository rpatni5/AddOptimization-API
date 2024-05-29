
CREATE TABLE [dbo].[QuoteSummaries](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Quantity] [int] NULL,
	[Vat] [int] NULL,
	[UnitPrice] [int] NULL,
	[TotalPriceExcVat] [int] NULL,
	[TotalPriceIncVat] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

