CREATE TABLE [dbo].[LeaveStatuses](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedByUserId] [int] NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
 
ALTER TABLE [dbo].[LeaveStatuses] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
 
ALTER TABLE [dbo].[LeaveStatuses] ADD  DEFAULT ((1)) FOR [IsActive]
GO
 