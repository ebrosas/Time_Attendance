/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetFireTeamAndFireWatch
*	Description: Get the Fire Team and Fire Watch member employees
*
*	Date			Author		Rev. #		Comments:
*	06/02/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetFireTeamAndFireWatch
(
	@loadType			TINYINT,	--(Note: 1 => Get all fire team members; 2 => Get all fire watch Members; 3 => Get all fire team / fire watch members)
	@processDate		DATETIME,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = NULL,
	@pageNumber			INT = 1,
	@pageSize			INT = 10	
)
AS 

	--Declare table variable
	DECLARE @tempTable TABLE (totalRows int)

	--Declare variables
	DECLARE	@userCostCenter				VARCHAR(12),
			@CMDMaster					NVARCHAR(MAX), 
			@CMDMaster2					NVARCHAR(MAX), 
			@CMD						VARCHAR(MAX), 
			@CMDTotalRecords			VARCHAR(MAX),
			@WHERE						VARCHAR(MAX),		
			@ORDERBY					VARCHAR(MAX),	
			@chrQuote					CHAR(1),
			@startIndex					INT,
			@endIndex					INT,
			@totalRecords				INT,
			@recordCount				INT,
			@minutes_MinShiftAllowance	INT

	SELECT	@userCostCenter				= '',
			@WHERE						= 'WHERE ISNUMERIC(b.PayStatus) = 1',
			@ORDERBY					= ' ORDER BY EmpNo, SwipeTime DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate the parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	IF @loadType = 1	--Get All Fire Team members
	BEGIN
    
		SET @CMDTotalRecords = '
									SELECT COUNT(*) 
									FROM tas.Vw_AllFireTeamMembers a
										INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo 
								'

		SET @CMD = '
						SELECT	DISTINCT
								a.SwipeDate, a.SwipeTime, a.SwipeLocation, a.SwipeType, a.Notes, a.EmpNo, b.EmpName, b.Position, b.GradeCode, RTRIM(b.BusinessUnit) AS CostCenter, RTRIM(e.BUname) AS CostCenterName,
								ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'''') AS Extension, ISNULL(LTRIM(RTRIM(d.WPPH1)), '''') AS MobileNo, f.Effective_ShiftPatCode AS ShiftPatCode, f.Effective_ShiftPointer AS ShiftPointer,
								f.Effective_ShiftCode AS ShiftCode, b.SupervisorNo AS SupervisorEmpNo, RTRIM(g.EmpName) AS SupervisorEmpName
						FROM tas.Vw_AllFireTeamMembers a
							INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
							LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = ''EXT''
							LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = ''MOBS''
							LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
							LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
							LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
				'
	END 

	ELSE IF @loadType = 2	--Get All Fire Watch members
	BEGIN
    
		SET @CMDTotalRecords = '
									SELECT COUNT(*) 
									FROM tas.Vw_AllFireWatchMembers a
										INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo 
								'

		SET @CMD = '
						SELECT	DISTINCT
								a.SwipeDate, a.SwipeTime, a.SwipeLocation, a.SwipeType, a.Notes, a.EmpNo, b.EmpName, b.Position, b.GradeCode, RTRIM(b.BusinessUnit) AS CostCenter, RTRIM(e.BUname) AS CostCenterName,
								ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'''') AS Extension, ISNULL(LTRIM(RTRIM(d.WPPH1)), '''') AS MobileNo, f.Effective_ShiftPatCode AS ShiftPatCode, f.Effective_ShiftPointer AS ShiftPointer,
								f.Effective_ShiftCode AS ShiftCode, b.SupervisorNo AS SupervisorEmpNo, RTRIM(g.EmpName) AS SupervisorEmpName
						FROM tas.Vw_AllFireWatchMembers a
							INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
							LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = ''EXT''
							LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = ''MOBS''
							LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
							LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
							LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
				'
	END 

	ELSE IF @loadType = 3	--Get All Fire Watch and Fire Watch members
	BEGIN
    
		SET @CMDTotalRecords = '
									SELECT COUNT(*) 
									FROM tas.Vw_FireTeamAndFireWatchMember a
										INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo 
								'

		SET @CMD = '
						SELECT	DISTINCT
								a.SwipeDate, a.SwipeTime, a.SwipeLocation, a.SwipeType, a.Notes, a.EmpNo, b.EmpName, b.Position, b.GradeCode, RTRIM(b.BusinessUnit) AS CostCenter, RTRIM(e.BUname) AS CostCenterName,
								ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'''') AS Extension, ISNULL(LTRIM(RTRIM(d.WPPH1)), '''') AS MobileNo, f.Effective_ShiftPatCode AS ShiftPatCode, f.Effective_ShiftPointer AS ShiftPointer,
								f.Effective_ShiftCode AS ShiftCode, b.SupervisorNo AS SupervisorEmpNo, RTRIM(g.EmpName) AS SupervisorEmpName
						FROM tas.Vw_FireTeamAndFireWatchMember a
							INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
							LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = ''EXT''
							LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = ''MOBS''
							LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
							LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
							LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
				'
	END 

	--Add date filter
	IF @processDate IS NOT NULL 
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.SwipeDate, 12) = ' + @chrQuote + CONVERT(varchar, @processDate, 12) + @chrQuote 

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add Cost Center filter
	IF @costCenter IS NOT NULL	
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(b.BusinessUnit) = ' + @chrQuote + RTRIM(@costCenter) + @chrQuote

	--Get the total records
	SELECT @CMDMaster2 = @CMDTotalRecords + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END

	INSERT INTO @tempTable
	EXEC sp_executesql @CMDMaster2 
	SELECT @totalRecords = totalRows FROM @tempTable

	--Build the final query
	SELECT @CMDMaster = 
		'SELECT * FROM 
		(
			SELECT *, ' + CONVERT(VARCHAR(10), @totalRecords) + 
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY EmpNo, SwipeTime DESC) as RowNumber 
			FROM (' + @CMD + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)		
		+ @ORDERBY
	
	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

GO 

/*	Debug:

PARAMETERS:
	@loadType			TINYINT,	--(Note: 1 => Get all fire team members; 2 => Get all fire watch Members; 3 => Get all fire team / fire watch members)
	@processDate		DATETIME,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = NULL,
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	--All Fire Team
	EXEC tas.Pr_GetFireTeamAndFireWatch 1, '02/06/2018', 0, '', 1, 100 
	EXEC tas.Pr_GetFireTeamAndFireWatch 1, '02/06/2018', 10001628
	EXEC tas.Pr_GetFireTeamAndFireWatch 1, '02/06/2018', 0, '5200'

	--All Fire Watch
	EXEC tas.Pr_GetFireTeamAndFireWatch 2, '02/06/2018' 
	EXEC tas.Pr_GetFireTeamAndFireWatch 2, '02/06/2018', 10001628
	EXEC tas.Pr_GetFireTeamAndFireWatch 2, '02/06/2018', 0, '5200'

	--Fire Team & Fire Watch
	EXEC tas.Pr_GetFireTeamAndFireWatch 3, '02/06/2018' 
	EXEC tas.Pr_GetFireTeamAndFireWatch 3, '02/06/2018', 10001628
	EXEC tas.Pr_GetFireTeamAndFireWatch 3, '02/06/2018', 0, '5200'

*/


