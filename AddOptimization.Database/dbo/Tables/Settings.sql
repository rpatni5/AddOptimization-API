CREATE TABLE [dbo].[Settings]
(
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [varchar](500) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[Description] [varchar](500) NULL,
	[IsEnabled] [bit] NOT NULL Default(0),
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL Default(0)
	CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Settings_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Settings_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
)
GO
CREATE NONCLUSTERED INDEX [IX_Settings_UpdatedByUserId]
    ON [dbo].[Settings]([UpdatedByUserId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Settings_CreatedByUserId]
    ON [dbo].[Settings]([CreatedByUserId] ASC);

