CREATE TABLE [dbo].[Roles] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [IsDeleted]       BIT              NOT NULL,
    [Name]            NVARCHAR (100)   NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    [UpdatedAt]       DATETIME2 (7)    NULL,
    [UpdatedByUserId] INT              NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Roles_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Roles_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Roles_CreatedByUserId]
    ON [dbo].[Roles]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Roles_UpdatedByUserId]
    ON [dbo].[Roles]([UpdatedByUserId] ASC);

