/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetLastSwipeOut
*	Description: Get the last swipe out at the Main Gate
*
*	Date			Author		Rev. #		Comments:
*	05/04/2016		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetLastSwipeOut
(
    @empNo	INT
)
RETURNS DATETIME
AS
BEGIN      

	DECLARE	@returnVal	DATETIME			

	--Get the 
	SELECT @returnVal = MAX(AttendanceDate)
	FROM tas.Master_EmployeeAttendance a
	WHERE a.EmployeeNo = @empNo
			
	--Return the in/out status
	RETURN @returnVal           
END

/*	Debugging:

	SELECT tas.fnGetLastSwipeOut(10003632)

*/
