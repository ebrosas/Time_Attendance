/*****************************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetOvertimeWFEmailDelivery
*	Description: This stored procedure is used to get the workflow emails pending for delivery
*
*	Date			Author		Rev.#		Comments:
*	01/08/2017		Ervin		1.0			Created
*	31/12/2017		Ervin		1.1			Added @actionType 3 & 4 that will be used in the "Overtime Approval Notification Service"
*****************************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetOvertimeWFEmailDelivery
(
	@actionType			tinyint,	--(Note: 1 -> Get all assignees; 2 -> Get records by assignee)
	@createdByEmpNo		int = 0,
	@assignedToEmpNo	int = 0,	
	@startDate			datetime = null,
	@endDate			datetime = null
)
AS
	
	--Initialize parameters
	IF ISNULL(@createdByEmpNo, 0) = 0
		SET @createdByEmpNo = NULL

	IF ISNULL(@assignedToEmpNo, 0) = 0
		SET @assignedToEmpNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL
		
	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL		

	IF @actionType = 1		--Get all assignees
	BEGIN

		SELECT DISTINCT
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.EmailSourceName,
			a.EmailCCRecipient,
			a.EmailCCRecipientType,
			a.IsDelivered	
		FROM tas.OvertimeWFEmailDelivery a
		WHERE 
			ISNULL(a.IsDelivered, 0) = 0
			AND (a.CreatedByEmpNo = @createdByEmpNo OR @createdByEmpNo IS NULL)
			AND (a.CurrentlyAssignedEmpNo = @assignedToEmpNo OR @assignedToEmpNo IS NULL)
			AND 
			(
				CONVERT(VARCHAR, a.CreatedDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
		ORDER BY a.CurrentlyAssignedEmpNo
	END

	ELSE IF @actionType = 2		--Get employee information by assignee
	BEGIN
	
		SELECT 
			a.DeliveryID,
			a.OTRequestNo,
			a.TS_AutoID,
			a.RequestSubmissionDate,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			a.ActivityCode,
			a.ActionMemberCode,
			a.EmailSourceName,
			a.EmailCCRecipient,
			a.EmailCCRecipientType,
			a.IsDelivered,
			a.CreatedByEmpNo,
			a.CreatedByEmpName,
			a.CreatedDate,
			b.EmpNo,
			c.EmpName,
			c.GradeCode, 
			b.CostCenter,
			RTRIM(d.BusinessUnitName) AS CostCenterName,
			c.Position,
			b.DT,
			b.OTStartTime,
			b.OTEndTime,
			b.OTType,
			b.CorrectionCode,
			e.[DESCRIPTION] AS CorrectionDesc,
			b.MealVoucherEligibility,
			b.OTApproved,
			f.ShiftPatCode,
			f.ShiftCode,
			f.Actual_ShiftCode,
			b.OTComment
		FROM tas.OvertimeWFEmailDelivery a
			INNER JOIN tas.OvertimeRequest b ON a.OTRequestNo = b.OTRequestNo
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON b.EmpNo = c.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE d ON RTRIM(b.CostCenter) = RTRIM(d.BusinessUnit)
			LEFT JOIN tas.Master_OTReasons_JDE e ON RTRIM(b.CorrectionCode) = LTRIM(RTRIM(e.CODE))
			LEFT JOIN tas.Tran_Timesheet f ON b.TS_AutoID = f.AutoID
		WHERE 
			ISNULL(a.IsDelivered, 0) = 0
			AND (a.CurrentlyAssignedEmpNo = @assignedToEmpNo OR @assignedToEmpNo IS NULL)
			AND 
			(
				CONVERT(VARCHAR, a.CreatedDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
		ORDER BY b.OTRequestNo DESC
	END

	ELSE IF @actionType = 3		--Get all assignees that will be sent through the "Overtime Notification Service"
	BEGIN

		SELECT DISTINCT
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			tas.fnOTRequestCreator(a.CurrentlyAssignedEmpNo, @startDate, @endDate) AS EmailCCRecipient
		FROM tas.OvertimeWFEmailDelivery a
			INNER JOIN tas.OvertimeRequest b ON a.OTRequestNo = b.OTRequestNo
		WHERE 
			RTRIM(b.StatusHandlingCode) = 'Open'
			AND ISNULL(a.IsDelivered, 0) = 0
			AND (a.CreatedByEmpNo = @createdByEmpNo OR @createdByEmpNo IS NULL)
			AND (a.CurrentlyAssignedEmpNo = @assignedToEmpNo OR @assignedToEmpNo IS NULL)
			AND 
			(
				CONVERT(VARCHAR, a.CreatedDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
		ORDER BY a.CurrentlyAssignedEmpNo
	END

	ELSE IF @actionType = 4		--Get OT requisitions filtered by specific approver that will be sent through the "Overtime Notification Service"
	BEGIN

		SELECT
			a.DeliveryID,
			a.OTRequestNo,
			a.CurrentlyAssignedEmpNo,
			a.CurrentlyAssignedEmpName,
			a.CurrentlyAssignedEmpEmail,
			b.EmpNo,
			c.EmpName,
			b.CostCenter,
			RTRIM(d.BUname) AS CostCenterName,
			b.DT,
			CASE WHEN b.OTApproved = 'Y' THEN 'Yes' ELSE 'No' END AS IsOTApproved,
			b.OTStartTime,
			b.OTEndTime,
			CASE WHEN b.OTApproved = 'N' THEN '-' ELSE tas.fnConvertMinuteToHourString(DATEDIFF(n, b.OTStartTime, b.OTEndTime)) END AS OTDuration,
			RTRIM(b.OTReason) AS OTReasonCode,
			e.[DESCRIPTION] AS OTReason,	
			b.OTComment
		FROM tas.OvertimeWFEmailDelivery a
			INNER JOIN tas.OvertimeRequest b ON a.OTRequestNo = b.OTRequestNo
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON b.EmpNo = c.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view d ON RTRIM(b.CostCenter) = RTRIM(d.BU)
			LEFT JOIN tas.Master_OTReasons_JDE e ON RTRIM(b.OTReason) = RTRIM(e.CODE)
		WHERE 
			RTRIM(b.StatusHandlingCode) = 'Open'
			AND ISNULL(a.IsDelivered, 0) = 0
			AND (a.CreatedByEmpNo = @createdByEmpNo OR @createdByEmpNo IS NULL)
			AND (a.CurrentlyAssignedEmpNo = @assignedToEmpNo OR @assignedToEmpNo IS NULL)
			AND 
			(
				CONVERT(VARCHAR, a.CreatedDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
				OR (@startDate IS NULL AND @endDate IS NULL)
			)
		ORDER BY a.CurrentlyAssignedEmpNo
	END


GO

/*	Debugging:

PARAMETERS:
	@actionType			tinyint,	--(Note: 1 -> Get all assignees; 2 -> Get records by assignee)
	@createdByEmpNo		int = 0,
	@assignedToEmpNo	int = 0,	
	@startDate			datetime = null,
	@endDate			datetime = null

	
	EXEC tas.Pr_GetOvertimeWFEmailDelivery 1
	EXEC tas.Pr_GetOvertimeWFEmailDelivery 2

	EXEC tas.Pr_GetOvertimeWFEmailDelivery 3, 0, 0, '02/01/2018', '03/01/2018'
	EXEC tas.Pr_GetOvertimeWFEmailDelivery 4, 0, 10003632, '02/01/2018', '03/01/2018'

*/
