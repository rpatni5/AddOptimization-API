CREATE TABLE [dbo].[Licenses] (
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
    [isDeleted]       BIT              DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Licenses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Licenses_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Licenses_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Licenses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Licenses_UpdatedByUserId]
    ON [dbo].[Licenses]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Licenses_CustomerId]
    ON [dbo].[Licenses]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Licenses_CreatedByUserId]
    ON [dbo].[Licenses]([CreatedByUserId] ASC);

