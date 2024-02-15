/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetOTApprovalHistory
*	Description: Get the overtime approval history records
*
*	Date			Author		Revision No.	Comments:
*	14/08/2017		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetOTApprovalHistory
(   
	@otRequestNo				BIGINT,
	@tsAutoID					INT = 0,
	@requestSubmissionDate		DATETIME = NULL
)
AS

	--Validate parameters
	IF ISNULL(@tsAutoID, 0) = 0
		SET @tsAutoID = NULL

	IF ISNULL(@requestSubmissionDate, '') = '' OR CONVERT(DATETIME, '') = @requestSubmissionDate
		SET @requestSubmissionDate = NULL

	SELECT	AutoID,
			OTRequestNo,
			TS_AutoID,
			RequestSubmissionDate,
			AppApproved,
			AppRemarks,
			AppRoutineSeq,
			AppCreatedBy,
			AppCreatedName,
			AppCreatedDate,
			RTRIM(b.Position) AS AppCreatedPosition,
			AppModifiedBy,
			AppModifiedName,
			AppModifiedDate,
			ApprovalRole
	FROM tas.OvertimeWFApprovalHistory a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.AppCreatedBy = b.EmpNo
	WHERE a.OTRequestNo = @otRequestNo
		AND (a.TS_AutoID = @tsAutoID OR @tsAutoID IS NULL)
		AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
	ORDER BY a.AutoID DESC

GO

/*	Debugging:
	
PARAMETERS:
	@otRequestNo				BIGINT,
	@tsAutoID					INT,
	@requestSubmissionDate		DATETIME

	EXEC tas.Pr_GetOTApprovalHistory 10

*/

