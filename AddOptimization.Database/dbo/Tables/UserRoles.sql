CREATE TABLE [dbo].[UserRoles] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [UserId]          INT              NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserRoles_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_UserRoles_ApplicationUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserRoles_CreatedByUserId]
    ON [dbo].[UserRoles]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserRoles_RoleId]
    ON [dbo].[UserRoles]([RoleId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserRoles_UserId]
    ON [dbo].[UserRoles]([UserId] ASC);

