
CREATE TABLE [dbo].[QuoteSummaries](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Vat] [decimal](10, 2) NULL,
	[UnitPrice] [decimal](10, 2) NULL,
	[TotalPriceExcVat] [decimal](10, 2) NULL,
	[TotalPriceIncVat] [decimal](10, 2) NULL,
	[QuoteId] [bigint] NOT NULL,
    [Description] [varchar](500) NULL,
	[Quantity] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[QuoteSummaries]  WITH CHECK ADD  CONSTRAINT [FK_QuoteSummaries_Quote] FOREIGN KEY([QuoteId])
REFERENCES [dbo].[Quotes] ([Id])
GO

ALTER TABLE [dbo].[QuoteSummaries] CHECK CONSTRAINT [FK_QuoteSummaries_Quote]
GO
