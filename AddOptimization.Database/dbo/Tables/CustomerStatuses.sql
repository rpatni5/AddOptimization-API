CREATE TABLE [dbo].[CustomerStatuses] (
    [Id]   UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_CustomerStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);

