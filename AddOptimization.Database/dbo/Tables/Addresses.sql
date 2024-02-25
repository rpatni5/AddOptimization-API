CREATE TABLE [dbo].[Addresses] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [TargetType]      NVARCHAR (50)    NULL,
    [TargetId]        INT              NULL,
    [Name]            NVARCHAR (200)   NULL,
    [Phone]           NVARCHAR (50)    NULL,
    [Address1]        NVARCHAR (200)   NULL,
    [Address2]        NVARCHAR (200)   NULL,
    [City]            NVARCHAR (100)   NULL,
    [Zip]             NVARCHAR (20)    NULL,
    [Province]        NVARCHAR (100)   NULL,
    [ProvinceCode]    NVARCHAR (10)    NULL,
    [Country]         NVARCHAR (100)   NULL,
    [CountryCode]     NVARCHAR (5)     NULL,
    [CustomerId]      UNIQUEIDENTIFIER NULL,
    [ExternalId]      INT              NULL,
    [IsDeleted]       BIT              NOT NULL,
    [GPSCoordinates]  NVARCHAR (100)   NULL,
    [CreatedAt]       DATETIME2 (7)    NULL,
    [CreatedByUserId] INT              NULL,
    [UpdatedAt]       DATETIME2 (7)    NULL,
    [UpdatedByUserId] INT              NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Addresses_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Addresses_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Addresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_CreatedByUserId]
    ON [dbo].[Addresses]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_CustomerId]
    ON [dbo].[Addresses]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_UpdatedByUserId]
    ON [dbo].[Addresses]([UpdatedByUserId] ASC);

