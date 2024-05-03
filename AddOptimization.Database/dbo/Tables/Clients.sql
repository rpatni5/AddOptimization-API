CREATE TABLE [dbo].[Clients]
(
	[Id] uniqueidentifier NOT NULL,
	[FirstName] [nvarchar](200) NULL,
	[LastName] [nvarchar](200) NULL,
	[Organization] [nvarchar](2000) NULL,
	[ManagerName] [nvarchar](200) NULL,
	[Email] [nvarchar](200) NULL,
	[CountryId] [uniqueidentifier] NOT NULL,
	[IsApprovalRequired] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Clients_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Clients_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Clients_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Countries] ([Id]) ON DELETE CASCADE

)
GO
CREATE NONCLUSTERED INDEX [IX_Clients_UpdatedByUserId]
    ON [dbo].[Clients]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Clients_CountryId]
    ON [dbo].[Clients]([CountryId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Clients_CreatedByUserId]
    ON [dbo].[Clients]([CreatedByUserId] ASC);
