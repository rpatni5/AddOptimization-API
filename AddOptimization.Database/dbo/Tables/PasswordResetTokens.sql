CREATE TABLE [dbo].[PasswordResetTokens] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [Token]           NVARCHAR (MAX)   NULL,
    [IsExpired]       BIT              NOT NULL,
    [ExpiryDate]      DATETIME2 (7)    NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PasswordResetTokens_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_CreatedByUserId]
    ON [dbo].[PasswordResetTokens]([CreatedByUserId] ASC);

