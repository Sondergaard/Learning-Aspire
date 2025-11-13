IF OBJECT_ID(N'dbo.Todos', N'U') IS NULL
BEGIN
CREATE TABLE dbo.Todos (
                           Id INT IDENTITY(1,1) PRIMARY KEY,
                           Title NVARCHAR(255) NOT NULL,
                           IsCompleted BIT NOT NULL DEFAULT (0)
);
END