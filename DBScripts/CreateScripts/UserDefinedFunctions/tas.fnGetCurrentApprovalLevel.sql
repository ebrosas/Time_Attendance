/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetCurrentApprovalLevel
*	Description: This UDF is used to get the approval level description of the current WF activity
*
*	Date:			Author:		Rev.#:		Comments:
*	06/09/2017		Ervin		1.0			Created
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetCurrentApprovalLevel
(
	@otRequestNo	BIGINT
)
RETURNS VARCHAR(500)
AS
BEGIN

	DECLARE	@result VARCHAR(500)

	SELECT TOP 1 @result = RTRIM(a.ActivityDesc2) 
	FROM tas.OvertimeWFTransactionActivity a
	WHERE OTRequestNo = @otRequestNo
		AND IsCurrent = 1
		AND ISNULL(IsCompleted, 0) = 0

	RETURN RTRIM(@result)

END

/*	Testing:

PARAMETERS:
	@otRequestNo			BIGINT	

	SELECT tas.fnGetCurrentApprovalLevel(4)

*/
