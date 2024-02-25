CREATE TABLE [dbo].[RolePermissions] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER NULL,
    [ScreenId]        UNIQUEIDENTIFIER NOT NULL,
    [FieldId]         UNIQUEIDENTIFIER NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RolePermissions_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_RolePermissions_Fields_FieldId] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[Fields] ([Id]),
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
    CONSTRAINT [FK_RolePermissions_Screens_ScreenId] FOREIGN KEY ([ScreenId]) REFERENCES [dbo].[Screens] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RolePermissions_CreatedByUserId]
    ON [dbo].[RolePermissions]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RolePermissions_FieldId]
    ON [dbo].[RolePermissions]([FieldId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RolePermissions_RoleId]
    ON [dbo].[RolePermissions]([RoleId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RolePermissions_ScreenId]
    ON [dbo].[RolePermissions]([ScreenId] ASC);

