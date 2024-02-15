	/*****************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfBypassOTApproval
*	Description: This function is used to check if an an approver already approved an OT requisition hence will be bypassed by the workflow engine
*
*	Date			Author		Rev. #		Comments:
*	27/08/2017		Ervin		1.0			Created
**********************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfBypassOTApproval 
(
	@otRequestNo	BIGINT,
	@empNo			INT,
	@actionRole		INT 
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT
	SET @result = 0
    
	IF EXISTS
    (
		SELECT AutoID FROM tas.OvertimeWFApprovalHistory a
		WHERE a.OTRequestNo = @otRequestNo
			AND a.AppCreatedBy = @empNo
			AND a.ActionRole = @actionRole
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnCheckIfBypassOTApproval(1, 10003653, 2) 

*/