CREATE TABLE [dbo].[Customers] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Name]             NVARCHAR (200)   NULL,
    [Email]            NVARCHAR (200)   NULL,
    [Phone]            NVARCHAR (200)   NULL,
    [Birthday]         NVARCHAR (200)   NULL,
    [ContactInfo]      NVARCHAR (500)   NULL,
    [Organizations]    NVARCHAR (2000)  NULL,
    [BillingAddressId] UNIQUEIDENTIFIER NULL,
    [CustomerStatusId] UNIQUEIDENTIFIER NOT NULL,
    [Notes]            NVARCHAR (MAX)   NULL,
    [ExternalId]       INT              NULL,
    [CreatedAt]        DATETIME2 (7)    NULL,
    [CreatedByUserId]  INT              NULL,
    [UpdatedAt]        DATETIME2 (7)    NULL,
    [UpdatedByUserId]  INT              NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Customers_Addresses_BillingAddressId] FOREIGN KEY ([BillingAddressId]) REFERENCES [dbo].[Addresses] ([Id]),
    CONSTRAINT [FK_Customers_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Customers_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Customers_CustomerStatuses_CustomerStatusId] FOREIGN KEY ([CustomerStatusId]) REFERENCES [dbo].[CustomerStatuses] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Customers_BillingAddressId]
    ON [dbo].[Customers]([BillingAddressId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customers_CreatedByUserId]
    ON [dbo].[Customers]([CreatedByUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customers_CustomerStatusId]
    ON [dbo].[Customers]([CustomerStatusId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Customers_UpdatedByUserId]
    ON [dbo].[Customers]([UpdatedByUserId] ASC);

