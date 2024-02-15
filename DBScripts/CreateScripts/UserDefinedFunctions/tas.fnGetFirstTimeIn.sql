/***********************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetFirstTimeIn
*	Description: This function is used to get the first time-in of an employee on specific date
*
*	Date			Author		Rev. #		Comments:
*	11/15/2017		Ervin		1.0			Created
*	23/11/2017		Ervin		1.1			Set the ORDER BY clause to use "dtIN" field
***********************************************************************************************************************/

ALTER FUNCTION tas.fnGetFirstTimeIn 
(
	@empNo				INT,
	@attendanceDate		DATETIME 
)
RETURNS DATETIME 
AS
BEGIN

    DECLARE	@result DATETIME

	SELECT TOP 1 @result = a.dtIN 
	FROM tas.Tran_Timesheet a 
	WHERE a.EmpNo = @empNo
		AND a.DT= @attendanceDate 
	ORDER BY a.dtIN

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnGetFirstTimeIn(10003374, '11/22/2017') 

*/