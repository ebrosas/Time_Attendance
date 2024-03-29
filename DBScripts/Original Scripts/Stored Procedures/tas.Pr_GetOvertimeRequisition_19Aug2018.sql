USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetOvertimeRequisition]    Script Date: 19/08/2018 11:41:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetOvertimeRequisition
*	Description: Get overtime requisitions
*
*	Date			Author		Revision No.	Comments:
*	15/08/2017		Ervin		1.0				Created
*	06/09/2017		Ervin		1.1				Added extra logic in fetching the Approval Level description that fecth data using the "fnGetCurrentApprovalLevel" function
*	11/09/2017		Ervin		1.2				Added "IsArrivedEarly" and "ArrivalSchedule" fields in the returned recordset
*	18/09/2017		Ervin		1.3				Refactored the logic in identifying the Shift Code to use
*	21/12/2017		Ervin		1.4				Refactored the logic when searching for records filtered by "@assignedToEmpNo"
*	24/12/2017		Ervin		1.5				Added "IsOTCommentModified" field
*	21/01/2018		Ervin		1.6				Set the following order sequence: 1) IsHold; 2) DT 
*	24/05/2018		Ervin		1.7				Added "IsOTRamadanExceedLimit" field in all load types
*	07/08/2018		Ervin		1.8				Added WIHT (NOLOCK) clause to enhance data retrieval performance
******************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetOvertimeRequisition]
(   
	@currentUserEmpNo	INT,
	@otRequestNo		BIGINT = 0,
	@empNo				INT = 0,	
	@costCenter			VARCHAR(12) = '',
	@createdByEmpNo		INT = 0,
	@assignedToEmpNo	INT = 0,
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@statusCode			VARCHAR(10) = NULL
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Validate parameters
	IF ISNULL(@otRequestNo, 0) = 0
		SET @otRequestNo = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@createdByEmpNo, 0) = 0
		SET @createdByEmpNo = NULL

	IF ISNULL(@assignedToEmpNo, 0) = 0
		SET @assignedToEmpNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@statusCode, '') = '' OR UPPER(RTRIM(@statusCode)) = 'ALL STATUS'
		SET @statusCode = NULL
	ELSE IF RTRIM(@statusCode) = 'Approved'
		SET @statusCode = 'Closed'

	IF @otRequestNo > 0		--Filter by the Identity ID
	BEGIN

			SELECT	DISTINCT
				a.OTRequestNo,			
				a.TS_AutoID,
				a.DT,
				a.EmpNo,
				e.EmpName,
				a.CostCenter,
				RTRIM(f.BusinessUnitName) AS CostCenterName,
				e.Position,
				e.GradeCode,
				c.ShiftPatCode,
				c.ShiftCode,
				c.Actual_ShiftCode,
				c.dtIN,
				c.dtOUT,
				CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
					THEN 
						CASE WHEN a.OTApproved = 'N' 
							THEN d.OTStartTime
							ELSE a.OTStartTime
						END 
					ELSE d.OTStartTime
				END AS OTStartTime,
				CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
					THEN 
						CASE WHEN a.OTApproved = 'N' 
							THEN d.OTEndTime
							ELSE a.OTEndTime
						END
					ELSE d.OTEndTime
				END AS OTEndTime,
				d.OTType,
				CASE WHEN a.OTApproved = 'N' 
					THEN 0 
					ELSE 
						CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
							THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
							ELSE DATEDIFF(n, d.OTStartTime, d.OTEndTime) 
						END 
				END AS OTDurationMinute,

				CASE WHEN a.OTApproved = 'N' 
					THEN 0 
					ELSE 
						CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
							THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
							ELSE tas.fmtMIN_HHmm(DATEDIFF(n, d.OTStartTime, d.OTEndTime)) 
						END 
				END AS OTDurationHour,	
				a.MealVoucherEligibility,
				d.Approved,
				RTRIM(a.OTComment) AS Comment,
				d.OTApproved,
				d.OTReason AS OTReasonCode,
				g.[DESCRIPTION] AS OTReason,				
				c.AutoID,
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
				c.Processed,
				tas.fnIsOTDueToShiftSpan(c.EmpNo, c.DT) AS IsOTDueToShiftSpan,
				tas.fnCheckIfArrivedEarly(a.EmpNo, a.DT) AS IsArrivedEarly,
				CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalFrom), 108) + ' - ' + CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalTo), 108) AS ArrivalSchedule,
				tas.fmtMIN_HHmm(DATEDIFF(n, c.dtIN, c.dtOUT)) AS TotalWorkDuration,
				tas.fmtMIN_HHmm(DATEDIFF(n, k.ArrivalTo, k.DepartFrom)) AS RequiredWorkDuration,
				CASE WHEN DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) > DATEDIFF(MINUTE, a.OTStartTime_Orig, a.OTEndTime_Orig) AND RTRIM(a.CorrectionCode) NOT IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW') 
					THEN 1 
					ELSE 0 
				END AS IsOTExceedOrig,
				a.IsOTCommentModified,	--Rev. #1.5				
				a.IsHold,
				CASE WHEN c.IsRamadan = 1 AND c.isMuslim = 1 AND RTRIM(c.ShiftCode) <> 'O' 
					AND (CASE WHEN a.OTApproved = 'N' 
							THEN 0 
							ELSE 
								CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
									THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
									ELSE DATEDIFF(n, d.OTStartTime, d.OTEndTime) 
								END 
						END) > 120
					THEN 1
					ELSE 0
				END AS IsOTRamadanExceedLimit	--Rev. #1.7		
		From tas.OvertimeRequest a WITH (NOLOCK) 
			LEFT JOIN tas.OvertimeWFTransactionActivity b WITH (NOLOCK) ON a.OTRequestNo = b.OTRequestNo AND a.TS_AutoID = b.TS_AutoID AND (RTRIM(a.DistListCode) = RTRIM(b.ActionMemberCode) OR ISNULL(a.DistListCode, '') = '')
			INNER JOIN tas.Tran_Timesheet c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DT AND a.TS_AutoID = c.AutoID
			INNER JOIN tas.Tran_Timesheet_Extra d WITH (NOLOCK) ON c.AutoID = d.XID_AutoID	
			LEFT JOIN tas.Master_Employee_JDE_View_V2 e WITH (NOLOCK) on a.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE f WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(f.BusinessUnit)
			LEFT JOIN tas.Master_OTReasons_JDE g WITH (NOLOCK) ON RTRIM(d.OTReason) = RTRIM(g.CODE)
			LEFT JOIN tas.Master_Employee_JDE_View h WITH (NOLOCK) ON a.CurrentlyAssignedEmpNo = h.EmpNo AND a.CurrentlyAssignedEmpNo > 0
			LEFT JOIN tas.OvertimeDistributionMember i WITH (NOLOCK) ON a.OTRequestNo = i.OTRequestNo AND b.WorkflowTransactionID = i.WorkflowTransactionID
			LEFT JOIN tas.Master_ShiftTimes j WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(j.ShiftPatCode) AND RTRIM(c.ShiftCode) = RTRIM(j.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes k WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(k.ShiftPatCode)	--Rev. #1.3 
				AND 
				(
					CASE WHEN (DATEDIFF(MINUTE, CONVERT(TIME, c.dtIN), CONVERT(TIME, j.ArrivalTo)) > c.Duration_Required / 2) OR (c.Duration_Worked_Cumulative >= (c.Duration_Required + (c.Duration_Required / 2)))  
						THEN RTRIM(c.Actual_ShiftCode)
						ELSE RTRIM(c.ShiftCode)
					END
				) = RTRIM(k.ShiftCode)
		WHERE a.OTRequestNo = @otRequestNo 
			AND RTRIM(a.CostCenter) IN 
			(
				SELECT RTRIM(PermitCostCenter) 
				FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
					INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
				WHERE RTRIM(b.UDCCode) = 'TAS3'
					AND PermitEmpNo = @currentUserEmpNo
			)
		ORDER BY a.IsHold, a.DT		--Rev. #1.6
    END 

	ELSE
    BEGIN
    
		SELECT	DISTINCT
				a.OTRequestNo,			
				a.TS_AutoID,
				a.DT,
				a.EmpNo,
				e.EmpName,
				a.CostCenter,
				RTRIM(f.BusinessUnitName) AS CostCenterName,
				e.Position,
				e.GradeCode,
				c.ShiftPatCode,
				c.ShiftCode,
				c.Actual_ShiftCode,
				c.dtIN,
				c.dtOUT,
				CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
					THEN 
						CASE WHEN a.OTApproved = 'N' 
							THEN d.OTStartTime
							ELSE a.OTStartTime
						END 
					ELSE d.OTStartTime
				END AS OTStartTime,
				CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
					THEN 
						CASE WHEN a.OTApproved = 'N' 
							THEN d.OTEndTime
							ELSE a.OTEndTime
						END
					ELSE d.OTEndTime
				END AS OTEndTime,
				d.OTType,

				CASE WHEN a.OTApproved = 'N' 
					THEN 0 
					ELSE 
						CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
							THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
							ELSE DATEDIFF(n, d.OTStartTime, d.OTEndTime) 
						END 
				END AS OTDurationMinute,

				CASE WHEN a.OTApproved = 'N' 
					THEN 0 
					ELSE 
						CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
							THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
							ELSE tas.fmtMIN_HHmm(DATEDIFF(n, d.OTStartTime, d.OTEndTime)) 
						END 
				END AS OTDurationHour,								

				a.MealVoucherEligibility,
				d.Approved,
				RTRIM(a.OTComment) AS Comment,
				d.OTApproved,
				d.OTReason AS OTReasonCode,
				g.[DESCRIPTION] AS OTReason,				
				c.AutoID,
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
				c.Processed,
				tas.fnIsOTDueToShiftSpan(c.EmpNo, c.DT) AS IsOTDueToShiftSpan,
				tas.fnCheckIfArrivedEarly(a.EmpNo, a.DT) AS IsArrivedEarly,
				CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalFrom), 108) + ' - ' + CONVERT(VARCHAR, CONVERT(TIME, k.ArrivalTo), 108) AS ArrivalSchedule,
				tas.fmtMIN_HHmm(DATEDIFF(n, c.dtIN, c.dtOUT)) AS TotalWorkDuration,
				tas.fmtMIN_HHmm(DATEDIFF(n, k.ArrivalTo, k.DepartFrom)) AS RequiredWorkDuration,
				CASE WHEN DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) > DATEDIFF(MINUTE, a.OTStartTime_Orig, a.OTEndTime_Orig) AND RTRIM(a.CorrectionCode) NOT IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW') 
					THEN 1 
					ELSE 0 
				END AS IsOTExceedOrig,
				a.IsOTCommentModified,	--Rev. #1.5
				a.IsHold,
				CASE WHEN c.IsRamadan = 1 AND c.isMuslim = 1 AND RTRIM(c.ShiftCode) <> 'O' 
					AND (CASE WHEN a.OTApproved = 'N' 
							THEN 0 
							ELSE 
								CASE WHEN RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
									THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
									ELSE DATEDIFF(n, d.OTStartTime, d.OTEndTime) 
								END 
						END) > 120
					THEN 1
					ELSE 0
				END AS IsOTRamadanExceedLimit	--Rev. #1.7	
		From tas.OvertimeRequest a WITH (NOLOCK) 
			LEFT JOIN tas.OvertimeWFTransactionActivity b WITH (NOLOCK) ON a.OTRequestNo = b.OTRequestNo AND a.TS_AutoID = b.TS_AutoID AND (RTRIM(a.DistListCode) = RTRIM(b.ActionMemberCode) OR ISNULL(a.DistListCode, '') = '')
			INNER JOIN tas.Tran_Timesheet c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DT AND a.TS_AutoID = c.AutoID
			INNER JOIN tas.Tran_Timesheet_Extra d WITH (NOLOCK) ON c.AutoID = d.XID_AutoID	
			LEFT JOIN tas.Master_Employee_JDE_View_V2 e WITH (NOLOCK) on a.EmpNo = e.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE f WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(f.BusinessUnit)
			LEFT JOIN tas.Master_OTReasons_JDE g WITH (NOLOCK) ON RTRIM(d.OTReason) = RTRIM(g.CODE)
			LEFT JOIN tas.Master_Employee_JDE_View h WITH (NOLOCK) ON a.CurrentlyAssignedEmpNo = h.EmpNo AND a.CurrentlyAssignedEmpNo > 0
			LEFT JOIN tas.OvertimeDistributionMember i WITH (NOLOCK) ON a.OTRequestNo = i.OTRequestNo AND b.WorkflowTransactionID = i.WorkflowTransactionID
			LEFT JOIN tas.Master_ShiftTimes j WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(j.ShiftPatCode) AND RTRIM(c.ShiftCode) = RTRIM(j.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes k WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(k.ShiftPatCode)	--Rev. #1.3
				AND 
				(
					CASE WHEN (DATEDIFF(MINUTE, CONVERT(TIME, c.dtIN), CONVERT(TIME, j.ArrivalTo)) > c.Duration_Required / 2) OR (c.Duration_Worked_Cumulative >= (c.Duration_Required + (c.Duration_Required / 2)))  
						THEN RTRIM(c.Actual_ShiftCode)
						ELSE RTRIM(c.ShiftCode)
					END
				) = RTRIM(k.ShiftCode)
		WHERE 		
			(a.OTRequestNo = @otRequestNo OR @otRequestNo IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				(RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND RTRIM(a.CostCenter) IN 
				(
					SELECT RTRIM(PermitCostCenter) 
					FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
						INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
					WHERE RTRIM(b.UDCCode) = 'TAS3'
						AND PermitEmpNo = @currentUserEmpNo
				)
			)
			AND (a.CreatedByEmpNo = @createdByEmpNo OR @createdByEmpNo IS NULL)
			AND 
			(
				(
					(a.CurrentlyAssignedEmpNo = @assignedToEmpNo OR (i.EmpNo = @assignedToEmpNo AND RTRIM(a.StatusCode) <> '122'))	--Rev. #1.4
					AND @assignedToEmpNo > 0
				)
				OR @assignedToEmpNo IS NULL
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
			AND (RTRIM(a.StatusHandlingCode) = RTRIM(@statusCode) OR @statusCode IS NULL)
		ORDER BY a.IsHold, a.DT		--Rev. #1.6
	END 

