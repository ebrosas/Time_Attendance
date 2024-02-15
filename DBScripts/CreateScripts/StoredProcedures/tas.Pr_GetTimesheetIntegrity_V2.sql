/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetIntegrity_V2
*	Description: This stored procedure is used to fetch data for the "Timesheet Integrity by Correction Code" form 
*
*	Date			Author		Rev. #		Comments:
*	18/07/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetIntegrity_V2
(   
	@actionCode		VARCHAR(10),
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
			@WHERE						= 'WHERE',
			@ORDERBY					= ' ORDER BY DT DESC',
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
								INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo '

	SET @CMD = 'SELECT	a.AutoID,
						a.CorrectionCode,	
						LTRIM(RTRIM(d.DRDL01)) AS CorrectionDesc,
						a.DT,
						a.dtIN,
						a.dtOUT,
						a.EmpNo,
						b.EmpName,
						a.BusinessUnit,
						c.BusinessUnitName,
						a.ShiftPatCode,
						a.ShiftCode,
						a.Actual_ShiftCode,
						a.ShiftAllowance,
						a.Duration_ShiftAllowance_Evening,
						a.Duration_ShiftAllowance_Night,
						a.OTType,
						a.OTStartTime,
						a.OTEndTime,
						a.NoPayHours,
						a.AbsenceReasonCode,
						a.LeaveType,
						a.DIL_Entitlement,
						a.RemarkCode,
						a.LastUpdateUser,
						a.LastUpdateTime,
						a.Processed
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
					LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.CorrectionCode) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) + ''-'' + LTRIM(RTRIM(d.DRRT)) = ''55-T0'' '
	
	--Add Employee No. filter				
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add Cost Center filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.BusinessUnit) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    
		
	--Add date filter	
	IF (@startDate IS NOT NULL AND @endDate IS NOT NULL)
	BEGIN
		
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 
	END

	--Add other filter based on @actionCode
	IF @actionCode = 'TSOPT1'		--Add OT, but there is no OT
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND (UPPER(RTRIM(a.CorrectionCode)) LIKE ''AO%'' OR UPPER(RTRIM(a.CorrectionCode)) IN (''ACS'', ''MA'')) ' +
			' AND (a.OTStartTime IS NULL OR a.OTEndTime IS NULL) '
	END 

	ELSE IF @actionCode = 'TSOPT2'	--Add NoPayHours, but there are no NoPayHour
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''AN%'' ' +
			' AND ISNULL(a.NoPayHours, 0) = 0 '
	END 

	ELSE IF @actionCode = 'TSOPT3'	--Add Shift Allowance, but there is no allowance
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''AS%'' ' +
			' AND ISNULL(a.ShiftAllowance, 0) = 0 '
	END 

	ELSE IF @actionCode = 'TSOPT4'	--Mark Absent, but not absent
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''MA%'' ' +
			' AND ISNULL(a.RemarkCode, '''') <> ''A'' '
	END 

	ELSE IF @actionCode = 'TSOPT5'		--Mark DIL, but there is no DIL
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''MD%'' ' +
			' AND ISNULL(a.DIL_Entitlement, '''') = '''' '
	END 

	ELSE IF @actionCode = 'TSOPT6'		--Remove OT, but still there is OT
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''RO%'' ' +
			' AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL) '
	END 

	ELSE IF @actionCode = 'TSOPT7'		--Remove NoPayHour, but still there is NoPayHour
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''RN%'' ' +
			' AND ISNULL(a.NoPayHours, 0) > 0 '
	END 

	ELSE IF @actionCode = 'TSOPT8'		--Remove Shift Allowances, but it is not removed
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''RS%'' ' +
			' AND a.ShiftAllowance = 1 '
	END 

	ELSE IF @actionCode = 'TSOPT9'		--Remove Absence, but still there is Absence
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''RA%'' ' +
			' AND RTRIM(a.RemarkCode) = ''A'' '
	END 

	ELSE IF @actionCode = 'TSOPT10'		--Remove DIL, but still there is DIL
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' ISNULL(a.CorrectionCode, '''') <> '''' ' +
			' AND UPPER(RTRIM(a.CorrectionCode)) LIKE ''RD%'' ' +
			' AND ISNULL(a.DIL_Entitlement, '''') <> '''' '
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
		+ CASE WHEN LEN(@ORDERBY) > 0 THEN @ORDERBY ELSE '' END 

	--Check if total records to return is greater than 200
	IF @recordCount > 100
		SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

GO 

/*	Debugging:

PARAMETERS:
	@actionCode		VARCHAR(12),
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= '',
	@pageNumber		INT = 1,
	@pageSize		INT = 10			

	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT1', null, null, 0, '', 0, 10		--Add OT, but there is no OT
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT2', '', '', 0, ''			--Add NoPayHours, but there are no NoPayHour
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT3', '', '', 0, ''			--Add Shift Allowance, but there is no allowance
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT4', '', '', 0, ''			--Mark Absent, but not absent
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT5', '', '', 0, ''			--Mark DIL, but there is no DIL
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT6', '', '', 0, ''			--Remove OT, but still there is OT
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT7', '', '', 0, ''			--Remove NoPayHour, but still there is NoPayHour
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT8', '', '', 0, ''			--Remove Shift Allowances, but it is not removed
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT9', '', '', 0, ''			--Remove Absence, but still there is Absence
	EXEC tas.Pr_GetTimesheetIntegrity_V2 'TSOPT10', '', '', 0, ''			--Remove DIL, but still there is DIL

*/


