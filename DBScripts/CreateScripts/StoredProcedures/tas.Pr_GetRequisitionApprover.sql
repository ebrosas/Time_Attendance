/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetRequisitionApprover
*	Description: Get the approvers who had approved the overtime request
*
*	Date			Author		Revision No.	Comments:
*	15/08/2017		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetRequisitionApprover
(   
	@otRequestNo			BIGINT,
	@tsAutoID				INT = 0,
	@requestSubmissionDate	DATETIME = NULL
)
AS

	--Validate parameters
	IF ISNULL(@tsAutoID, 0) = 0
		SET @tsAutoID = NULL

	IF ISNULL(@requestSubmissionDate, '') = '' OR CONVERT(DATETIME, '') = @requestSubmissionDate
		SET @requestSubmissionDate = NULL

	SELECT	DISTINCT
			a.AppCreatedBy AS ApprovedByEmpNo,
			RTRIM(a.AppCreatedName) AS ApprovedByEmpName,
			LTRIM(RTRIM(ISNULL(b.EAEMAL, ''))) AS ApprovedByEmpEmail
	FROM tas.OvertimeWFApprovalHistory a
		LEFT JOIN tas.syJDE_F01151 b ON a.AppCreatedBy = CAST(b.EAAN8 AS INT) AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' --AND f.EAEHIER = 1 
	WHERE a.AppApproved = 1
		AND a.OTRequestNo = @otRequestNo
		AND (a.TS_AutoID = @tsAutoID OR @tsAutoID IS NULL)
		AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
	ORDER BY a.AppCreatedBy

GO 

/*	Debug:

PARAMATERS:
	@otRequestNo			BIGINT,
	@tsAutoID				INT = 0,
	@requestSubmissionDate	DATETIME = NULL	

	EXEC tas.Pr_GetRequisitionApprover 3

*/





	