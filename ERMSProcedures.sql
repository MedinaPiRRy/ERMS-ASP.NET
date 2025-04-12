-- Insert a new employee
CREATE PROCEDURE AddEmployee
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Role NVARCHAR(50),
    ManagerId INT = NULL,
    IdentityUserId NVARCHAR(450)
AS
BEGIN
    INSERT INTO Employees (FullName, Email, Role, ManagerId, IdentityUserId)
    VALUES (FullName, Email, Role, ManagerId, IdentityUserId)
END

-- Get all employees assigned to a project
CREATE PROCEDURE GetEmployeesByProject
    @rojectId INT
AS
BEGIN
    SELECT E.Id, E.FullName, E.Email
    FROM Employees E
    INNER JOIN ProjectAssignments PA ON E.Id = PA.EmployeeId
    WHERE PA.ProjectId = ProjectId
END

-- Update status of a task
CREATE PROCEDURE UpdateTaskStatus
    TaskId INT,
    Status NVARCHAR(50)
AS
BEGIN
    UPDATE TaskItems
    SET Status = Status
    WHERE Id = TaskId
END

-- Check employee task summary (SSRS utility)
CREATE PROCEDURE GetEmployeeTaskSummary
AS
BEGIN
    SELECT E.FullName, COUNT(T.Id) AS TotalTasks,
           SUM(CASE WHEN T.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks
    FROM Employees E
    LEFT JOIN TaskItems T ON E.Id = T.EmployeeId
    GROUP BY E.FullName
END

-- Query for Report Builder:
-- Employee performance and project progress
SELECT 
    E.FullName,
    COUNT(T.Id) AS TotalTasks,
    SUM(CASE WHEN T.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
    CAST(
        CASE 
            WHEN COUNT(T.Id) = 0 THEN 0
            ELSE 100.0 * SUM(CASE WHEN T.Status = 'Completed' THEN 1 ELSE 0 END) / COUNT(T.Id)
        END AS DECIMAL(5,2)
    ) AS CompletionRate
FROM Employees E
LEFT JOIN TaskItems T ON E.Id = T.EmployeeId
GROUP BY E.FullName
ORDER BY CompletionRate DESC
