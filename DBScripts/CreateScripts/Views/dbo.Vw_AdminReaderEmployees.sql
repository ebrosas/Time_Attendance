/***************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: dbo.Vw_AdminReaderEmployees
*	Description: Get the list of all employees who are required to swipe in the Admin Bldg. readers
*
*	Date:			Author:		Rev. #:		Comments:
*	07/08/2022		Ervin		1.0			Created
*
*****************************************************************************************************************************************************************************************************/

CREATE VIEW dbo.Vw_AdminReaderEmployees
AS
	
	SELECT	
		x.CostCenter, x.CostCenterName, 
		y.LocationName, x.ReaderNo, y.ReaderName, 
		x.UserID, 
		x.EmpNo, x.EmpName, x.Position, x.PayGrade,
		x.ShiftPatCode, 
		CASE WHEN x.IsDayShift = 1 THEN 'Yes' ELSE 'No' END AS IsDayShift
		--,y.* 
	FROM
    (
		--Get employees from Reader #71 - Admin Building 2
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			72 AS ReaderNo,
			a.UserID
		FROM [dbo].[AdminBldg1] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)

		UNION
    
		--Get employees from Reader #72 - Admin Building 1
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			71 AS ReaderNo,
			a.UserID
		FROM [dbo].[AdminBldg2] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #73 - Procurement Bldg.
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			73 AS ReaderNo,
			a.UserID
		FROM [dbo].[ProcurementBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #74 - Safety & Security Bldg.
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			74 AS ReaderNo,
			a.UserID
		FROM [dbo].[SafetySecurityBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #75 - Production Bldg.
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			75 AS ReaderNo,
			a.UserID
		FROM [dbo].[ProductionBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #76 - Project Office
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			76 AS ReaderNo,
			a.UserID
		FROM [dbo].[ProjectOffice] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #77 - Technical Bldg.
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			77 AS ReaderNo,
			a.UserID
		FROM [dbo].[TechnicalBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #78 - Engineering East Door
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			78 AS ReaderNo,
			a.UserID
		FROM [dbo].[EngineeringEastDoor] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #79 - Engineering West Door
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			79 AS ReaderNo,
			a.UserID
		FROM [dbo].[EngineeringWestDoor] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	
		UNION

		--Get employees from Reader #80 - Foil Mill Bldg. (Notes: There is 1 employee who has invalid value for "C_Unique" field in [UNIS_tUser] table
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			80 AS ReaderNo,
			a.UserID
		FROM [dbo].[FoilMillBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
		--WHERE ISNUMERIC(b.C_Unique) = 0
	
		UNION

		--Get employees from Reader #81 - Remlt Bldg
		SELECT	RTRIM(c.BusinessUnit) AS CostCenter, RTRIM(d.BUname) AS CostCenterName, c.EmpNo, c.EmpName, c.Position, c.GradeCode AS PayGrade,
			e.ShiftPatCode, f.IsDayShift,
			81 AS ReaderNo,
			a.UserID
		FROM [dbo].[RemeltBldg] a WITH (NOLOCK)
			INNER JOIN [tas].[UNIS_tUser] b WITH (NOLOCK) ON a.UserID = b.L_ID AND ISNUMERIC(b.C_Unique) = 1
			INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON CAST(b.C_Unique AS INT) + 10000000 = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.Master_EmployeeAdditional e WITH (NOLOCK) ON c.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
	) x
	LEFT JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.SourceID = 2

GO 	

/*
	SELECT * FROM [dbo].[AdminBuilding2] a WITH (NOLOCK) 
    
	SELECT * FROM [tas].[UNIS_tUser] a
	WHERE a.L_ID = 193

	SELECT * FROM [tas].[unis_tenter_alpeta] a

	SELECT * FROM [dbo].[FoilMillBldg] a WITH (NOLOCK)

	BEGIN TRAN T1

	UPDATE [dbo].[FoilMillBldg]
	SET UserID = 956
	WHERE RTRIM(EmpName) = 'MOHAMED ISA ALSH'

	COMMIT TRAN T1

*/