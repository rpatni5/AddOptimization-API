CREATE TABLE [dbo].[Users] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [FullName]   NVARCHAR (200) NULL,
    [ProviderId] NVARCHAR (200) NULL,
    [LastAction] DATETIME2 (7)  NULL,
    [CreatedAt]  DATETIME2 (7)  NULL,
    [UpdatedAt]  DATETIME2 (7)  NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

