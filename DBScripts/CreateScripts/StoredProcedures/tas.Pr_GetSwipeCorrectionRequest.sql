/*****************************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetSwipeCorrectionRequest
*	Description: This stored procedure is used to fetch the assigned swipe correction request for approval
*
*	Date			Author		Rev.#	Comments:
*	21/07/2015		Ervin		1.0		Created
*	06/10/2015		Ervin		1.1		Refactored the filter condition. Check if record exist in "WorkflowTransactionActivity" table
*	13/10/2015		Ervin		1.2		Refactored the logic when @assignTypeID = 0
*	22/02/2016		Ervin		1.3		Added filter condition that checks if "IsMainGateSwipeRestored" equal to zero
*	20/04/2022		Ervin		1.4		Refactored the code to enhance data retrieval performance
*	07/12/2022		Ervin		1.5		Commented the following filter conditions: d.AutoID = i.TS_AutoID
*	05/01/2023		Ervin		1.6		Refactored the logic in filtering the data by SwipeDate and DT
*	14/01/2023		Ervin		1.7		Changed the value of @CONST_MONTHS_THRESHOLD to 3
*	31/01/2023		Ervin		1.8		Changed the value of @CONST_MONTHS_THRESHOLD to 4
*	01/03/2023		Ervin		1.9		Added filter that checks if IsLastRow = 1
*****************************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetSwipeCorrectionRequest]
(
	@statusCode			varchar(10) = '',	--(Note: Open, Approved, Rejected, Cancelled)
	@assignTypeID		tinyint = 0,		--(Note: 0 = All; 1 = Me; 2 = Others)
	@empNo				int = 0,	
	@costCenter			varchar(12)	= '',
	@startDate			datetime = null,
	@endDate			datetime = null,
	@assignedToEmpNo	int = 0,
	@userEmpNo			int = 0	
)
AS
BEGIN
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
	
	DECLARE	@CONST_MONTHS_THRESHOLD		TINYINT = 4			--Rev. #1.8

	DECLARE @userCostCenter varchar(12)
	SET @userCostCenter = ''
	
	--Initialize parameters
	IF ISNULL(@statusCode, '') = '' --OR UPPER(RTRIM(@statusCode)) = 'ALL STATUS'
		SET @statusCode = NULL

	--IF @assignTypeID NOT IN (1, 2)
	--	SET @assignTypeID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL
		
	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL		

	IF ISNULL(@userEmpNo, 0) > 0
	BEGIN

		--Get the cost center of the current user
		SELECT @userCostCenter = RTRIM(BusinessUnit)
		FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
		WHERE EmpNo = @userEmpNo
	END

	SELECT DISTINCT
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
		a.NetMinutesMG,
		a.NetMinutesWP,
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
		a.IsClosed,
		a.ClosedDate,
		a.StatusID,
		a.StatusCode,
		a.StatusDesc,
		a.StatusHandlingCode,
		a.CurrentlyAssignedEmpNo,
		a.CurrentlyAssignedEmpName,
		a.CurrentlyAssignedEmpEmail,
		a.ServiceProviderTypeCode,
		a.DistListCode,
		d.LeaveType,
		d.RemarkCode,
		d.CorrectionCode,
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
		d.dtIN,
		d.dtOUT,
		d.Duration_Worked_Cumulative,
		d.NetMinutes,
		j.AppApproved,
		j.AppRemarks,
		a.IsSubmittedForApproval,
		a.SubmittedDate,
		a.IsMainGateSwipeRestored
	FROM
		(
			SELECT * FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK)
			WHERE --YEAR(SwipeDate) = YEAR(GETDATE())
				SwipeDate >= CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MONTH, @CONST_MONTHS_THRESHOLD * -1, GETDATE()), 12))		--Rev. #1.6
		) a
		INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BusinessUnit)
		OUTER APPLY
        (
			SELECT * FROM tas.Tran_Timesheet WITH (NOLOCK)
			WHERE --YEAR(DT) = YEAR(GETDATE())
				DT >= CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MONTH, @CONST_MONTHS_THRESHOLD * -1, GETDATE()), 12))				--Rev. #1.6
				AND EmpNo = a.EmpNo
				AND DT = a.SwipeDate
				AND IsLastRow = 1		--Rev. #1.9
		) d
		OUTER APPLY
        (
			SELECT y.* 
			FROM tas.Tran_Timesheet x WITH (NOLOCK)
				INNER JOIN tas.Tran_Timesheet_Extra y WITH (NOLOCK) ON x.AutoID = y.XID_AutoID	
			WHERE --YEAR(x.DT) = YEAR(GETDATE())
				x.DT >= CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MONTH, @CONST_MONTHS_THRESHOLD * -1, GETDATE()), 12))			--Rev. #1.6
				AND x.AutoID = d.AutoID
		) g

		LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(ISNULL(d.Actual_ShiftCode, a.ShiftCode)) = RTRIM(h.ShiftCode)
		INNER JOIN tas.WorkflowTransactionActivity i WITH (NOLOCK) ON a.SwipeID = i.SwipeID /*AND d.AutoID = i.TS_AutoID*/ AND a.SubmittedDate = i.RequestSubmissionDate	--Rev. #1.5
		OUTER APPLY
		(
			SELECT TOP 1 * FROM tas.WorkflowApproval WITH (NOLOCK)
			WHERE SwipeID = a.SwipeID 
				AND TS_AutoID = d.AutoID
				AND RequestSubmissionDate = a.SubmittedDate
				AND a.IsClosed = 1
			ORDER BY AutoID DESC
		) j
		LEFT JOIN tas.Master_Employee_JDE_View k WITH (NOLOCK) ON a.CurrentlyAssignedEmpNo = k.EmpNo AND ISNULL(a.CurrentlyAssignedEmpNo, 0) > 0
	WHERE 
		(
			a.IsSubmittedForApproval = 1
			OR (ISNULL(a.IsSubmittedForApproval, 0) = 0 AND a.IsClosed = 1 AND RTRIM(a.StatusCode) = '110')		--Rejected request
		)
		AND 
		(
			(@statusCode IN ('Open', 'Rejected', 'Cancelled') AND RTRIM(a.StatusHandlingCode) = RTRIM(@statusCode))
			OR
			(@statusCode = 'Approved' AND a.IsClosed = 1)
			OR
			(UPPER(RTRIM(@statusCode)) = 'ALL STATUS' AND RTRIM(a.StatusHandlingCode) IN ('Open', 'Rejected', 'Cancelled', 'Approved', 'Closed'))
			OR 
			@statusCode IS NULL
		)	
		AND
		(
			(
				@assignTypeID = 0 
				AND 
				(
					(@userEmpNo > 0 AND a.CurrentlyAssignedEmpNo = @userEmpNo)
					OR 
					RTRIM(k.BusinessUnit) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TASNEW'
							AND PermitEmpNo = @userEmpNo
					)
				)
			) 
			OR
			(@assignTypeID = 1 AND a.CurrentlyAssignedEmpNo = @userEmpNo) 
			OR 
			(@assignTypeID = 2 AND @assignedToEmpNo > 0 AND a.CurrentlyAssignedEmpNo = @assignedToEmpNo) 
			OR 
			(@assignTypeID = 2 AND ISNULL(@assignedToEmpNo, 0) = 0 AND a.CurrentlyAssignedEmpNo <> @userEmpNo) 
			OR
			@assignTypeID IS NULL
		)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND 
		(
			RTRIM(a.CostCenter) = RTRIM(@costCenter) 
			OR 
			(
				@costCenter IS NULL --AND ISNULL(@userEmpNo, 0) = 0
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
		AND 
		(
			CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			OR (@startDate IS NULL AND @endDate IS NULL)
		)	
		AND ISNULL(a.IsMainGateSwipeRestored, 0) = 0 
	ORDER BY a.IsClosed, a.StatusHandlingCode, a.SwipeDate DESC, a.CostCenter, a.EmpNo

END 

/*	Debug:

PARAMETERS:
	@statusCode			varchar(10) = '',	--(Note: Open, Approved, Rejected, Cancelled)
	@assignTypeID		tinyint = 0,		--(Note: 0 = All; 1 = Me; 2 = Others)
	@empNo				int = 0,	
	@costCenter			varchar(12)	= '',
	@startDate			datetime = null,
	@endDate			datetime = null,
	@assignedToEmpNo	int = 0,
	@userEmpNo			int = 0	

	EXEC tas.Pr_GetSwipeCorrectionRequest 'Open', 0, 10003463, '', NULL, NULL, 0, 10003632

*/

