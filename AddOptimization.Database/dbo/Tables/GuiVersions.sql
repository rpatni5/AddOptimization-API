CREATE TABLE [dbo].[GuiVersions]
(
	[Id] [uniqueidentifier] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DownloadPath] [varchar](500) NULL,
	[GuiVersionNo] [varchar](500) NULL,
	[FrameworkVersionNo] [varchar](500) NULL,
	[IsLatest] [bit] NOT NULL,
	CONSTRAINT [PK_GuiVersions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GuiVersions_ApplicationUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_GuiVersions_ApplicationUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
)
GO
CREATE NONCLUSTERED INDEX [IX_GuiVersions_UpdatedByUserId]
    ON [dbo].[GuiVersions]([UpdatedByUserId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_GuiVersions_CreatedByUserId]
    ON [dbo].[GuiVersions]([CreatedByUserId] ASC);