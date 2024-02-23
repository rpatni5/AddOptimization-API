USE [AddOptimization]
GO
 
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ApplicationUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](200) NULL,
	[LastName] [nvarchar](200) NULL,
	[Email] [nvarchar](200) NULL,
	[UserName] [nvarchar](200) NOT NULL,
	[Password] [nvarchar](200) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[LastLogin] [datetime] NULL,
	[FullName] [nvarchar](500) NULL,
	[CreatedByUserId] [int] NULL,
	[UpdatedByUserId] [int] NULL,
	[IsEmailsEnabled] [bit] NULL,
	[FailedLoginAttampts] [int] NULL,
	[IsLocked] [bit] NULL,
 CONSTRAINT [PK_ApplicationUsers_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ApplicationUsers]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationUsers_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ApplicationUsers] CHECK CONSTRAINT [FK_ApplicationUsers_CreatedByUserId]
GO

ALTER TABLE [dbo].[ApplicationUsers]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationUsers_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[ApplicationUsers] ([Id])
GO

ALTER TABLE [dbo].[ApplicationUsers] CHECK CONSTRAINT [FK_ApplicationUsers_UpdatedByUserId]
GO


