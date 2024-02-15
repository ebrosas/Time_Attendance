/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAssignedOvertimeRequest
*	Description: Get the currently assigned overtime requisitions
*
*	Date			Author		Revision No.	Comments:
*	09/08/2017		Ervin		1.0				Created
*	06/09/2017		Ervin		1.1				Added extra logic in fetching the Approval Level description that fecth data using the "fnGetCurrentApprovalLevel" function
*	12/09/2017		Ervin		1.2				Added the following fields in the query output results: ArrivalSchedule, TotalWorkDuration, RequiredWorkDuration, IsOTExceedOrig
*	18/09/2017		Ervin		1.3				Refactored the logic in identifying the Shift Code to use
*	08/01/2018		Ervin		1.4				Calculate the value of "OTDurationHourOrig" based on "OTStartTime_Orig" and "OTEndTime_Orig" fields
*	21/01/2018		Ervin		1.5				Set the following order sequence: 1) IsHold; 2) DT 
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAssignedOvertimeRequest
(   
	@currentUserEmpNo	INT, 
	@assignTypeID		TINYINT = 0,		--(Note: 0 = All; 1 = Me; 2 = Others)
	@assignedToEmpNo	INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@costCenter			VARCHAR(12) = '',
	@empNo				INT = 0	
)
AS

	DECLARE	@minutes_MinOT_NSS		INT,
			@code_OTType_Regular	VARCHAR(10)		--OT Code for regular working day

	--Get the minimum OT allowed
	SELECT	@minutes_MinOT_NSS		= Minutes_MinOT_NSS,
			@code_OTType_Regular	= RTRIM(Code_OTtype_Regular)
	FROM tas.System_Values

	--Validate parameters
	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	DISTINCT 
			a.DT,
			a.CostCenter AS BusinessUnit,
			f.BusinessUnitName,
			a.EmpNo,
			e.EmpName,
			e.Position,
			e.GradeCode,
			c.ShiftPatCode,
			c.ShiftCode,
			c.Actual_ShiftCode,
			c.dtIN,
			c.dtOUT,
			d.OTStartTime,
			d.OTEndTime,
			d.OTType,
			CASE WHEN a.OTApproved = 'N' THEN 0 ELSE DATEDIFF(n, d.OTStartTime, d.OTEndTime) END AS OTDurationMinute,
			CASE WHEN a.OTApproved = 'N' THEN 0 ELSE tas.fmtMIN_HHmm(DATEDIFF(n, d.OTStartTime, d.OTEndTime)) END AS OTDurationHour,
			
			--CASE WHEN RTRIM(a.OtReason) IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW')
			--	THEN tas.fmtMIN_HHmm(DATEDIFF(n, DATEADD(n, 180, d.OTStartTime), d.OTEndTime))	--(Note: If OT Reason is callout, then deduct 3 hours in the OT Start Time)
			--	ELSE tas.fmtMIN_HHmm(DATEDIFF(n, d.OTStartTime, d.OTEndTime)) 
			--END AS OTDurationHourOrig,
			CASE WHEN RTRIM(a.OtReason) IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW')
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, DATEADD(n, 180, a.OTStartTime_Orig), a.OTEndTime_Orig))	--(Note: If OT Reason is callout, then deduct 3 hours in the OT Start Time)
				ELSE tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime_Orig, a.OTEndTime_Orig)) 
			END AS OTDurationHourOrig,	--Rev. #1.4

			d.Approved,
			a.MealVoucherEligibility,
			RTRIM(a.OTComment) AS Comment,
			a.OTApproved,
			RTRIM(a.OTReason) AS OTReasonCode,
			g.[DESCRIPTION] AS OTReason,				
			c.AutoID,
			d.XID_AutoID,
			c.Processed,
			a.OTRequestNo,
			a.StatusCode,
			a.StatusDesc,
			a.StatusHandlingCode,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ServiceProviderTypeCode,
			a.DistListCode,
			CASE WHEN ISNULL(a.DistListCode, '') <> '' 
				THEN b.ActivityDesc2
				ELSE tas.fnGetCurrentApprovalLevel(a.OTRequestNo)	--Rev. #1.1
			END AS DistListDesc,
			CASE WHEN ISNULL(a.CurrentlyAssignedEmpNo, 0) = 0 AND RTRIM(a.StatusHandlingCode) = 'Open'
				THEN tas.fnOTDistributionGroupMembers(a.OTRequestNo, b.WorkflowTransactionID)
				ELSE NULL
			END AS DistListMembers,
			a.LastUpdateEmpNo,
			a.LastUpdateEmpName, 
			a.LastUpdateUserID, 
			a.LastUpdateTime, 
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.CreatedByEmail,
			a.CreatedDate,
			a.SubmittedDate,
			CASE WHEN RTRIM(a.OTReason) IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW') THEN 1 ELSE 0 END AS IsCallOut,
			tas.fnIsOTDueToShiftSpan(c.EmpNo, c.DT) AS IsOTDueToShiftSpan,
			CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalFrom), 108) + ' - ' + CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalTo), 108) AS ArrivalSchedule,
			tas.fmtMIN_HHmm(DATEDIFF(n, c.dtIN, c.dtOUT)) AS TotalWorkDuration,
			tas.fmtMIN_HHmm(DATEDIFF(n, k.ArrivalTo, k.DepartFrom)) AS RequiredWorkDuration,
			CASE WHEN DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) > DATEDIFF(MINUTE, a.OTStartTime_Orig, a.OTEndTime_Orig) AND RTRIM(a.CorrectionCode) NOT IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW') 
				THEN 1 
				ELSE 0 
			END AS IsOTExceedOrig,
			a.OTStartTime_Orig,
			a.OTEndTime_Orig,
			a.IsHold
	From tas.OvertimeRequest a 
		INNER JOIN tas.OvertimeWFTransactionActivity b ON a.OTRequestNo = b.OTRequestNo AND a.TS_AutoID = b.TS_AutoID AND (RTRIM(a.DistListCode) = RTRIM(b.ActionMemberCode) OR ISNULL(a.DistListCode, '') = '')
		INNER JOIN tas.Tran_Timesheet c ON a.EmpNo = c.EmpNo AND a.DT = c.DT AND a.TS_AutoID = c.AutoID
		INNER JOIN tas.Tran_Timesheet_Extra d ON c.AutoID = d.XID_AutoID	
		LEFT JOIN tas.Master_Employee_JDE_View_V2 e on a.EmpNo = e.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE f ON RTRIM(a.CostCenter) = RTRIM(f.BusinessUnit)
		LEFT JOIN tas.Master_OTReasons_JDE g ON RTRIM(d.OTReason) = RTRIM(g.CODE)
		LEFT JOIN tas.Master_Employee_JDE_View h ON a.CurrentlyAssignedEmpNo = h.EmpNo AND a.CurrentlyAssignedEmpNo > 0
		LEFT JOIN tas.OvertimeDistributionMember i ON a.OTRequestNo = i.OTRequestNo AND b.WorkflowTransactionID = i.WorkflowTransactionID
		LEFT JOIN tas.Master_ShiftTimes j ON RTRIM(c.ShiftPatCode) = RTRIM(j.ShiftPatCode) AND RTRIM(c.ShiftCode) = RTRIM(j.ShiftCode)	--Based on the scheduled shift
		LEFT JOIN tas.Master_ShiftTimes k ON RTRIM(c.ShiftPatCode) = RTRIM(k.ShiftPatCode)	--Rev. #1.3 
			AND 
			(
				CASE WHEN (DATEDIFF(MINUTE, CONVERT(TIME, c.dtIN), CONVERT(TIME, j.ArrivalTo)) > c.Duration_Required / 2) OR (c.Duration_Worked_Cumulative >= (c.Duration_Required + (c.Duration_Required / 2)))  
					THEN RTRIM(c.Actual_ShiftCode)
					ELSE RTRIM(c.ShiftCode)
				END
			) = RTRIM(k.ShiftCode)
	WHERE 		
		RTRIM(a.StatusHandlingCode) = 'Open'
		AND 
		(
			(
				@assignTypeID = 0	--Assigned to All
				AND 
				(
					(ISNULL(a.CurrentlyAssignedEmpNo, 0) = @currentUserEmpNo OR ISNULL(i.EmpNo, 0) = @currentUserEmpNo)
					OR  
                    (
						RTRIM(a.CostCenter) IN 
						(
							SELECT RTRIM(PermitCostCenter) 
							FROM tas.syJDE_PermitCostCenter a
								INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = 'TAS3'
								AND PermitEmpNo = @currentUserEmpNo
						)
					)
				)
			) 
			OR
			(
				@assignTypeID = 1	--Assigned to Me
				AND (ISNULL(a.CurrentlyAssignedEmpNo, 0) = @currentUserEmpNo OR ISNULL(i.EmpNo, 0) = @currentUserEmpNo)
			)	
			OR 
			(
				@assignTypeID = 2	--Assigned to Others
				AND 
				(
					(
						@assignedToEmpNo = 0
						AND (ISNULL(a.CurrentlyAssignedEmpNo, 0) <> @currentUserEmpNo AND ISNULL(i.EmpNo, 0) <> @currentUserEmpNo) 
						
					)
					OR 
					(
						@assignedToEmpNo > 0 
						AND (ISNULL(a.CurrentlyAssignedEmpNo, 0) = @assignedToEmpNo OR ISNULL(i.EmpNo, 0) = @assignedToEmpNo)
					)
				)	
				AND RTRIM(a.CostCenter) IN 
				(
					SELECT RTRIM(PermitCostCenter) 
					FROM tas.syJDE_PermitCostCenter a
						INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
					WHERE RTRIM(b.UDCCode) = 'TAS3'
						AND PermitEmpNo = @currentUserEmpNo
				)	
			)
		)
		AND 
		(
			(
				a.DT BETWEEN @startDate AND @endDate 
				AND 
				(@startDate IS NOT NULL AND @endDate IS NOT NULL)
				AND 
				@startDate < @endDate
			)
			OR
			(
				a.DT = @startDate 
				AND 
				(@endDate IS NULL OR @endDate = @startDate)
			)
			OR 
			(@startDate IS NULL AND @endDate IS NULL)
		)
		AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY a.IsHold, a.DT 

GO 

/*	Debugging:
	
PARAMETERS:
	@currentUserEmpNo	INT, 
	@assignTypeID		TINYINT = 0,		--(Note: 0 = All; 1 = Me; 2 = Others)
	@assignedToEmpNo	INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@costCenter			VARCHAR(12) = '',
	@empNo				INT = 0	

	EXEC tas.Pr_GetAssignedOvertimeRequest 10001988, 1, 0, '', '', '', 10003770
	EXEC tas.Pr_GetAssignedOvertimeRequest 10003632, 1			--Currently assigned to me
	EXEC tas.Pr_GetAssignedOvertimeRequest 10001988, 1			--Currently assigned to all	
	EXEC tas.Pr_GetAssignedOvertimeRequest 10003656, 2			--Currently assigned to others

*/

