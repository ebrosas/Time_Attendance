/*********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetLastPermanentShiftPattern
*	Description: This function is used to get the last permanent shift pattern code assigned to an employee
*
*	Date			Author		Rev. #		Comments:
*	01/06/2019		Ervin		1.0			Created
***********************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetLastPermanentShiftPattern 
(
	@empNo	INT
)
RETURNS VARCHAR(2) 
AS
BEGIN

    DECLARE	@shiftPatCode VARCHAR(2) 

	SELECT TOP 1 @shiftPatCode = RTRIM(a.ShiftPatCode) 
	FROM tas.Tran_ShiftPatternChanges a WITH(NOLOCK)
	WHERE a.EmpNo = @empNo
		AND RTRIM(a.ChangeType) = 'D'
	ORDER BY a.LastUpdateTime DESC

	RETURN @shiftPatCode
END


/*	Debugging:

	SELECT tas.fnGetLastPermanentShiftPattern(10006023) 

	SELECT * FROM tas.Tran_ShiftPatternChanges a
	WHERE a.EmpNo = 10006023

	SELECT * FROM tas.Tran_ShiftPatternUpdates a
	WHERE a.EmpNo = 10006023
	ORDER BY a.DateX DESC

	BEGIN TRAN T1

	UPDATE tas.Tran_ShiftPatternChanges 
	SET ChangeType = 'D'
	WHERE EmpNo = 10006023
		AND AutoID = 1062

	COMMIT TRAN T

*/