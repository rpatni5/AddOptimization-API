CREATE TABLE [dbo].[License] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [LicenseKey]      NVARCHAR (255)   NULL,
    [LicenseDuration] INT              NOT NULL,
    [NoOfDevices]     INT              NOT NULL,
    [ExpirationDate]  DATETIME2 (7)    NOT NULL,
    [CustomerId]      UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    [UpdatedAt]       DATETIME2 (7)    NULL,
    [UpdatedByUserId] INT              NULL,
    CONSTRAINT [PK_License] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_License_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_License_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_License_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_License_UpdatedByUserId]
    ON [dbo].[License]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_License_CustomerId]
    ON [dbo].[License]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_License_CreatedByUserId]
    ON [dbo].[License]([CreatedByUserId] ASC);

