CREATE TABLE [dbo].[AbsenceRequest](
	[Id] [uniqueidentifier] NOT NULL,
	[Comment] [nvarchar](500) NULL,
	[Date] [datetime] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[UserId] [int] NOT NULL,
	[LeaveStatusId] [int] NOT NULL
) ON [PRIMARY]
 
GO
 
ALTER TABLE [dbo].[AbsenceRequest] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
 
ALTER TABLE [dbo].[AbsenceRequest] ADD  DEFAULT ((1)) FOR [IsActive]
GO