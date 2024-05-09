CREATE TABLE [dbo].[PublicHolidays]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](20) NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[Date] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CountryId] [uniqueidentifier] NULL,
	CONSTRAINT [PK_PublicHolidays] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PublicHolidays_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_PublicHolidays_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
	CONSTRAINT [FK_PublicHolidays_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Countries] ([Id]) ON DELETE CASCADE
)
GO
CREATE NONCLUSTERED INDEX [IX_PublicHolidays_UpdatedByUserId]
    ON [dbo].[PublicHolidays]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PublicHolidays_CountryId]
    ON [dbo].[PublicHolidays]([CountryId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PublicHolidays_CreatedByUserId]
    ON [dbo].[PublicHolidays]([CreatedByUserId] ASC);