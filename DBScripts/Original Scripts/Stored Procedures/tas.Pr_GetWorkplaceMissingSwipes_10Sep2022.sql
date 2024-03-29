USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetWorkplaceMissingSwipes]    Script Date: 10/09/2022 14:58:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetWorkplaceMissingSwipes
*	Description: This stored procedure is used to fetch the missing swipes at the workplace
*
*	Date			Author		Rev.#		Comments:
*	16/07/2015		Ervin		1.0			Created
*	22/07/2015		Ervin		1.1			Added @displayType parameter
*	05/08/2015		Ervin		1.2			Added the following fields to be return in the query: Superintendent, SuperintendentEmail, CostCenterManager, CostCenterManagerEmail, ServiceProviderEmpNo, ServiceProviderEmail
*	09/08/2015		Ervin		1.3			Remove the filter condition for "IsProcessedByTimesheet" and "RemarkCode"
*	10/08/2015		Ervin		1.4			Added the following fields to be returned in the query: Shaved_IN, Shaved_OUT, ShiftTiming
*	12/08/2015		Ervin		1.5			Added "ShiftDetail" field. Added condition that checks if SwipeDate is less than or equals to "DT_SwipeLastProcessed" from tas.System_Values table
*	20/08/2015		Ervin		1.6			Refactored the where filter conditions to link data in "Tran_TempSwipeData" table
*	01/09/2015		Ervin		1.7			Refactored the code in fetching the workplace administrators
*	01/09/2015		Ervin		1.8			Refactored the condition in setting the value of "IsTimesheetExecuted" flag. Check if log record exist in "SyncWorkplaceSwipeToTimesheetLog" table
*	05/09/2015		Ervin		1.9			Added load condition for "SWPVALSWIP - Show valid swipes" and "SWPVALMISS - Show valid and missing swipes"
*	11/09/2015		Ervin		2.0			Refactored the logic in identifying the value of "IsTimesheetExecuted" field
*	13/09/2015		Ervin		2.1			Use the a.CostCenter instead of d.BusinessUnit in the join tables
*	17/09/2015		Ervin		2.2			Added @userEmpNo parameter
*	26/09/2015		Ervin		2.3			Added filter field condition "IsSubmittedForApproval". Added "Show swipe corrections for approval" display option
*	01/10/2015		Ervin		2.4			Modified the filter condition for returning the valid swipes. Check if IsCorrected is null
*	05/10/2015		Ervin		2.5			Added link to WorkflowHistory table in "Show swipe corrections for approval" load type
*	10/10/2015		Ervin		2.6			Refactored logic in @displayType = 'SWPATENHIS'	
*	12/10/2015		Ervin		2.7			Added condition to return missing swipes only when records exist in "SyncWorkplaceSwipeToTimesheetLog" table
*	18/10/2015		Ervin		2.8			Added "ShiftSpan" in the query results
*	10/12/2015		Ervin		2.9			Modified the join condition to "Tran_TempSwipeData" table
*	13/12/2015		Ervin		3.0			Added filter condition to return records greater than or equal to the Effectivity Date which is the date when HR sent the circular on 09-Dec-2015
*	24/12/2015		Ervin		3.1			Return records which have been corrected, submitted and already approved in @displayType = 'SWPVALSWIP'	(Show valid swipes)
*	24/12/2015		Ervin		3.2			Refactored the logic for @displayType = 'SWPWITHCHK'
*	28/12/2015		Ervin		3.3			Refactored the logic in calculating the value for "NetMinutesMG" and "NetMinutesWP" fields
*	21/01/2016		Ervin		3.4			Get the value of TimeInMG and TimeOutMG from dtIN_Old and dtOUT_Old
*	26/01/2016		Ervin		3.5			Added condition that set the Cost Center Manager Emp. No. to zero if equal to 10001281 and 10005002
*	05/02/2016		Ervin		3.6			Return the missing swipes with Correction Code that exists in the log table
*	08/02/2016		Ervin		3.7			Added filter condition if an employee is excluded in the workplace swipe system as setup in the "WorkplaceSwipeExclusion" table
*	15/02/2016		Ervin		3.8			Commented condition that filters swipe approval record by the "SubmittedByEmpNo" field
*	15/02/2016		Ervin		3.9			Added filter condition to return unprocessed or unpaid records only 
*	18/02/2016		Ervin		4.0			Added filter condition that checks the value of "IsMainGateSwipeRestored" field
*	29/02/2016		Ervin		4.1			Commented all occurences of @CONST_EFFECTIVITY_DATE
*	16/03/2016		Ervin		4.2			Modified logic in @displayType = 'SWPVALSWIP'. Return records wherein Timesheet has value in the "CorrectionCode" field 
*	14/08/2016		Ervin		4.3			Removed filter condition that hides unprocessed or unpaid records to be reutrned in the query
*	05/03/2017		Ervin		4.4			Implemented Rev. #3.6 in  @displayType = 'SWPFORAPV'
*	05/22/2018		Ervin		4.5			Added filter condition "a.Remarks <> 'Day-off'" in the following display types: SWPALL, SWPNOCHK, SWPWITHCHK
*	11/07/2018		Ervin		4.6			Convert a.Remarks to empty string if it is null
*	18/07/2018		Ervin		4.7			Commented the condition a.Remarks <> 'Day-off' for @displayType = 'SWPALL'
*	03/12/2020		Ervin		4.8			Added filter clause "WHERE IsActive = 1" when joining to "WorkplaceReaderSetting" table
*	03/05/2022		Ervin		4.9			Commented code that checks if CorrectionCode is null in "Show valid and missing swipes" option
*	11/08/2022		Ervin		5.0			Implemented logic for sending separate notifications for employees who use the Admin Bldg. readers
*	14/08/2022		Ervin		5.1			Added filter condition to return day-shift workers for Mustafa and shift workers for clerks
*	22/08/2022		Ervin		5.2			Disabled checking for l.IsAdminBldgEnabled = 0. Added default Swipe Correction Admin logic
*	04/09/2022		Ervin		5.3			Modified the logic in fetching data for the Missing Workplace Swipe Report for plan workers
*****************************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetWorkplaceMissingSwipes]
(
	@displayType		varchar(10),	
	@empNo				int = 0,	
	@costCenter			varchar(12)	= '',
	@startDate			datetime = null,
	@endDate			datetime = null,
	@userEmpNo			int = 0,
	@statusCode			varchar(10) = ''	/*	Status Code Options:
												STATALL,        // All Status
												STATOPEN,	    // Open
												STATAPPRVE,	    // Approved
												STATREJECT,	    // Rejected
												STATCANCEL	    // Cancelled
											*/
)

