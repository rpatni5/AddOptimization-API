CREATE TABLE [dbo].[LicenseDevices] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [CustomerId]      UNIQUEIDENTIFIER NOT NULL,
    [MachineName]     NVARCHAR (MAX)   NULL,
    [LicenseId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    [UpdatedAt]       DATETIME2 (7)    NULL,
    [UpdatedByUserId] INT              NULL,
    CONSTRAINT [PK_LicenseDevices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LicenseDevices_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_LicenseDevices_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_LicenseDevices_Licenses_LicenseId] FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[Licenses] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevices_UpdatedByUserId]
    ON [dbo].[LicenseDevices]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevices_LicenseId]
    ON [dbo].[LicenseDevices]([LicenseId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevices_CreatedByUserId]
    ON [dbo].[LicenseDevices]([CreatedByUserId] ASC);

