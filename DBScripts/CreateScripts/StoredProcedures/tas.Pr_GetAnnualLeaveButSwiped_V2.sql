/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAnnualLeaveButSwiped_V2
*	Description: Get the list of employees who came to work while on leave
*
*	Date			Author		Rev. #		Comments:
*	18/07/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAnnualLeaveButSwiped_V2
(   
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= '',
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
			--@WHERE						= 'WHERE',
			@WHERE						= 'WHERE a.EmpNo > 10000000
											AND ISNULL(a.LeaveType, '''') IN (''AL'')
											AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL)
											AND ISNUMERIC(b.PayStatus) = 1 ',
			@ORDERBY					= ' ORDER BY DT DESC, CostCenter, EmpNo',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_Timesheet a
								INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
								LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						b.EmpName,
						a.BusinessUnit AS CostCenter,
						RTRIM(c.BusinessUnitName) AS CostCenterName,
						a.DT,
						a.dtIN,
						a.dtOUT,			
						CASE WHEN DATEDIFF(n, a.dtIN, a.dtOUT) < 0
							THEN DATEDIFF(n, a.dtIN, a.dtOUT) + (24 * 60)
							ELSE DATEDIFF(n, a.dtIN, a.dtOUT)
						END AS Duration,
						CASE WHEN 
						(
							SELECT COUNT(*) 
							FROM tas.Tran_Timesheet 
							WHERE EmpNo = a.EmpNo 
								AND DT = a.DT
								AND dtIN IS NOT NULL 
								AND dtOUT IS NOT NULL	
						) > 1 THEN 1 ELSE 0 END AS HasMultipleSwipe,
						a.ShiftPatCode,
						a.ShiftCode,
						a.Actual_ShiftCode,
						a.LeaveType,
						LTRIM(RTRIM(d.DRDL01)) AS LeaveTypeDesc
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)
					LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.LeaveType) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) = ''58'' AND LTRIM(RTRIM(d.DRRT)) = ''VC'' '

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.BusinessUnit) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    
		
	--Add @startDate and @endDate filter	
	IF (@startDate IS NOT NULL AND @endDate IS NOT NULL)
	BEGIN
		
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 
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
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= '',
	@pageNumber		INT = 1,
	@pageSize		INT = 10		

	EXEC tas.Pr_GetAnnualLeaveButSwiped_V2									--No filter
	EXEC tas.Pr_GetAnnualLeaveButSwiped_V2 '01/01/2016', '31/01/2016'		--By @startDate and @endDate
	EXEC tas.Pr_GetAnnualLeaveButSwiped_V2 '', '', 10003589					--By Employee No.
	EXEC tas.Pr_GetAnnualLeaveButSwiped_V2 '', '', 0, '7600'				--By Cost Center

*/