--NOTES:
/*	Display Type Options

	SWPALL: Show missing swipes (all)
	SWPNOCHK: Show missing swipes (no correction)
	SWPWITHCHK: Show missing swipes (with correction)
	SWPVALSWIP:	Show valid swipes
	SWPVALMISS:	Show valid and missing swipes
	SWIPECCC: Return the cost centers with missing swipes
	SWPATENHIS: Employee Attendance History Report

*/
AS
BEGIN

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	DECLARE	@CONST_EFFECTIVITY_DATE		DATETIME,
			@isApplyToTimesheet			BIT,
			@userCostCenter				VARCHAR(12) = '',
			@filterDayShift				BIT = 0,
			@DefaultAdminEmpNo			INT = 0,
			@DefaultAdminEmail			VARCHAR(50) = ''

	SELECT	@CONST_EFFECTIVITY_DATE		= '12/09/2015',		--(Note: This is the date when HR sent the circular to everyone)
			@isApplyToTimesheet = 0

	IF ISNULL(@displayType, '') = ''
		SET @displayType = 'SWPALL'

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL
		
	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL		

	IF ISNULL(@userEmpNo, 0) = 0
		SET @userEmpNo = NULL
		
	IF ISNULL(@statusCode, '') = '' 
		SET @statusCode = NULL	
	ELSE
	BEGIN

		IF @statusCode = 'STATALL'
			SET @statusCode = NULL
		ELSE IF @statusCode = 'STATOPEN'
			SET @statusCode = 'Open'
		ELSE IF @statusCode = 'STATAPPRVE'
			SET @statusCode = 'Approved'
		ELSE IF @statusCode = 'STATREJECT'
			SET @statusCode = 'Rejected'
		ELSE IF @statusCode = 'STATCANCEL'
			SET @statusCode = 'Cancelled'
	END
	
	SELECT @isApplyToTimesheet = ISNULL(SyncWorkplaceToTimesheet, 0)
	FROM tas.WorkplaceTimesheetSetting WITH (NOLOCK)
	WHERE IsActive = 1

	--Get the cost center of the current user
	IF ISNULL(@userEmpNo, 0) > 0
	BEGIN
    
		SELECT @userCostCenter = RTRIM(a.BusinessUnit)
		FROM tas.Master_Employee_JDE a WITH (NOLOCK)
		WHERE a.EmpNo = @userEmpNo
	END 

	IF @userCostCenter = '7500'
		SET @filterDayShift = 1

	--Check if current user is a System Administrator or the search is filtered by Emp. No. If so, then disable the filter by IsDayShift
	IF EXISTS
    (
		SELECT 1 FROM tas.fnGetActionMember_WithSubstitute_V2('READRADMIN', 'ALL', 0) a
		WHERE a.EmpNo = @userEmpNo
	) 
	OR @empNo > 0
	BEGIN
    
		SET @filterDayShift = NULL 
	END 

	--Get the default person who will server as the Swipe Correction Admin for cost centers without defined administrators (Rev. #5.2)
	SELECT	@DefaultAdminEmpNo = a.EmpNo,
			@DefaultAdminEmail = a.EmpEmail 
	FROM tas.fnGetActionMember_WithSubstitute_V2('SWIPEHRADM', 'ALL', 0) a

	IF @displayType = 'SWPALL'					--Show missing swipes (all)
	BEGIN
		
		--Get missing swipes from plant workplace readers
		SELECT DISTINCT 
			SwipeID,
			a.EmpNo,
			a.EmpName,
			b.GradeCode, 
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.Position,
			a.IsDayShift,
			a.ShiftPatCode,
			a.ShiftCode,
			ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
			a.ShiftPointer,
			CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
				THEN 'Day-off'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
				THEN 'Morning shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
				THEN 'Evening Shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
				THEN 'Night shift'
				ELSE ''
			END AS ShiftDetail,		--Rev. #1.5
			a.SwipeDate,
			a.TimeInMG,
			a.TimeOutMG,
			a.TimeInWP,
			a.TimeOutWP,
			a.DurationRequired,

			--Start of Rev.# 3.3
			CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
				ELSE NULL
			END AS NetMinutesMG,
			CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
				ELSE NULL
			END AS NetMinutesWP,
			--End of Rev.# 3.3

			CASE WHEN a.IsCorrected = 1
				THEN a.Remarks
				ELSE
					CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks,
			a.IsProcessedByTimesheet,
			a.IsCorrected,	
			a.CorrectionType,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateEmpName,
			d.LeaveType,
			d.RemarkCode,
			d.CorrectionCode,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
			0 AS ServiceProviderEmpNo,
			'' AS ServiceProviderEmail,

			ISNULL(d.OTType, g.OTType) AS OTType,
			ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
			ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
			ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
			ISNULL(g.Approved, 0) AS OTApproved,

			d.NoPayHours,
			d.Shaved_IN,
			d.Shaved_OUT,			
			CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
			d.AutoID,				
			CASE WHEN 
				(
					d.AutoID IS NOT NULL 
					AND NOT
					(
						--Note: Exclude morning and evening shifts if dtOUT is not yet defined
						(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
						AND d.dtOUT IS NULL
						AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
					)
					AND j.LogID IS NOT NULL
				)
				OR j.LogID IS NULL
				THEN 1 
				ELSE 0 
			END AS IsTimesheetExecuted,
			d.dtIN,
			d.dtOUT,
			d.Duration_Worked_Cumulative,
			d.NetMinutes,
			a.StatusID,
			a.StatusCode,
			a.StatusDesc,
			a.StatusHandlingCode,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ServiceProviderTypeCode,
			a.DistListCode,
			a.IsClosed,
			a.ClosedDate,
			a.IsSubmittedForApproval,
			a.SubmittedDate,
			a.SubmittedByEmpNo,
			@isApplyToTimesheet AS IsApplyToTimesheet,
			0 AS IsValidSwipe,
			d.ShiftSpan
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d
			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g
			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)			
			LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))	--Rev. #2.9
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) m	
		WHERE 
			(
				(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
				OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
			)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END
			) = 1			
			AND 
			(
				ISNULL(a.IsSubmittedForApproval, 0) = 0		--Not yet submitted for approval
				OR (a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '110')	--Rejected by approver		
			)
			AND 
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	--Rev. #4.0
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 0 AND l.IsSyncTimesheet = 1)
			AND 
			(
				(m.IsDayShift = 1 AND @filterDayShift = 1)
				OR (m.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)

	UNION
    
		--Get missing workplace swipes from Admin Bldg. readers
		SELECT	DISTINCT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		--Rev. #1.5
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,

				--Start of Rev.# 3.3
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				--End of Rev.# 3.3

				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				ISNULL(g.Approved, 0) AS OTApproved,

				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,			
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,				
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,
				d.dtIN,
				d.dtOUT,
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				0 AS IsValidSwipe,
				d.ShiftSpan
			FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				
				--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT 
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
							AND DT = a.SwipeDate
				) d

				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				
				--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)			
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
					WHERE IsActive = 1 
						AND RTRIM(CostCenter) = RTRIM(a.CostCenter)
				) m
				OUTER APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(
					(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
					OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
				)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND 
				(
					(
						(
							ISNULL(d.CorrectionCode, '') = ''
							OR 
							(
								ISNULL(d.CorrectionCode, '') <> '' 
								AND 
								(d.dtIN IS NULL OR d.dtOUT IS NULL)
								AND
								j.LogID IS NOT NULL
							) 
						)
						AND d.IsLastRow = 1
					)
					OR d.AutoID IS NULL
				)
				AND a.SwipeDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Exlude records when Timesheet is not yet processed
							AND 
				(
					ISNULL(a.IsSubmittedForApproval, 0) = 0		--Not yet submitted for approval
					OR (a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '110')	--Rejected by approver		
				)
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 /*AND l.IsSyncTimesheet = 1*/)
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
				AND 
				(
					(n.IsDayShift = 1 AND @filterDayShift = 1)
					OR (n.IsDayShift = 0 AND @filterDayShift = 0)
					OR @filterDayShift IS NULL 
				)
		ORDER BY SwipeDate DESC, CostCenter, EmpNo
	END

	ELSE IF @displayType = 'SWPNOCHK'			--Show missing swipes (no correction)
	BEGIN
		
		SELECT  DISTINCT
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		--Rev. #1.5
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,								
				a.DurationRequired,

				--Start of Rev.# 3.3
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				--End of Rev.# 3.3

				CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
					THEN 'Missing swipe in and out at the workplace'
					WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
					THEN 'Missing swipe in at the workplace'
					WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
					THEN 'Missing swipe out at the workplace'
					ELSE ''
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				ISNULL(g.Approved, 0) AS OTApproved,

				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					--OR j.LogID IS NOT NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,		
				d.dtIN,
				d.dtOUT,
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				d.ShiftSpan
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))	--Rev. #2.9
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) l	
		WHERE 
			(
				(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
			)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0

			--Start of Rev. #3.6
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			--End of Rev. #3.6

			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					--OR j.LogID IS NOT NULL
					THEN 1 
					ELSE 0 
				END
			) = 1		
			AND ISNULL(a.IsSubmittedForApproval, 0) = 0	--Return records not yet submitted for approval			
			--AND a.SwipeDate >= @CONST_EFFECTIVITY_DATE	--Rev. #3.0						
			
			--Start of Rev. #3.7 
			AND	
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			--End of Rev. #3.7			

			--Start of Rev. #3.9
			--AND	--Rev. #4.3  
			--(
			--	(ISNULL(d.Processed, 0) = 0 AND d.AutoID IS NOT NULL) 
			--	OR d.AutoID IS NULL
			--)
			--End of Rev. #3.9

			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	--Rev. #4.0
			AND ISNULL(a.Remarks, '') <> 'Day-off'	--Rev. #4.6
			AND 
			(
				(l.IsDayShift = 1 AND @filterDayShift = 1)
				OR (l.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)
		ORDER BY SwipeDate DESC, CostCenter, EmpNo
	END

	ELSE IF @displayType = 'SWPWITHCHK'			--Show missing swipes (with correction)
	BEGIN
		
		--Get missing swipes from plant workplace readers
		SELECT DISTINCT 
			SwipeID,
			a.EmpNo,
			a.EmpName,
			b.GradeCode, 
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.Position,
			a.IsDayShift,
			a.ShiftPatCode,
			a.ShiftCode,
			ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
			a.ShiftPointer,
			CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
				THEN 'Day-off'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
				THEN 'Morning shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
				THEN 'Evening Shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
				THEN 'Night shift'
				ELSE ''
			END AS ShiftDetail,		
			a.SwipeDate,
			a.TimeInMG,
			a.TimeOutMG,
			a.TimeInWP,
			a.TimeOutWP,
			a.DurationRequired,
			CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
				ELSE NULL
			END AS NetMinutesMG,
			CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
				ELSE NULL
			END AS NetMinutesWP,
			CASE WHEN a.IsCorrected = 1
				THEN a.Remarks
				ELSE
					CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks,
			a.IsProcessedByTimesheet,
			a.IsCorrected,	
			a.CorrectionType,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateEmpName,
			d.LeaveType,
			d.RemarkCode,
			d.CorrectionCode,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
			0 AS ServiceProviderEmpNo,
			'' AS ServiceProviderEmail,
			ISNULL(d.OTType, g.OTType) AS OTType,
			ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
			ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
			ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
			ISNULL(g.Approved, 0) AS OTApproved,
			d.NoPayHours,
			d.Shaved_IN,
			d.Shaved_OUT,			
			CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
			d.AutoID,				
			CASE WHEN 
				(
					d.AutoID IS NOT NULL 
					AND NOT
					(
						--Note: Exclude morning and evening shifts if dtOUT is not yet defined
						(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
						AND d.dtOUT IS NULL
						AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
					)
					AND j.LogID IS NOT NULL
				)
				OR j.LogID IS NULL
				THEN 1 
				ELSE 0 
			END AS IsTimesheetExecuted,
			d.dtIN,
			d.dtOUT,
			d.Duration_Worked_Cumulative,
			d.NetMinutes,
			a.StatusID,
			a.StatusCode,
			a.StatusDesc,
			a.StatusHandlingCode,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ServiceProviderTypeCode,
			a.DistListCode,
			a.IsClosed,
			a.ClosedDate,
			a.IsSubmittedForApproval,
			a.SubmittedDate,
			a.SubmittedByEmpNo,
			@isApplyToTimesheet AS IsApplyToTimesheet,
			0 AS IsValidSwipe,
			d.ShiftSpan
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			
			--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT 
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d

			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			
			--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g

			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)			
			LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))	--Rev. #2.9
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) m	
		WHERE IsCorrected = 1 
			AND ISNULL(CorrectionType, 0) > 0
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END
			) = 1			
			AND 
			(
				ISNULL(a.IsSubmittedForApproval, 0) = 0		--Not yet submitted for approval
				OR (a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '110')	--Rejected by approver		
			)
			AND 
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND ISNULL(a.Remarks, '') <> 'Day-off'	
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 0 AND l.IsSyncTimesheet = 1)
			AND 
			(
				(m.IsDayShift = 1 AND @filterDayShift = 1)
				OR (m.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)

		UNION
    
		--Get missing swipes from Admin Bldg. readers
		SELECT DISTINCT 
			SwipeID,
			a.EmpNo,
			a.EmpName,
			b.GradeCode, 
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.Position,
			a.IsDayShift,
			a.ShiftPatCode,
			a.ShiftCode,
			ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
			a.ShiftPointer,
			CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
				THEN 'Day-off'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
				THEN 'Morning shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
				THEN 'Evening Shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
				THEN 'Night shift'
				ELSE ''
			END AS ShiftDetail,		--Rev. #1.5
			a.SwipeDate,
			a.TimeInMG,
			a.TimeOutMG,
			a.TimeInWP,
			a.TimeOutWP,
			a.DurationRequired,
			CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
				ELSE NULL
			END AS NetMinutesMG,
			CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
				ELSE NULL
			END AS NetMinutesWP,
			CASE WHEN a.IsCorrected = 1
				THEN a.Remarks
				ELSE
					CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks,
			a.IsProcessedByTimesheet,
			a.IsCorrected,	
			a.CorrectionType,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateEmpName,
			d.LeaveType,
			d.RemarkCode,
			d.CorrectionCode,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
			0 AS ServiceProviderEmpNo,
			'' AS ServiceProviderEmail,
			ISNULL(d.OTType, g.OTType) AS OTType,
			ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
			ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
			ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
			ISNULL(g.Approved, 0) AS OTApproved,
			d.NoPayHours,
			d.Shaved_IN,
			d.Shaved_OUT,			
			CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
			d.AutoID,				
			CASE WHEN 
				(
					d.AutoID IS NOT NULL 
					AND NOT
					(
						--Note: Exclude morning and evening shifts if dtOUT is not yet defined
						(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
						AND d.dtOUT IS NULL
						AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
					)
					AND j.LogID IS NOT NULL
				)
				OR j.LogID IS NULL
				THEN 1 
				ELSE 0 
			END AS IsTimesheetExecuted,
			d.dtIN,
			d.dtOUT,
			d.Duration_Worked_Cumulative,
			d.NetMinutes,
			a.StatusID,
			a.StatusCode,
			a.StatusDesc,
			a.StatusHandlingCode,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ServiceProviderTypeCode,
			a.DistListCode,
			a.IsClosed,
			a.ClosedDate,
			a.IsSubmittedForApproval,
			a.SubmittedDate,
			a.SubmittedByEmpNo,
			@isApplyToTimesheet AS IsApplyToTimesheet,
			0 AS IsValidSwipe,
			d.ShiftSpan
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			
			--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT 
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d

			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			
			--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g

			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)			
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			CROSS APPLY
			(
				SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
				WHERE IsActive = 1 
					AND RTRIM(CostCenter) = RTRIM(a.CostCenter)
			) m
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) n
		WHERE IsCorrected = 1 
			AND ISNULL(CorrectionType, 0) > 0
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND a.SwipeDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Exlude records when Timesheet is not yet processed
			AND 
			(
				ISNULL(a.IsSubmittedForApproval, 0) = 0		--Not yet submitted for approval
				OR (a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '110')	--Rejected by approver		
			)
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	--Rev. #4.0
			AND ISNULL(a.Remarks, '') <> 'Day-off'	--Rev. #4.6
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 AND l.IsSyncTimesheet = 1)    
			AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
			AND 
			(
				(n.IsDayShift = 1 AND @filterDayShift = 1)
				OR (n.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)
		ORDER BY SwipeDate DESC, CostCenter, EmpNo
	END

	ELSE IF @displayType = 'SWPVALSWIP'			--Show valid swipes
	BEGIN
		
		--Get valid swipes from plant workplace readers
		SELECT DISTINCT 
			SwipeID,
			a.EmpNo,
			a.EmpName,
			b.GradeCode, 
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.Position,
			a.IsDayShift,
			a.ShiftPatCode,
			a.ShiftCode,
			ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
			a.ShiftPointer,
			CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
				THEN 'Day-off'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
				THEN 'Morning shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
				THEN 'Evening Shift'
				WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
				THEN 'Night shift'
				ELSE ''
			END AS ShiftDetail,		
			a.SwipeDate,
			CASE WHEN j.dtIN_Old IS NOT NULL 
				THEN j.dtIN_Old
				ELSE a.TimeInMG
			END AS TimeInMG,
			CASE WHEN j.dtOUT_Old IS NOT NULL
				THEN j.dtOUT_Old
				ELSE a.TimeOutMG
			END AS TimeOutMG,
			a.TimeInWP,
			a.TimeOutWP,
			a.DurationRequired,
			CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
				THEN DATEDIFF
					(
						mi, 
						CASE WHEN j.dtIN_Old IS NOT NULL 
							THEN j.dtIN_Old
							ELSE a.TimeInMG
						END, 
						CASE WHEN j.dtOUT_Old IS NOT NULL
							THEN j.dtOUT_Old
							ELSE a.TimeOutMG
						END
					)
				ELSE NULL
			END AS NetMinutesMG,
			CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
				THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
				ELSE NULL
			END AS NetMinutesWP,
			CASE WHEN a.IsCorrected = 1
				THEN a.Remarks
				ELSE
					CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks,
			a.IsProcessedByTimesheet,
			a.IsCorrected,	
			a.CorrectionType,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateEmpName,
			d.LeaveType,
			d.RemarkCode,
			d.CorrectionCode,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
			0 AS ServiceProviderEmpNo,
			'' AS ServiceProviderEmail,

			ISNULL(d.OTType, g.OTType) AS OTType,
			ISNULL(g.Approved, 0) AS OTApproved,
			CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.OTStartTime_New
				ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
			END AS OTStartTime,
			CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
				THEN j.OTEndTime_New
				ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
			END AS OTEndTime,
			DATEDIFF
			(
				n,
				CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.OTStartTime_New
					ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
				END,
				CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
					THEN j.OTEndTime_New
					ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
				END
			) AS OTDuration,

			CASE WHEN j.NoPayHours_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.NoPayHours_New
				ELSE d.NoPayHours
			END AS NoPayHours,

			CASE WHEN j.ShavedIn_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.ShavedIn_New
				ELSE d.Shaved_IN
			END AS Shaved_IN,
			CASE WHEN j.ShavedOut_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.ShavedOut_New
				ELSE d.Shaved_OUT
			END AS Shaved_OUT,

			CASE WHEN j.DurationWorkedCumulative_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.DurationWorkedCumulative_New
				ELSE d.Duration_Worked_Cumulative
			END AS Duration_Worked_Cumulative,

			CASE WHEN j.NetMinutes_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
				THEN j.NetMinutes_New
				ELSE d.NetMinutes
			END AS NetMinutes,
			d.dtIN,
			d.dtOUT,
			CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
			d.AutoID,	
			CASE WHEN 
				(
					d.AutoID IS NOT NULL 
					AND NOT
					(
						--Note: Exclude morning and evening shifts if dtOUT is not yet defined
						(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
						AND d.dtOUT IS NULL
						AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
					)
					AND j.LogID IS NOT NULL
				)
				OR j.LogID IS NULL
				THEN 1 
				ELSE 0 
			END	AS IsTimesheetExecuted, 
			a.StatusID,
			a.StatusCode,
			a.StatusDesc,
			a.StatusHandlingCode,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ServiceProviderTypeCode,
			a.DistListCode,
			a.IsClosed,
			a.ClosedDate,
			a.IsSubmittedForApproval,
			a.SubmittedDate,
			a.SubmittedByEmpNo,
			@isApplyToTimesheet AS IsApplyToTimesheet,
			1 AS IsValidSwipe,
			d.ShiftSpan
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			
			--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1 
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d

			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			
			--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g

			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) m
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
			AND ISNULL(a.IsDayShift, 0) = 0			
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END
			) = 1		
			AND 
			(
				(
					--Return records not yet submitted for approval	
					ISNULL(a.IsSubmittedForApproval, 0) = 0	AND a.IsCorrected IS NULL
				)
				OR
				(
					--Return records which have been corrected, submitted and already approved
					a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '123'	
				)
			)
			AND	
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 0 AND l.IsSyncTimesheet = 1)
			AND 
			(
				(m.IsDayShift = 1 AND @filterDayShift = 1)
				OR (m.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)

		UNION

		--Get valid swipes from Admin Bldg. readers
		SELECT DISTINCT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				CASE WHEN j.dtIN_Old IS NOT NULL 
					THEN j.dtIN_Old
					ELSE a.TimeInMG
				END AS TimeInMG,
				CASE WHEN j.dtOUT_Old IS NOT NULL
					THEN j.dtOUT_Old
					ELSE a.TimeOutMG
				END AS TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF
						(
							mi, 
							CASE WHEN j.dtIN_Old IS NOT NULL 
								THEN j.dtIN_Old
								ELSE a.TimeInMG
							END, 
							CASE WHEN j.dtOUT_Old IS NOT NULL
								THEN j.dtOUT_Old
								ELSE a.TimeOutMG
							END
						)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(g.Approved, 0) AS OTApproved,
				CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.OTStartTime_New
					ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
				END AS OTStartTime,
				CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
					THEN j.OTEndTime_New
					ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
				END AS OTEndTime,
				DATEDIFF
				(
					n,
					CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
						THEN j.OTStartTime_New
						ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
					END,
					CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
						THEN j.OTEndTime_New
						ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
					END
				) AS OTDuration,

				CASE WHEN j.NoPayHours_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NoPayHours_New
					ELSE d.NoPayHours
				END AS NoPayHours,

				CASE WHEN j.ShavedIn_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedIn_New
					ELSE d.Shaved_IN
				END AS Shaved_IN,
				CASE WHEN j.ShavedOut_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedOut_New
					ELSE d.Shaved_OUT
				END AS Shaved_OUT,

				CASE WHEN j.DurationWorkedCumulative_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.DurationWorkedCumulative_New
					ELSE d.Duration_Worked_Cumulative
				END AS Duration_Worked_Cumulative,

				CASE WHEN j.NetMinutes_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NetMinutes_New
					ELSE d.NetMinutes
				END AS NetMinutes,
				d.dtIN,
				d.dtOUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END	AS IsTimesheetExecuted, 
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				1 AS IsValidSwipe,
				d.ShiftSpan
			FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				
				--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1 
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
							AND DT = a.SwipeDate
				) d

				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				
				--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
					WHERE IsActive = 1 
						AND RTRIM(CostCenter) = RTRIM(a.CostCenter)
				) m
				OUTER APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
					WHERE x.EmpNo = a.EmpNo
				) n
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
				AND a.SwipeDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Exlude records when Timesheet is not yet processed	
				AND 
				(
					(
						--Return records not yet submitted for approval	
						ISNULL(a.IsSubmittedForApproval, 0) = 0	AND a.IsCorrected IS NULL
					)
					OR
					(
						--Return records which have been corrected, submitted and already approved
						a.IsSubmittedForApproval = 1 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '123'	
					)
				)
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	--Rev. #4.0
				AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 AND l.IsSyncTimesheet = 1)  
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
				AND 
				(
					(n.IsDayShift = 1 AND @filterDayShift = 1)
					OR (n.IsDayShift = 0 AND @filterDayShift = 0)
					OR @filterDayShift IS NULL 
				)
			ORDER BY SwipeDate DESC, CostCenter, EmpNo
	END		

	ELSE IF @displayType = 'SWIPECCC'			--Return the cost centers with missing swipes from the Plant readers
	BEGIN
		
		--Get the missing swipes from the plant readers
		SELECT DISTINCT
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
			ISNULL(g.AdminEmpNo, @DefaultAdminEmpNo) AS ServiceProviderEmpNo,	--Rev. #5.2	
			LTRIM(RTRIM(ISNULL(h.EAEMAL, @DefaultAdminEmail))) AS ServiceProviderEmail
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			LEFT JOIN
			(
				SELECT a.CostCenter, b.GenericNo AS AdminEmpNo
				FROM
				(
					SELECT DISTINCT CostCenter, TimesheetAdmins 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)		--Rev. #5.0
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 AND y.ReaderNo BETWEEN 41 AND 70		--(Notes: Reader nos. from 41 to 70 refer to the plant readers)
					WHERE IsActive = 1
				) a
				CROSS APPLY tas.fnParseStringArrayToInt(a.TimesheetAdmins, ',') b
			) g ON RTRIM(a.CostCenter) = RTRIM(g.CostCenter)
			LEFT JOIN tas.syJDE_F01151 h WITH (NOLOCK) ON g.AdminEmpNo = h.EAAN8
			LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			CROSS APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND ISNULL(y.IsDayShift, 0) = 0
				WHERE x.EmpNo = a.EmpNo
			) m	
		WHERE 
			(
				(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
				OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
				OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
			)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0		
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND		--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END				
			) = 1	
			AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval		
			AND	
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	--Rev. #4.0
			AND		--Rev. #5.3
			(
				l.IsWorkplaceEnabled = 1 AND (l.IsAdminBldgEnabled = 0 OR m.IsDayShift = 0) AND l.IsSyncTimesheet = 1
			)	
		ORDER BY CostCenter
	END

	ELSE IF @displayType = 'SWPVALMISS'			--Get missing and valid swipes from Plant readers
	BEGIN
		
		SELECT DISTINCT * FROM
		(
			--Missing swipes
			SELECT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(g.Approved, 0) AS OTApproved,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,		
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				d.dtIN,
				d.dtOUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				0 AS IsValidSwipe,
				d.ShiftSpan				
			FROM 
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
							AND DT = a.SwipeDate
				) d

				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID		--Rev. #5.0
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
				LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))	--Rev. #2.9
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 x.EffectiveDate 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)		--Rev. #5.0
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 --AND y.ReaderNo BETWEEN 41 AND 70		--(Notes: Reader nos. from 41 to 70 refer to the plant readers)
					WHERE x.IsActive = 1
						AND RTRIM(x.CostCenter) = RTRIM(a.CostCenter)
				) m
				CROSS APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift  
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND ISNULL(y.IsDayShift, 0) = 0
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(
					(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
					OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
				)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND		--Rev. #5.3
				(
					l.IsWorkplaceEnabled = 1 AND (l.IsAdminBldgEnabled = 0 OR n.IsDayShift = 0) AND l.IsSyncTimesheet = 1
				)	
				--AND
				--( 
				--	(l.IsWorkplaceEnabled = 1 AND l.IsSyncTimesheet = 1)
				--	AND (l.IsAdminBldgEnabled = 0 AND ISNULL(a.IsDayShift, 0) = 0)		--Rev. #5.2
				--)
				AND 
				(
					(
						(
							ISNULL(d.CorrectionCode, '') = ''
							OR 
							(
								ISNULL(d.CorrectionCode, '') <> '' 
								AND 
								(d.dtIN IS NULL OR d.dtOUT IS NULL)
								AND
								j.LogID IS NOT NULL
							) 
						)
						AND d.IsLastRow = 1
					)
					OR d.AutoID IS NULL
				)
				AND
				(
					(i.DT IS NULL OR i.TempSwipeID IS NULL) AND (l.IsAdminBldgEnabled = 0 OR n.IsDayShift = 0)		--Rev. #5.2
				)
				AND	--(Note: Exlude records when Timesheet is not yet processed)
				(
					CASE WHEN 
						(
							d.AutoID IS NOT NULL 
							AND NOT
							(
								--Note: Exclude morning and evening shifts if dtOUT is not yet defined
								(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
								AND d.dtOUT IS NULL
								AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
							)
							AND j.LogID IS NOT NULL
						)
						OR j.LogID IS NULL
						THEN 1 
						ELSE 0 
					END
				) = 1		
				AND ISNULL(a.IsSubmittedForApproval, 0) = 0	--Return records not yet submitted for approval		
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)

		UNION

			--Valid swipes
			SELECT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				CASE WHEN j.dtIN_Old IS NOT NULL 
					THEN j.dtIN_Old
					ELSE a.TimeInMG
				END AS TimeInMG,
				CASE WHEN j.dtOUT_Old IS NOT NULL
					THEN j.dtOUT_Old
					ELSE a.TimeOutMG
				END AS TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF
						(
							mi, 
							CASE WHEN j.dtIN_Old IS NOT NULL 
								THEN j.dtIN_Old
								ELSE a.TimeInMG
							END, 
							CASE WHEN j.dtOUT_Old IS NOT NULL
								THEN j.dtOUT_Old
								ELSE a.TimeOutMG
							END
						)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(g.Approved, 0) AS OTApproved,
				CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.OTStartTime_New
					ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
				END AS OTStartTime,
				CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
					THEN j.OTEndTime_New
					ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
				END AS OTEndTime,
				ISNULL(DATEDIFF
				(
					n,
					CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
						THEN j.OTStartTime_New
						ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
					END,
					CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
						THEN j.OTEndTime_New
						ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
					END
				), 0) AS OTDuration,

				CASE WHEN j.NoPayHours_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NoPayHours_New
					ELSE d.NoPayHours
				END AS NoPayHours,

				CASE WHEN j.ShavedIn_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedIn_New
					ELSE d.Shaved_IN
				END AS Shaved_IN,
				CASE WHEN j.ShavedOut_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedOut_New
					ELSE d.Shaved_OUT
				END AS Shaved_OUT,

				CASE WHEN j.DurationWorkedCumulative_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.DurationWorkedCumulative_New
					ELSE d.Duration_Worked_Cumulative
				END AS Duration_Worked_Cumulative,

				CASE WHEN j.NetMinutes_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NetMinutes_New
					ELSE d.NetMinutes
				END AS NetMinutes,

				d.dtIN,
				d.dtOUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END	AS IsTimesheetExecuted,		
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				1 AS IsValidSwipe,
				d.ShiftSpan
			FROM 
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
				) d

				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID		--Rev. #5.0
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 x.EffectiveDate 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)		--Rev. #5.0
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 --AND y.ReaderNo BETWEEN 41 AND 70		--(Notes: Reader nos. from 41 to 70 refer to the plant readers)
					WHERE x.IsActive = 1
						AND RTRIM(x.CostCenter) = RTRIM(a.CostCenter)
				) m
				CROSS APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift    
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND ISNULL(y.IsDayShift, 0) = 0
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
				AND		--Rev. #5.3
				(
					l.IsWorkplaceEnabled = 1 AND (l.IsAdminBldgEnabled = 0 OR n.IsDayShift = 0) AND l.IsSyncTimesheet = 1
				)
				--AND
				--( 
				--	(l.IsWorkplaceEnabled = 1 AND l.IsSyncTimesheet = 1)
				--	AND (l.IsAdminBldgEnabled = 0 AND ISNULL(a.IsDayShift, 0) = 0)		--Rev. #5.2
				--)	
				AND (d.IsLastRow = 1 OR d.AutoID IS NULL)	
				AND	--(Note: Exlude records when Timesheet is not yet processed)
				(
					CASE WHEN 
						(
							d.AutoID IS NOT NULL 
							AND NOT
							(
								--Note: Exclude morning and evening shifts if dtOUT is not yet defined
								(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
								AND d.dtOUT IS NULL
								AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
							)
							AND j.LogID IS NOT NULL
						)
						OR j.LogID IS NULL
						THEN 1 
						ELSE 0 
					END
				) = 1
				AND a.IsCorrected IS NULL
				AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval	
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
		) a
		ORDER BY SwipeDate DESC, IsValidSwipe, CostCenter, EmpNo
	END

	--Rev. #5.0
	ELSE IF @displayType = 'SWIPEADMIN'			--Return the cost centers with missing swipes from the Admin Building readers
	BEGIN
		
		SELECT DISTINCT
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				g.AdminEmpNo AS ServiceProviderEmpNo,
				LTRIM(RTRIM(h.EAEMAL)) AS ServiceProviderEmail
			FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				LEFT JOIN
				(
					SELECT a.CostCenter, b.GenericNo AS AdminEmpNo
					FROM
					(
						SELECT DISTINCT CostCenter, TimesheetAdmins 
						FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)
							INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 AND y.SourceID = 2
						WHERE IsActive = 1
					) a
					CROSS APPLY tas.fnParseStringArrayToInt(a.TimesheetAdmins, ',') b
				) g ON RTRIM(a.CostCenter) = RTRIM(g.CostCenter)
				LEFT JOIN tas.syJDE_F01151 h WITH (NOLOCK) ON g.AdminEmpNo = h.EAAN8
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 EffectiveDate 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)  
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 AND y.SourceID = 2
					WHERE IsActive = 1 
						AND RTRIM(x.CostCenter) = RTRIM(a.CostCenter)
				) m
				CROSS APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode 
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND y.IsDayShift = 1
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(
					(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
					OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
				)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND 
				(
					(
						(
							ISNULL(d.CorrectionCode, '') = ''
							OR 
							(
								ISNULL(d.CorrectionCode, '') <> '' 
								AND 
								(d.dtIN IS NULL OR d.dtOUT IS NULL)
								AND
								j.LogID IS NOT NULL
							) 
						)
						AND d.IsLastRow = 1
					)
					OR d.AutoID IS NULL
				)
				AND a.SwipeDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))	--Exlude records when Timesheet is not yet processed
				AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval		
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 AND l.IsSyncTimesheet = 1)
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
		ORDER BY CostCenter
	END

	--Rev. #5.0
	ELSE IF @displayType = 'SWPVALADMN'			--Get missing and valid swipes from Admin Bldg. readers
	BEGIN
		
		SELECT DISTINCT * FROM
		(
			--Missing swipes
			SELECT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(g.Approved, 0) AS OTApproved,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,		
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				d.dtIN,
				d.dtOUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				0 AS IsValidSwipe,
				d.ShiftSpan				
			FROM 
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
				
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
							AND DT = a.SwipeDate
				) d

				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
				
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
				LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))	--Rev. #2.9
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 x.EffectiveDate 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)  
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 AND y.SourceID = 2		--(Notes: SourceID = 2 referes to all Admin Bldg. readers)
					WHERE x.IsActive = 1
						AND RTRIM(x.CostCenter) = RTRIM(a.CostCenter)
				) m
				CROSS APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode 
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND y.IsDayShift = 1
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(
					(TimeINWP IS NOT NULL AND TimeOutWP IS NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NOT NULL)
					OR (TimeINWP IS NULL AND TimeOutWP IS NULL)
					OR (IsCorrected = 1 AND ISNULL(CorrectionType, 0) > 0)
				)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND (l.IsWorkplaceEnabled = 1 AND l.IsSyncTimesheet = 1 AND l.IsAdminBldgEnabled = 1 )
				AND 
				(
					(
						(
							ISNULL(d.CorrectionCode, '') = ''
							OR 
							(
								ISNULL(d.CorrectionCode, '') <> '' 
								AND 
								(d.dtIN IS NULL OR d.dtOUT IS NULL)
								AND
								j.LogID IS NOT NULL
							) 
						)
						AND d.IsLastRow = 1
					)
					OR d.AutoID IS NULL
				)
				AND (i.DT IS NULL OR i.TempSwipeID IS NULL) 
				AND	--(Note: Exlude records when Timesheet is not yet processed)
				(
					CASE WHEN 
						(
							d.AutoID IS NOT NULL 
							AND NOT
							(
								--Note: Exclude morning and evening shifts if dtOUT is not yet defined
								(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
								AND d.dtOUT IS NULL
								AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
							)
							AND j.LogID IS NOT NULL
						)
						OR j.LogID IS NULL
						THEN 1 
						ELSE 0 
					END
				) = 1		
				AND ISNULL(a.IsSubmittedForApproval, 0) = 0	--Return records not yet submitted for approval		
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)

		UNION

			--Valid swipes
			SELECT 
				SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				CASE WHEN j.dtIN_Old IS NOT NULL 
					THEN j.dtIN_Old
					ELSE a.TimeInMG
				END AS TimeInMG,
				CASE WHEN j.dtOUT_Old IS NOT NULL
					THEN j.dtOUT_Old
					ELSE a.TimeOutMG
				END AS TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF
						(
							mi, 
							CASE WHEN j.dtIN_Old IS NOT NULL 
								THEN j.dtIN_Old
								ELSE a.TimeInMG
							END, 
							CASE WHEN j.dtOUT_Old IS NOT NULL
								THEN j.dtOUT_Old
								ELSE a.TimeOutMG
							END
						)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				CASE WHEN a.IsCorrected = 1
					THEN a.Remarks
					ELSE
						CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe in and out at the workplace'
							WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
							THEN 'Missing swipe in at the workplace'
							WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
							THEN 'Missing swipe out at the workplace'
							ELSE ''
						END
				END AS Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				a.LastUpdateTime,
				a.LastUpdateEmpNo,
				a.LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,

				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(g.Approved, 0) AS OTApproved,
				CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.OTStartTime_New
					ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
				END AS OTStartTime,
				CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
					THEN j.OTEndTime_New
					ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
				END AS OTEndTime,
				ISNULL(DATEDIFF
				(
					n,
					CASE WHEN j.OTStartTime_New IS NOT NULL	AND ISNULL(a.IsCorrected, 0) = 0
						THEN j.OTStartTime_New
						ELSE ISNULL(d.OTStartTime, g.OTStartTime) 
					END,
					CASE WHEN j.OTEndTime_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0	
						THEN j.OTEndTime_New
						ELSE ISNULL(d.OTEndTime, g.OTEndTime) 
					END
				), 0) AS OTDuration,

				CASE WHEN j.NoPayHours_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NoPayHours_New
					ELSE d.NoPayHours
				END AS NoPayHours,

				CASE WHEN j.ShavedIn_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedIn_New
					ELSE d.Shaved_IN
				END AS Shaved_IN,
				CASE WHEN j.ShavedOut_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.ShavedOut_New
					ELSE d.Shaved_OUT
				END AS Shaved_OUT,

				CASE WHEN j.DurationWorkedCumulative_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.DurationWorkedCumulative_New
					ELSE d.Duration_Worked_Cumulative
				END AS Duration_Worked_Cumulative,

				CASE WHEN j.NetMinutes_New IS NOT NULL AND ISNULL(a.IsCorrected, 0) = 0
					THEN j.NetMinutes_New
					ELSE d.NetMinutes
				END AS NetMinutes,

				d.dtIN,
				d.dtOUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END	AS IsTimesheetExecuted,		
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				1 AS IsValidSwipe,
				d.ShiftSpan
			FROM 
				(
					SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
					WHERE YEAR(SwipeDate) = YEAR(GETDATE())
				) a
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)				
				OUTER APPLY
				(
					SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
					WHERE YEAR(DT) = YEAR(GETDATE())
						AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
				) d
				LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
				LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8				
				OUTER APPLY
				(
					SELECT y.* 
					FROM tas.Tran_Timesheet x WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
					WHERE YEAR(x.DT) = YEAR(GETDATE())
						AND x.AutoID = d.AutoID
				) g

				LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
				LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
				CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
				CROSS APPLY
				(
					SELECT TOP 1 x.EffectiveDate 
					FROM tas.WorkplaceReaderSetting x WITH (NOLOCK)  
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo AND y.LocationCode = 8 AND y.SourceID = 2		--(Notes: SourceID = 2 referes to all Admin Bldg. readers)
					WHERE x.IsActive = 1
						AND RTRIM(x.CostCenter) = RTRIM(a.CostCenter)
				) m
				CROSS APPLY		--Rev. #5.1
				(
					SELECT x.ShiftPatCode 
					FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
						INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) AND y.IsDayShift = 1
					WHERE x.EmpNo = a.EmpNo
				) n	
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND 
				(
					RTRIM(a.CostCenter) = RTRIM(@costCenter) 
					OR 
					(
						@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
					)
					OR 
					(
						@costCenter IS NULL AND @userEmpNo > 0
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
								INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TASNEW'
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
				AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				AND 
				(
					CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
					OR (@startDate IS NULL AND @endDate IS NULL)
				)
				AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
				AND (l.IsWorkplaceEnabled = 1 AND l.IsSyncTimesheet = 1 AND l.IsAdminBldgEnabled = 1 )
				AND (d.IsLastRow = 1 OR d.AutoID IS NULL)	
				AND	--(Note: Exlude records when Timesheet is not yet processed)
				(
					CASE WHEN 
						(
							d.AutoID IS NOT NULL 
							AND NOT
							(
								--Note: Exclude morning and evening shifts if dtOUT is not yet defined
								(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
								AND d.dtOUT IS NULL
								AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
							)
							AND j.LogID IS NOT NULL
						)
						OR j.LogID IS NULL
						THEN 1 
						ELSE 0 
					END
				) = 1
				AND a.IsCorrected IS NULL
				AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval	
				AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
				AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
		) a
		ORDER BY SwipeDate DESC, IsValidSwipe, CostCenter, EmpNo
	END

	ELSE IF @displayType = 'SWIPEALLCC'			--Return all cost centers with missing and complete swipes
	BEGIN
		
		--Get the valid and missing swipes from plant readers
		SELECT DISTINCT
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,						
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,			
			g.AdminEmpNo AS ServiceProviderEmpNo,
			LTRIM(RTRIM(h.EAEMAL)) AS ServiceProviderEmail
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			LEFT JOIN
			(
				SELECT a.CostCenter, b.GenericNo AS AdminEmpNo
				FROM
				(
					SELECT DISTINCT CostCenter, TimesheetAdmins 
					FROM tas.WorkplaceReaderSetting WITH (NOLOCK)
					WHERE IsActive = 1
				) a
				CROSS APPLY tas.fnParseStringArrayToInt(a.TimesheetAdmins, ',') b
			) g ON RTRIM(a.CostCenter) = RTRIM(g.CostCenter)
			LEFT JOIN tas.syJDE_F01151 h WITH (NOLOCK) ON g.AdminEmpNo = h.EAAN8
			LEFT JOIN tas.Tran_TempSwipeData i WITH (NOLOCK) ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			LEFT JOIN tas.WorkplaceSwipeExclusion k WITH (NOLOCK) ON a.EmpNo = k.EmpNo AND RTRIM(a.CostCenter) = RTRIM(k.CostCenter)
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) l
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0			
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND		--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END				
			) = 1	
			AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval		
			AND	
			(
				(k.AutoID IS NOT NULL AND a.SwipeDate < k.EffectiveDate)
				OR k.AutoID IS NULL 
			)	
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND 
			(
				(l.IsDayShift = 1 AND @filterDayShift = 1)
				OR (l.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)

		UNION
        
		--Get the valid and missing swipes from Admin Bldg. readers
		SELECT DISTINCT
			a.CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			c.Superintendent,
			LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,						
			c.CostCenterManager,
			LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,			
			g.AdminEmpNo AS ServiceProviderEmpNo,
			LTRIM(RTRIM(h.EAEMAL)) AS ServiceProviderEmail
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			LEFT JOIN
			(
				SELECT a.CostCenter, b.GenericNo AS AdminEmpNo
				FROM
				(
					SELECT DISTINCT CostCenter, TimesheetAdmins 
					FROM tas.WorkplaceReaderSetting WITH (NOLOCK)
					WHERE IsActive = 1
				) a
				CROSS APPLY tas.fnParseStringArrayToInt(a.TimesheetAdmins, ',') b
			) g ON RTRIM(a.CostCenter) = RTRIM(g.CostCenter)
			LEFT JOIN tas.syJDE_F01151 h WITH (NOLOCK) ON g.AdminEmpNo = h.EAAN8
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			CROSS APPLY
			(
				SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
				WHERE IsActive = 1 
					AND RTRIM(CostCenter) = RTRIM(a.CostCenter)
			) m
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) n
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND a.SwipeDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))	--Exlude records when Timesheet is not yet processed
			AND ISNULL(a.IsSubmittedForApproval, 0) = 0		--Return records not yet submitted for approval		
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 AND l.IsSyncTimesheet = 1)
			AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
			AND 
			(
				(n.IsDayShift = 1 AND @filterDayShift = 1)
				OR (n.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)
		ORDER BY CostCenter
	END

	ELSE IF @displayType = 'SWPATENHIS'			--Used in Employee Attendance History Report
	BEGIN
		
		SELECT 
			a.AutoID,
			a.EmpNo,
			b.EmpName,
			a.GradeCode,
			b.PayStatus,
			LTRIM(RTRIM(ISNULL(g.JMDL01, ''))) AS Position,
			a.ShiftPatCode,
			a.ShiftCode,
			ISNULL(a.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
			e.IsDayShift,
			a.BusinessUnit AS CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.DT AS SwipeDate,
			a.dtIN,
			a.dtOUT,
			a.Shaved_IN,
			a.Shaved_OUT,
			a.Duration_Required,
			a.Duration_Worked_Cumulative,
			a.NetMinutes,
			a.NoPayHours,
			ISNULL(a.OTType, d.OTType) AS OTType,
			ISNULL(a.OTStartTime, d.OTStartTime) AS OTStartTime,
			ISNULL(a.OTEndTime, d.OTEndTime) AS OTEndTime,
			DATEDIFF
			(
				n,
				CASE WHEN a.OTStartTime IS NOT NULL THEN a.OTStartTime ELSE d.OTStartTime END,
				CASE WHEN a.OTEndTime IS NOT NULL THEN a.OTEndTime ELSE d.OTendTime END
			) AS OTDuration,
			CASE WHEN ISNULL(a.Actual_ShiftCode, a.ShiftCode) = 'O' THEN 'Day Off'
				WHEN RTRIM(ISNULL(a.LeaveType, '')) = 'AL' THEN 'Annual Leave'
				WHEN RTRIM(ISNULL(a.LeaveType, '')) = 'SL' THEN 'Sick Leave'
				WHEN RTRIM(ISNULL(a.LeaveType, '')) = 'IL' THEN 'Injury Leave'
				WHEN RTRIM(ISNULL(a.RemarkCode, '')) = 'A' THEN 'Absent'
				ELSE ''
			END AS Remarks,
			a.LeaveType,
			a.RemarkCode,
			a.CorrectionCode,
			a.AbsenceReasonCode,
			a.AbsenceReasonColumn,
			@isApplyToTimesheet AS IsApplyToTimesheet,

			h.TimeInMG,
			h.TimeOutMG,
			h.TimeInWP,
			h.TimeOutWP,
			h.DurationRequired,

			--Start of Rev.# 3.3
			CASE WHEN h.TimeInMG IS NOT NULL AND h.TimeOutMG IS NOT NULL
				THEN DATEDIFF(mi, h.TimeInMG, h.TimeOutMG)
				ELSE NULL
			END AS NetMinutesMG,
			CASE WHEN h.TimeInWP IS NOT NULL AND h.TimeOutWP IS NOT NULL
				THEN DATEDIFF(mi, h.TimeInWP, h.TimeOutWP)
				ELSE NULL
			END AS NetMinutesWP,
			--End of Rev.# 3.3

			CASE WHEN h.IsCorrected = 1
				THEN h.Remarks
				ELSE
					CASE WHEN ISNULL(h.TimeInWP, '') = '' AND ISNULL(h.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(h.TimeInWP, '') = '' AND ISNULL(h.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(h.TimeInWP, '') <> '' AND ISNULL(h.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks	
		FROM tas.Tran_Timesheet a WITH (NOLOCK) 
			LEFT JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
			LEFT JOIN tas.Tran_Timesheet_Extra d WITH (NOLOCK) ON a.AutoID = d.XID_AutoID	
			INNER JOIN tas.Master_ShiftPatternTitles e WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
			INNER JOIN tas.syJDE_F060116 f WITH (NOLOCK) on a.EmpNo = f.YAAN8
			LEFT JOIN tas.syJDE_F08001 g WITH (NOLOCK) on LTRIM(RTRIM(f.YAJBCD)) = LTRIM(RTRIM(g.JMJBCD))
			LEFT JOIN tas.Tran_WorkplaceSwipe h WITH (NOLOCK) ON a.EmpNo = h.EmpNo AND a.DT = h.SwipeDate
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				(
					RTRIM(a.BusinessUnit) = RTRIM(@costCenter) 
					AND RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
				)
				OR @costCenter IS NULL
			)
			AND CONVERT(VARCHAR, a.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			AND a.IsLastRow = 1
			AND NOT	--Note: Exclude morning and evening shifts if dtOUT is not yet defined
			(
				ISNULL(a.Actual_ShiftCode, a.ShiftCode) IN ('M', 'E') AND a.dtOUT IS NULL AND h.TimeOutMG IS NULL
			)
		ORDER BY a.BusinessUnit, a.EmpNo, a.DT
	END

	ELSE IF @displayType = 'SWPFORAPV'			--Show swipe corrections for approval
	BEGIN
		
		--Get corrected swipes from plant workplace readers
		SELECT  DISTINCT
				a.SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				a.Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				ISNULL(k.HistCreatedDate, a.LastUpdateTime) AS LastUpdateTime,
				ISNULL(k.HistCreatedBy, a.LastUpdateEmpNo) AS LastUpdateEmpNo,
				ISNULL(k.HistCreatedName, a.LastUpdateEmpName) AS LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,
				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				ISNULL(g.Approved, 0) AS OTApproved,
				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,					
				d.dtIN,
				d.dtOUT,
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				CASE WHEN a.StatusHandlingCode IN ('Closed', 'Approved') 
					THEN 1 
					ELSE 0
				END AS IsValidSwipe
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a
			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			
			--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d

			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			
			--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g

			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.Tran_TempSwipeData i ON d.EmpNo = i.EmpNo AND d.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, i.DTSwipeNewProcess, 12))
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			CROSS APPLY
			(
				SELECT TOP 1 * FROM tas.WorkflowHistory WITH (NOLOCK) 
				WHERE SwipeID = a.SwipeID 
				ORDER BY AutoID DESC
			) k
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) m
		WHERE 
			IsCorrected = 1 
			AND ISNULL(CorrectionType, 0) > 0
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND RTRIM(a.CostCenter) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
			AND 
			(
				CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
			AND ISNULL(a.IsDayShift, 0) = 0
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND
			(
				i.DT IS NULL OR i.TempSwipeID IS NULL
			)
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END
			) = 1	
			AND 
			(
				(@statusCode IN ('Open', 'Rejected', 'Cancelled') AND RTRIM(a.StatusHandlingCode) = RTRIM(@statusCode))
				OR
				(@statusCode = 'Approved' AND a.IsClosed = 1 AND RTRIM(a.StatusCode) IN ('120', '123'))
				OR
				(UPPER(RTRIM(@statusCode)) = 'ALL STATUS' AND RTRIM(a.StatusHandlingCode) IN ('Open', 'Rejected', 'Cancelled', 'Approved', 'Closed'))
				OR 
				@statusCode IS NULL
			)		
			AND 
			(
				ISNULL(a.IsSubmittedForApproval, 0) = 1		--Submitted requests for approval
				OR (ISNULL(a.IsSubmittedForApproval, 0) = 0 AND a.IsClosed = 1)	--Rejected by approver or cancelled by user		
			)
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 0 AND l.IsSyncTimesheet = 1)
			AND 
			(
				(m.IsDayShift = 1 AND @filterDayShift = 1)
				OR (m.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)

		UNION
        
		--Get corrected swipes from Admin Bldg. readers
		SELECT  DISTINCT
				a.SwipeID,
				a.EmpNo,
				a.EmpName,
				b.GradeCode, 
				a.CostCenter,
				RTRIM(c.BusinessUnitName) AS CostCenterName,
				a.Position,
				a.IsDayShift,
				a.ShiftPatCode,
				a.ShiftCode,
				ISNULL(d.Actual_ShiftCode, a.ShiftCode) AS Actual_ShiftCode,
				a.ShiftPointer,
				CASE WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'O'
					THEN 'Day-off'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'M' 
					THEN 'Morning shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'E' 
					THEN 'Evening Shift'
					WHEN ISNULL(d.Actual_ShiftCode, a.ShiftCode) = 'N' 
					THEN 'Night shift'
					ELSE ''
				END AS ShiftDetail,		
				a.SwipeDate,
				a.TimeInMG,
				a.TimeOutMG,
				a.TimeInWP,
				a.TimeOutWP,
				a.DurationRequired,
				CASE WHEN a.TimeInMG IS NOT NULL AND a.TimeOutMG IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInMG, a.TimeOutMG)
					ELSE NULL
				END AS NetMinutesMG,
				CASE WHEN a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL
					THEN DATEDIFF(mi, a.TimeInWP, a.TimeOutWP)
					ELSE NULL
				END AS NetMinutesWP,
				a.Remarks,
				a.IsProcessedByTimesheet,
				a.IsCorrected,	
				a.CorrectionType,
				a.CreatedDate,
				a.CreatedByEmpNo,
				a.CreatedByEmpName,
				ISNULL(k.HistCreatedDate, a.LastUpdateTime) AS LastUpdateTime,
				ISNULL(k.HistCreatedBy, a.LastUpdateEmpNo) AS LastUpdateEmpNo,
				ISNULL(k.HistCreatedName, a.LastUpdateEmpName) AS LastUpdateEmpName,
				d.LeaveType,
				d.RemarkCode,
				d.CorrectionCode,
				c.Superintendent,
				LTRIM(RTRIM(e.EAEMAL)) AS SuperintendentEmail,
				c.CostCenterManager,
				LTRIM(RTRIM(f.EAEMAL)) AS CostCenterManagerEmail,
				0 AS ServiceProviderEmpNo,
				'' AS ServiceProviderEmail,
				ISNULL(d.OTType, g.OTType) AS OTType,
				ISNULL(d.OTStartTime, g.OTStartTime) AS OTStartTime,
				ISNULL(d.OTEndTime, g.OTEndTime) AS OTEndTime,
				ISNULL(DATEDIFF(n, CASE WHEN ISNULL(d.OTStartTime, '') = '' THEN g.OTStartTime	ELSE d.OTStartTime END, CASE WHEN ISNULL(d.OTEndTime, '') = '' THEN g.OTEndTime	ELSE d.OTEndTime END), 0) AS OTDuration,
				ISNULL(g.Approved, 0) AS OTApproved,
				d.NoPayHours,
				d.Shaved_IN,
				d.Shaved_OUT,
				CONVERT(VARCHAR(8), h.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), h.DepartFrom, 108) AS ShiftTiming,
				d.AutoID,	
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END AS IsTimesheetExecuted,					
				d.dtIN,
				d.dtOUT,
				d.Duration_Worked_Cumulative,
				d.NetMinutes,
				a.StatusID,
				a.StatusCode,
				a.StatusDesc,
				a.StatusHandlingCode,
				a.CurrentlyAssignedEmpNo,
				a.CurrentlyAssignedEmpName,
				a.CurrentlyAssignedEmpEmail,
				a.ServiceProviderTypeCode,
				a.DistListCode,
				a.IsClosed,
				a.ClosedDate,
				a.IsSubmittedForApproval,
				a.SubmittedDate,
				a.SubmittedByEmpNo,
				@isApplyToTimesheet AS IsApplyToTimesheet,
				CASE WHEN a.StatusHandlingCode IN ('Closed', 'Approved') 
					THEN 1 
					ELSE 0
				END AS IsValidSwipe
		FROM --tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			(
				SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
				WHERE YEAR(SwipeDate) = YEAR(GETDATE())
			) a

			INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
			
			--LEFT JOIN tas.Tran_Timesheet  d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DT AND d.IsLastRow = 1
			OUTER APPLY
			(
				SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
				WHERE YEAR(DT) = YEAR(GETDATE())
					AND EmpNo = a.EmpNo
						AND DT = a.SwipeDate
			) d

			LEFT JOIN tas.syJDE_F01151 e WITH (NOLOCK) ON c.Superintendent = e.EAAN8
			LEFT JOIN tas.syJDE_F01151 f WITH (NOLOCK) ON c.CostCenterManager = f.EAAN8
			
			--LEFT JOIN tas.Tran_Timesheet_Extra g WITH (NOLOCK) ON d.AutoID = g.XID_AutoID	
			OUTER APPLY
			(
				SELECT y.* 
				FROM tas.Tran_Timesheet x WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
				WHERE YEAR(x.DT) = YEAR(GETDATE())
					AND x.AutoID = d.AutoID
			) g

			LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.SyncWorkplaceSwipeToTimesheetLog j WITH (NOLOCK) ON d.AutoID = j.AutoID AND d.EmpNo = j.EmpNo AND RTRIM(a.CostCenter) = RTRIM(j.CostCenter)
			CROSS APPLY
			(
				SELECT TOP 1 * FROM tas.WorkflowHistory WITH (NOLOCK) 
				WHERE SwipeID = a.SwipeID 
				ORDER BY AutoID DESC
			) k
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) l	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	
			CROSS APPLY
			(
				SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
				WHERE IsActive = 1 
					AND RTRIM(CostCenter) = RTRIM(a.CostCenter)
			) m
			OUTER APPLY		--Rev. #5.1
			(
				SELECT x.ShiftPatCode, ISNULL(y.IsDayShift, 0) AS IsDayShift 
				FROM tas.Master_EmployeeAdditional x WITH (NOLOCK)
					INNER JOIN tas.Master_ShiftPatternTitles y WITH (NOLOCK) ON RTRIM(x.ShiftPatCode) = RTRIM(y.ShiftPatCode) 
				WHERE x.EmpNo = a.EmpNo
			) n
		WHERE 
			IsCorrected = 1 
			AND ISNULL(CorrectionType, 0) > 0
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				RTRIM(a.CostCenter) = RTRIM(@costCenter) 
				OR 
				(
					@costCenter IS NULL AND ISNULL(@userEmpNo, 0) = 0
				)
				OR 
				(
					@costCenter IS NULL AND @userEmpNo > 0
					AND RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			AND a.SwipeDate BETWEEN @startDate AND @endDate
			AND 
			(
				(
					(
						ISNULL(d.CorrectionCode, '') = ''
						OR 
						(
							ISNULL(d.CorrectionCode, '') <> '' 
							AND 
							(d.dtIN IS NULL OR d.dtOUT IS NULL)
							AND
							j.LogID IS NOT NULL
						) 
					)
					AND d.IsLastRow = 1
				)
				OR d.AutoID IS NULL
			)
			AND	--(Note: Exlude records when Timesheet is not yet processed)
			(
				CASE WHEN 
					(
						d.AutoID IS NOT NULL 
						AND NOT
						(
							--Note: Exclude morning and evening shifts if dtOUT is not yet defined
							(CASE WHEN ISNULL(d.Actual_ShiftCode, '') <> '' THEN d.Actual_ShiftCode ELSE d.ShiftCode END) IN ('M', 'E')	
							AND d.dtOUT IS NULL
							AND d.DT = CONVERT(DATETIME, GETDATE(), 101) 
						)
						AND j.LogID IS NOT NULL
					)
					OR j.LogID IS NULL
					THEN 1 
					ELSE 0 
				END
			) = 1	
			AND 
			(
				(@statusCode IN ('Open', 'Rejected', 'Cancelled') AND RTRIM(a.StatusHandlingCode) = RTRIM(@statusCode))
				OR
				(@statusCode = 'Approved' AND a.IsClosed = 1 AND RTRIM(a.StatusCode) IN ('120', '123'))
				OR
				(UPPER(RTRIM(@statusCode)) = 'ALL STATUS' AND RTRIM(a.StatusHandlingCode) IN ('Open', 'Rejected', 'Cancelled', 'Approved', 'Closed'))
				OR 
				@statusCode IS NULL
			)		
			AND 
			(
				ISNULL(a.IsSubmittedForApproval, 0) = 1		--Submitted requests for approval
				OR (ISNULL(a.IsSubmittedForApproval, 0) = 0 AND a.IsClosed = 1)	--Rejected by approver or cancelled by user		
			)
			AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0	
			AND (l.IsWorkplaceEnabled = 1 AND l.IsAdminBldgEnabled = 1 AND l.IsSyncTimesheet = 1)
			AND (a.SwipeDate >= m.EffectiveDate AND m.EffectiveDate IS NOT NULL)
			AND 
			(
				(n.IsDayShift = 1 AND @filterDayShift = 1)
				OR (n.IsDayShift = 0 AND @filterDayShift = 0)
				OR @filterDayShift IS NULL 
			)
		ORDER BY a.IsClosed, a.StatusHandlingCode, a.SwipeDate DESC, a.CostCenter, a.EmpNo
	END
END


/*	Debug:

PARAMETERS:
	@displayType		varchar(10),	
	@empNo				int = 0,	
	@costCenter			varchar(12)	= '',
	@startDate			datetime = null,
	@endDate			datetime = null,
	@userEmpNo			int = 0,
	@statusCode			varchar(10) = ''

	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPVALSWIP', 0, '7600', '04/16/2022', '05/15/2022', 10003632, 'STATALL'		--Show valid swipes
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPALL', 0, '7600', '04/16/2022', '05/15/2022', 10003632, 'STATALL'			--Show missing swipes
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPWITHCHK', 0, '7600', '04/16/2022', '05/15/2022', 10003632, 'STATALL'		--Shonw corrected swipes pending for submission
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPFORAPV', 0, '7600', '04/16/2022', '05/15/2022', 10003632, 'STATALL'		--Shonw corrected swipes pending for approval
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWIPECCC', 0, '', '07/16/2022', '08/15/2022', 10003632, 'STATALL'			--Return the cost centers with missing swipes
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWIPEALLCC', 0, '', '04/16/2022', '05/15/2022', 10003632, 'STATALL'			--Return all cost centers with missing and complete swipes
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPVALMISS', 0, '7600', '04/30/2022', '04/30/2022', 10003632, 'STATALL'		--Show valid and missing swipes swipes (Used in Workplace Missing Swipes Notification Service)

	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWIPECCC', 0, '', '08/10/2022', '08/10/2022', 10003632, ''					--Return the cost centers with missing swipes from the Plant readers		
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPVALMISS', 0, '5200', '08/10/2022', '08/10/2022', 10003632, NULL			--Show valid and missing swipes swipes (Used in Workplace Missing Swipes Notification Service)

	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWIPECCC', 0, '', '08/14/2022', '08/14/2022', 10003632, ''				--Return the cost centers with missing swipes from the Plant readers		
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPVALMISS', 0, '4100', '08/14/2022', '08/14/2022', 10003632, NULL			--Show valid and missing swipes swipes (Used in Workplace Missing Swipes Notification Service)

	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWIPEADMIN', 0, '', '08/14/2022', '08/14/2022', 10003632, ''				--Return the cost centers with missing swipes from the Admin Bldg. readers
	EXEC tas.Pr_GetWorkplaceMissingSwipes 'SWPVALADMN', 0, '4100', '08/14/2022', '08/14/2022', 10003632, NULL			--Show valid and missing swipes swipes (Used in Workplace Missing Swipes Notification Service)

*/