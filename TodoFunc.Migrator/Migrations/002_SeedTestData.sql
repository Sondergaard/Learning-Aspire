-- Insert sample test data if not already present
IF NOT EXISTS (SELECT 1 FROM dbo.Todos WHERE Title = 'Learn .NET Aspire')
BEGIN
    INSERT INTO dbo.Todos (Title, IsCompleted) VALUES
    ('Learn .NET Aspire', 0),
    ('Separate migration concerns', 1),
    ('Implement proper security', 0);
END

