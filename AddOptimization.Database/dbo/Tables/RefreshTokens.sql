CREATE TABLE [dbo].[RefreshTokens] (
    [Id]                INT              IDENTITY (1, 1) NOT NULL,
    [ApplicationUserId] INT              NOT NULL,
    [Token]             UNIQUEIDENTIFIER NOT NULL,
    [IsExpired]         BIT              NOT NULL,
    [ExpiredAt]         DATETIME2 (7)    NULL,
    [CreatedAt]         DATETIME2 (7)    NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RefreshTokens_ApplicationUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RefreshTokens_ApplicationUserId]
    ON [dbo].[RefreshTokens]([ApplicationUserId] ASC);

