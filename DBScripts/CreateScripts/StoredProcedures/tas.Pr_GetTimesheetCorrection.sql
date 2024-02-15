/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetCorrection
*	Description: Get the Timesheet Correction transaction history records 
*
*	Date			Author		Rev. #		Comments:
*	28/07/2016		Ervin		1.0			Created
*	24/12/2016		Ervin		1.1			Fetch the OTStartTime, OTEndTime, and OTType fields from Tran_Timesheet_Extra table
*	27/12/2016		Ervin		1.2			Added "Duration_ShiftAllowance_Evening" and "Duration_ShiftAllowance_Night" fields in the query results
*	29/12/2016		Ervin		1.3			Added "MealVoucherEligibility" in the query results
*	02/01/2017		Ervin		1.4			Refactord the code in fetching Timesheet records filtered by AutoID
*	16/02/2017		Ervin		1.5			Added link to "Tran_WorkplaceSwipe" table to get the employee's swipe in the plant readers
*	22/03/2017		Ervin		1.6			Create join to "tas.Vw_Master_ShiftPatternTitles" view
*	29/04/2017		Ervin		1.7			Added validation that checks if employee exist in the "WorkplaceSwipeExclusion" table
*	28/12/2017		Ervin		1.8			Added "Duration_Worked_Cumulative" in the returned dataset
*	10/01/2018		Ervin		1.9			Added "IsDriver", "IsLiasonOfficer", and "IsHedger" fields in the returned dataset
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetCorrection
(   
	@costCenter			VARCHAR(12) = '',
	@empNo				INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@autoID				INT = 0,
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
			@ORDERBY					= ' ORDER BY DT ASC, BusinessUnit, EmpNo',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	SELECT @minutes_MinShiftAllowance = a.Minutes_MinShiftAllowance FROM tas.System_Values a

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_Timesheet '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						LTRIM(RTRIM(c.YAALPH)) AS EmpName,
						LTRIM(RTRIM(ISNULL(d.JMDL01, ''''))) AS Position,
						a.BusinessUnit,
						LTRIM(RTRIM(e.BUname)) AS BusinessUnitName,
						a.CorrectionCode,
						LTRIM(RTRIM(b.DRDL01)) AS CorrectionDesc,
						a.DT,
						a.dtIN,
						a.dtOUT,	
						CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND ISNULL(h.IsDayShift, 0) = 0 
							AND NOT EXISTS (SELECT AutoID FROM tas.WorkplaceSwipeExclusion WHERE EmpNo = a.EmpNo AND IsActive = 1 AND a.DT >= EffectiveDate)' +		--Rev #1.7
							'THEN 1 
							ELSE 0 
						END AS RequiredToSwipeAtWorkplace,
						g.TimeInMG,
						g.TimeOutMG,
						g.TimeInWP,
						g.TimeOutWP,	
						CASE WHEN g.CorrectionType IS NULL 
							THEN ''N/A''
							ELSE 
								CASE WHEN g.IsCorrected = 1 THEN ''Yes'' ELSE ''No'' END 
						END AS IsCorrected,		
						CASE WHEN g.CorrectionType IS NULL 
							THEN ''N/A''
							ELSE 
								CASE WHEN g.IsClosed = 1 THEN ''Yes'' ELSE ''No'' END 
						END AS IsCorrectionApproved,
						CASE WHEN g.CorrectionType IS NULL 
							THEN ''''
							ELSE 
								CASE WHEN g.IsCorrected = 1
									THEN g.Remarks
									ELSE
										CASE WHEN ISNULL(g.TimeInWP, '''') = '''' AND ISNULL(g.TimeOutWP, '''') = ''''
											THEN ''Missing swipe in and out at the workplace''
											WHEN ISNULL(g.TimeInWP, '''') = '''' AND ISNULL(g.TimeOutWP, '''') <> ''''
											THEN ''Missing swipe in at the workplace''
											WHEN ISNULL(g.TimeInWP, '''') <> '''' AND ISNULL(g.TimeOutWP, '''') = ''''
											THEN ''Missing swipe out at the workplace''
											ELSE ''''
										END
								END 
						END AS Remarks,						
						a.ShiftPatCode,
						a.ShiftCode,
						a.Actual_ShiftCode,
						CASE WHEN a.Duration_ShiftAllowance_Evening >= ' + RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' AND a.Duration_ShiftAllowance_Night >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Eve/Nght''
        					 WHEN a.Duration_ShiftAllowance_Night >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Night''
        					 WHEN a.Duration_ShiftAllowance_Evening >= ' +  RTRIM(CAST(@minutes_MinShiftAllowance AS VARCHAR(10))) + ' THEN ''Evening''  	
        					 ELSE ''''
        				END AS ShiftAllowanceDesc,
						a.ShiftAllowance,
						a.Duration_ShiftAllowance_Evening,
						a.Duration_ShiftAllowance_Night,
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
						a.IsLastRow,
						a.Processed,
						a.MealVoucherEligibility,
						f.OTstartTime AS OTStartTime_TE,
						f.OTendTime AS OTEndTime_TE,
						f.OTtype AS OTType_TE,
						a.Duration_Worked_Cumulative,
						a.IsDriver,
						a.IsLiasonOfficer,
						a.IsHedger											
				FROM tas.Tran_Timesheet a
					LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.CorrectionCode) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) + ''-'' + LTRIM(RTRIM(b.DRRT)) = ''55-T0''
					INNER JOIN tas.syJDE_F060116 c ON a.EmpNo = c.YAAN8
					LEFT JOIN tas.syJDE_F08001 d on LTRIM(RTRIM(c.YAJBCD)) = LTRIM(RTRIM(d.JMJBCD))
					LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(a.BusinessUnit) = RTRIM(e.BU)
					LEFT JOIN tas.Tran_Timesheet_Extra f ON a.AutoID = f.XID_AutoID
					LEFT JOIN tas.Vw_WorkplaceSwipe g ON a.EmpNo = g.EmployeeNo AND a.DT = g.SwipeDate AND a.IsLastRow = 1
					LEFT JOIN tas.Vw_Master_ShiftPatternTitles h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 	'

	--Add Cost Center filter
	IF @costCenter IS NOT NULL	
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(BusinessUnit) = ' + RTRIM(@costCenter) 

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add date range filter
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 

	ELSE IF @startDate IS NOT NULL AND @endDate IS NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, DT, 12) = ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote 

	--Add AutoID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) 

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY DT ASC, BusinessUnit, EmpNo) as RowNumber 
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

	SELECT * FROM tas.syJDE_F060116 a
	WHERE a.YAAN8 = 10003632

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
	@costCenter			VARCHAR(12),
	@empNo				INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@autoID				INT = 0,
	@pageNumber			INT = 1,
	@pageSize			INT = 10		

	EXEC tas.Pr_GetTimesheetCorrection 
	EXEC tas.Pr_GetTimesheetCorrection '', 10003523, '16/02/2016', '15/03/2016', 0, 1, 100
	EXEC tas.Pr_GetTimesheetCorrection '', 10003632, '16/02/2016', '15/03/2016'	
	EXEC tas.Pr_GetTimesheetCorrection '7600'									--Filtered by Cost Center
	EXEC tas.Pr_GetTimesheetCorrection '', 10003366								--Filtered By Emp. No.
	EXEC tas.Pr_GetTimesheetCorrection '', 0, '28/02/2016'						--Filtered By Start Date
	EXEC tas.Pr_GetTimesheetCorrection '', 0, '16/02/2016', '15/03/2016'		--Filtered By Start Date and End Date
	EXEC tas.Pr_GetTimesheetCorrection '7600', 10003632, '12/20/2016', '15/03/2016'		
	EXEC tas.Pr_GetTimesheetCorrection '7600', 0, '16/02/2016', '15/03/2016', 1, 100	
	EXEC tas.Pr_GetTimesheetCorrection '', 0, null, null, 4942142 					--Filtered by AutoID
		

*/


