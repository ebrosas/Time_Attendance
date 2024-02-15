/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetEmployeeFirstSwipeIn
*	Description: Get the employee's first swipe in based on the specified date
*
*	Date			Author		Rev. #		Comments:
*	04/12/2016		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetEmployeeFirstSwipeIn
(
    @empNo	INT,
	@DT		DATETIME
)
RETURNS TIME
AS
BEGIN      

	DECLARE	@returnVal TIME			

	--Get the 
	SELECT @returnVal = CONVERT(TIME, MIN(a.dtIN))
	FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = @empNo
		AND a.DT = @DT
			
	--Return the in/out status
	RETURN @returnVal           
END

/*	Debugging:

	SELECT tas.fnGetEmployeeFirstSwipeIn(10003632, '12/01/2016')

*/
