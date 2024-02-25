CREATE TABLE [dbo].[Screens] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [Name]      NVARCHAR (100)   NOT NULL,
    [ScreenKey] NVARCHAR (100)   NOT NULL,
    [Route]     NVARCHAR (500)   NULL,
    CONSTRAINT [PK_Screens] PRIMARY KEY CLUSTERED ([Id] ASC)
);

