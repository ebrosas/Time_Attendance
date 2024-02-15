/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckApprovedWorkplaceSwipe
*	Description: Check if attendance record has related workplace swipe correction
*
*	Date			Author		Rev. #		Comments:
*	06/02/2017		Ervin		1.0			Created
*************************************************************************************************************************************************/

CREATE FUNCTION tas.fnCheckApprovedWorkplaceSwipe
(
	@empNo				INT,
	@attendanceDate		DATETIME
)
RETURNS @returnTable 
TABLE 
(
	EmployeeNo			INT,
	HasApprovedSwipe	BIT 
) 
AS     
BEGIN 

	DECLARE @isApprovedSwipe BIT
    SET @isApprovedSwipe = 0

	IF EXISTS
    (
		SELECT a.SwipeID FROM tas.Tran_WorkplaceSwipe a
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @attendanceDate
			AND a.IsCorrected = 1
			AND a.IsClosed = 1
			AND a.CorrectionType > 0
			AND RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')
	)
	BEGIN
    
		--Set the flag
		SET @isApprovedSwipe = 1
	END 

	INSERT @returnTable
	SELECT @empNo, @isApprovedSwipe

	RETURN 
END


/*	Debugging:

	SELECT * from tas.fnCheckApprovedWorkplaceSwipe(10006040, '02/02/2017')

*/
