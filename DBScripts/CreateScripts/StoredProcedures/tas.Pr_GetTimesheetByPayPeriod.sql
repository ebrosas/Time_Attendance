/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetByPayPeriod
*	Description: Get the employee's attendance records by date period
*
*	Date			Author		Rev. #		Comments:
*	19/07/2016		Ervin		1.0			Created
*	10/01/2017		Ervin		1.1			Modified the Where filter clause when date duration is specified
*	06/03/2017		Ervin		1.2			Added condition to return the attendance records with corrected and approved workplace swipes
*	09/03/2017		Ervin		1.3			Added "IsExceptional" in the return output
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetByPayPeriod
(   
	@empNo				INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@withExceptionOnly	BIT = 0,
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
			@WHERE						= 'WHERE',
			@ORDERBY					= ' ORDER BY DT ASC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Get system value flags
	SELECT @minutes_MinShiftAllowance = a.Minutes_MinShiftAllowance FROM tas.System_Values a

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
							FROM tas.Tran_Timesheet '

	SET @CMD = 'SELECT	
						CASE WHEN (CONVERT(VARCHAR, LastUpdateTime, 12) > ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND DT < ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND Processed = 0)
							THEN 1
							ELSE 0
						END AS IsExceptional,
						a.AutoID,
						a.CorrectionCode,
						LTRIM(RTRIM(b.DRDL01)) AS CorrectionCodeDesc,
						a.DT,
						a.dtIN,
						a.dtOUT,
						a.EmpNo,
						a.ShiftPatCode,
						a.ShiftCode,
						a.Actual_ShiftCode,
						CASE WHEN a.Duration_ShiftAllowance_Evening >= ' + RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' AND a.Duration_ShiftAllowance_Night >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Eve/Nght''
        					 WHEN a.Duration_ShiftAllowance_Night >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Night''
        					 WHEN a.Duration_ShiftAllowance_Evening >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Evening''  	
        					 ELSE ''''
        				END AS ShiftAllowance,

						a.OTStartTime,
						a.OTEndTime,
						a.OTType,
						a.NoPayHours,
						a.AbsenceReasonCode,
						a.LeaveType,
						a.DIL_Entitlement,
						a.RemarkCode,
						a.LastUpdateUser,
						a.LastUpdateTime,
						a.IsLastRow
				FROM tas.Tran_Timesheet a
					LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.CorrectionCode) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) + ''-'' + LTRIM(RTRIM(b.DRRT)) = ''55-T0'' '

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add date range filter
	IF	@startDate IS NOT NULL 
		AND @endDate IS NOT NULL
	BEGIN
    
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND (' ELSE ' ' END + 'CONVERT(VARCHAR, DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote  + 
			' OR (CONVERT(VARCHAR, LastUpdateTime, 12) > ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND DT < ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND Processed = 0)) '
	END 

	IF @withExceptionOnly = 1
	BEGIN
    
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (ISNULL(CorrectionCode, '''') <> '''' OR ISNULL(NoPayHours, 0) > 0 OR ISNULL(AbsenceReasonCode, '''') <> '''' OR ISNULL(LeaveType, '''') <> '''' OR ISNULL(DIL_Entitlement, '''') <> '''' OR ISNULL(RemarkCode, '''') <> '''' ' + -- OR (CONVERT(VARCHAR, LastUpdateTime, 12) > ' + @chrQuote + CONVERT(VARCHAR, @endDate, 12) + @chrQuote  + ' AND Processed = 0)) ' 
			' OR (CONVERT(VARCHAR, LastUpdateTime, 12) > ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND DT < ' + @chrQuote + CONVERT(VARCHAR, @startDate, 12) + @chrQuote + ' AND Processed = 0)) '
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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY DT ASC) as RowNumber 
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

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all leave type codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'VC'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve Leave Absence Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'WC'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all Timesheet Correction Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-T0'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY DRDL01

	--Retrieve all Leave Types
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-LV'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all Absent Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '00-TD'
	ORDER BY LTRIM(RTRIM(DRKY))

PARAMETERS:
	@empNo				INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@withExceptionOnly	BIT = 0,
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetTimesheetByPayPeriod 
	EXEC tas.Pr_GetTimesheetByPayPeriod 10003631											--By Employee No.
	EXEC tas.Pr_GetTimesheetByPayPeriod 10003745, '16/01/2016', '15/02/2016'				--By Date Range
	EXEC tas.Pr_GetTimesheetByPayPeriod 10003441, '12/16/2016', '01/15/2017', 0, 1, 50		--By Employee No. and Date Range
	EXEC tas.Pr_GetTimesheetByPayPeriod 10006040, '02/16/2017', '03/15/2017', 1				--By Employee No. and Date Range and with exceptions
	EXEC tas.Pr_GetTimesheetByPayPeriod 10006040, '01/16/2017', '02/15/2017', 1				--By Employee No. and Date Range and with exceptions

*/


