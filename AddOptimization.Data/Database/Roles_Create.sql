USE [AddOptimization]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Roles](
    [Id][uniqueidentifier] NOT NULL,
[Name][nvarchar](100) NOT NULL,
    [CreatedAt][datetime] NULL,
    [UpdatedAt][datetime] NULL,
    [CreatedByUserId][int] NULL,
    [UpdatedByUserId][int] NULL,
    [IsDeleted][bit] NOT NULL,
    [DepartmentId][uniqueidentifier] NULL,
 CONSTRAINT[PK_Roles_Id] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]
GO
ALTER TABLE [dbo].[Roles] ADD DEFAULT(newid()) FOR[Id]
GO

ALTER TABLE [dbo].[Roles] ADD CONSTRAINT[DF_Roles_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK_Roles_CreatedByUserId] FOREIGN KEY([CreatedByUserId])
REFERENCES[dbo].[ApplicationUsers]([Id])
GO

ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK_Roles_CreatedByUserId]
GO

ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK_Roles_UpdatedByUserId] FOREIGN KEY([UpdatedByUserId])
REFERENCES[dbo].[ApplicationUsers]([Id])
GO

ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT[FK_Roles_UpdatedByUserId]
GO


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Database
{
    class Roles_Create
    {
    }
}
