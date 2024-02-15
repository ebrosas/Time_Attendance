/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetManualTimesheetEntry
*	Description: Get the list of employees who swiped manually at the Main Gate or Foil Mill readers
*
*	Date			Author		Revision No.	Comments:
*	11/07/2016		Ervin		1.0				Created
*	03/09/2016		Ervin		1.1				Added "CreatedTime" field in the Order by clause

******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetManualTimesheetEntry
(   
	@autoID			INT = 0,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = '',
	@dateIN			DATETIME = NULL,
	@dateOUT		DATETIME = NULL,
	@pageNumber		INT = 1,
	@pageSize		INT = 10	
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
			@recordCount				INT

	SELECT	@userCostCenter				= '',
			@WHERE						= 'WHERE',
			@ORDERBY					= ' ORDER BY CreatedTime DESC, LastUpdateTime DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@dateIN, '') = ''
		SET @dateIN = NULL

	IF ISNULL(@dateOUT, '') = ''
		SET @dateOUT = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_ManualAttendance a
								INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
								LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						ISNULL(b.EmpName, d.ContractorEmpName) AS EmpName,
						b.Position,
						b.BusinessUnit,
						c.BusinessUnitName,
						a.dtIN,
						CASE WHEN a.timeIN IS NOT NULL AND ISDATE(LEFT(a.timeIN, 2) + '':'' + RIGHT(a.timeIN, 2) + '':00'') = 1
							THEN CONVERT(TIME, LEFT(a.timeIN, 2) + '':'' + RIGHT(a.timeIN, 2) + '':00'')
							ELSE NULL
						END AS timeIN,
						a.dtOUT,
						CASE WHEN a.timeOUT IS NOT NULL AND ISDATE(LEFT(a.timeOUT, 2) + '':'' + RIGHT(a.timeOUT, 2) + '':00'') = 1
							THEN CONVERT(TIME, LEFT(a.timeOUT, 2) + '':'' + RIGHT(a.timeOUT, 2) + '':00'')
							ELSE NULL
						END AS [timeOUT],
						a.CreatedUser,
						a.CreatedTime,
						ISNULL(a.LastUpdateUser, a.CreatedUser) AS LastUpdateUser,
						ISNULL(a.LastUpdateTime, a.CreatedTime) AS LastUpdateTime,
						CASE WHEN a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor 
				FROM tas.Tran_ManualAttendance a
					LEFT JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)
					LEFT JOIN tas.Master_ContractEmployee d ON a.EmpNo = d.EmpNo '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(b.BusinessUnit) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    

	--Add @dateIN filter
	IF (@dateIN IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.dtIN, 12) = ' + @chrQuote + CONVERT(varchar, @dateIN, 12) + @chrQuote 
	END

	--Add @dateOUT filter
	IF (@dateOUT IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.dtOUT, 12) = ' + @chrQuote + CONVERT(varchar, @dateOUT, 12) + @chrQuote 
	END

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY CreatedTime DESC, LastUpdateTime DESC) as RowNumber 
			FROM (' + @CMD + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)		
		+ @ORDERBY

	--Check if total records to return is greater than 200
	--IF @recordCount > 100
	--	SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

GO 

/*	Debugging:

PARAMETERS:
	@autoID			INT = 0,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = '',
	@dateIN			DATETIME = NULL,
	@dateOUT		DATETIME = NULL,
	@pageNumber		INT = 1,
	@pageSize		INT = 10	

	EXEC tas.Pr_GetManualTimesheetEntry 
	EXEC tas.Pr_GetManualTimesheetEntry 24810							--By AutoID
	EXEC tas.Pr_GetManualTimesheetEntry 0, 10003632						--By Employee No.
	EXEC tas.Pr_GetManualTimesheetEntry 0, 50019						--By Contractor No.	
	EXEC tas.Pr_GetManualTimesheetEntry 0, 0, '7600'					--By Cost Center
	EXEC tas.Pr_GetManualTimesheetEntry 0, 0, '', '02/04/2016'			--By Date In
	EXEC tas.Pr_GetManualTimesheetEntry 0, 0, '', '', '27/03/2016'		--By Date Out
	EXEC tas.Pr_GetManualTimesheetEntry 0, 0, '', '', '', 1, 20			--Last 20 Manual Attendances

	SELECT * FROM tas.Tran_ManualAttendance a 
	ORDER BY a.CreatedTime DESC

	BEGIN TRAN T1

	DELETE FROM tas.Tran_ManualAttendance
	WHERE AutoID = 24818

	COMMIT TRAN T1

*/


