/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAttendanceHistory_V2
*	Description: Get attedance history records 
*
*	Date			Author		Rev. #		Comments:
*	04/08/2016		Ervin		1.0			Created
*	30/11/2016		Ervin		1.1			Added "Duration_Worked_Cumulative" in the query results
*	15/12/2016		Ervin		1.2			Added DISTINCT clause in the SELECT statement
*	16/12/2016		Ervin		1.3			Removed join to "Audit_Tran_Timesheet_Extra" table
*	12/07/2017		Ervin		1.4			Added extra logic that calculates the work duration if "Duration_Worked_Cumulative" is zero
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAttendanceHistory_V2
(   
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME,
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
			@ORDERBY					= ' ORDER BY DT DESC, dtIN DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_Timesheet a '
								--LEFT JOIN tas.Audit_Tran_Timesheet_Extra c ON a.AutoID = c.XID_AutoID '	--Rev. #1.3

	SET @CMD = 'SELECT	DISTINCT
						a.AutoID,
						a.EmpNo,
						a.BusinessUnit,
						a.IsLastRow,
						a.Processed,
						a.CorrectionCode,
						a.DT,
						a.dtIN,
						a.dtOUT,

						--Rev. #1.4
						CASE WHEN a.IsLastRow = 1 THEN a.NetMinutes
							WHEN ISNULL(a.Duration_Worked_Cumulative, 0) = 0 AND a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL THEN DATEDIFF(MINUTE, a.dtIN, a.dtOUT)
							ELSE 
								CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0 AND a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL
									THEN a.Duration_Worked_Cumulative
									ELSE 0
								END
						END AS Duration_Worked_Cumulative,

						a.Shaved_IN,
						a.Shaved_OUT, 
						a.ShiftPatCode,
						a.ShiftCode,
						a.Actual_ShiftCode,		
						a.OTType,
						a.OTStartTime,
						a.OTEndTime,							
						a.NoPayHours,
						a.AbsenceReasonCode,
						a.LeaveType,
						a.DIL_Entitlement,
						a.RemarkCode,
						a.LastUpdateUser,
						a.LastUpdateTime
				FROM tas.Tran_Timesheet a '
					--LEFT JOIN tas.Audit_Tran_Timesheet_Extra c ON a.AutoID = c.XID_AutoID '	--Rev. #1.3

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add date range filter
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY DT DESC) as RowNumber 
			FROM (' + @CMD + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)		
		+ @ORDERBY

	--Check if total records to return is greater than 200
	IF @recordCount > 100
		SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

GO 

/*	Debugging:

PARAMETERS:
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@pageNumber		INT = 1,
	@pageSize		INT = 10			

	EXEC tas.Pr_GetAttendanceHistory_V2 10001931, '15/03/2016', '15/03/2016'
		
*/


