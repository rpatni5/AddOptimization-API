CREATE TABLE [dbo].[LicenseDevice] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [CustomerId]      UNIQUEIDENTIFIER NOT NULL,
    [MotherBoardId]   NVARCHAR (MAX)   NULL,
    [MachineName]     NVARCHAR (MAX)   NULL,
    [LicenseId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    [UpdatedAt]       DATETIME2 (7)    NULL,
    [UpdatedByUserId] INT              NULL,
    CONSTRAINT [PK_LicenseDevice] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LicenseDevice_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_LicenseDevice_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_LicenseDevice_License_LicenseId] FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[License] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevice_UpdatedByUserId]
    ON [dbo].[LicenseDevice]([UpdatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevice_LicenseId]
    ON [dbo].[LicenseDevice]([LicenseId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LicenseDevice_CreatedByUserId]
    ON [dbo].[LicenseDevice]([CreatedByUserId] ASC);

