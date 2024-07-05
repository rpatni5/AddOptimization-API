CREATE TABLE [dbo].[Countries]
( 
    [Id] [uniqueidentifier] NOT NULL,
	[CountryName] [varchar](200) NOT NULL,
	[CountryCode] [char](2) NULL,
	[DialCode] [varchar](50) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Countries_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Countries_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
)
GO
CREATE NONCLUSTERED INDEX [IX_Countries_UpdatedByUserId]
    ON [dbo].[Countries]([UpdatedByUserId] ASC); 

GO
CREATE NONCLUSTERED INDEX [IX_Countries_CreatedByUserId]
    ON [dbo].[Countries]([CreatedByUserId] ASC);