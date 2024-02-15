/***********************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfMutipleGateEntry
*	Description: This function is used to check if an employee has multiple records in the Timesheet on the same day
*
*	Date			Author		Rev. #		Comments:
*	08/08/2017		Ervin		1.0			Created
***********************************************************************************************************************/

CREATE FUNCTION tas.fnCheckIfMutipleGateEntry 
(
	@empNo			INT,
	@processDate	DATETIME 
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT
	SET @result = 0
    
	IF 
	(
		SELECT COUNT(AutoID) FROM tas.Tran_Timesheet a
		WHERE a.EmpNo = @empNo
			AND a.DT = @processDate
	) > 1
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnCheckIfMutipleGateEntry(10001766, '07/23/2017') 

*/