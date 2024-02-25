CREATE TABLE [dbo].[ApplicationUsers] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]           NVARCHAR (200) NULL,
    [LastName]            NVARCHAR (200) NULL,
    [Email]               NVARCHAR (200) NULL,
    [FullName]            NVARCHAR (500) NULL,
    [UserName]            NVARCHAR (200) NOT NULL,
    [Password]            NVARCHAR (200) NULL,
    [IsActive]            BIT            NOT NULL,
    [IsLocked]            BIT            NULL,
    [IsEmailsEnabled]     BIT            NULL,
    [FailedLoginAttampts] INT            NULL,
    [LastLogin]           DATETIME2 (7)  NULL,
    [CreatedAt]           DATETIME2 (7)  NULL,
    [CreatedByUserId]     INT            NULL,
    [UpdatedAt]           DATETIME2 (7)  NULL,
    [UpdatedByUserId]     INT            NULL,
    CONSTRAINT [PK_ApplicationUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ApplicationUsers_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_ApplicationUsers_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ApplicationUsers_CreatedByUserId]
    ON [dbo].[ApplicationUsers]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ApplicationUsers_UpdatedByUserId]
    ON [dbo].[ApplicationUsers]([UpdatedByUserId] ASC);

