CREATE TABLE [dbo].[Fields] (
    [Id]       UNIQUEIDENTIFIER NOT NULL,
    [ScreenId] UNIQUEIDENTIFIER NULL,
    [Name]     NVARCHAR (100)   NOT NULL,
    [FieldKey] NVARCHAR (200)   NULL,
    CONSTRAINT [PK_Fields] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Fields_Screens_ScreenId] FOREIGN KEY ([ScreenId]) REFERENCES [dbo].[Screens] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Fields_ScreenId]
    ON [dbo].[Fields]([ScreenId] ASC);

