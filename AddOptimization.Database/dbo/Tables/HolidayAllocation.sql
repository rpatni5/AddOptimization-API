CREATE TABLE HolidayAllocation(
    [Id] [uniqueidentifier] NOT NULL,
    [UserId] [int] NOT NULL,
    [Holidays] [int] NOT NULL,
    [CreatedAt] [datetime2](7)  NULL,
    [CreatedByUserId] [int]  NULL,
    [UpdatedAt] [datetime2](7) NULL,
    [UpdatedByUserId] [int] NULL,
    [IsDeleted] [bit] NOT NULL Default 0,
    [IsActive] [bit] NOT NULL Default 1,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES ApplicationUsers(Id)
);
