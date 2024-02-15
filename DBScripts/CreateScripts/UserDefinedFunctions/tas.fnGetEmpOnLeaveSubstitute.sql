/*************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetEmpOnLeaveSubstitute
*	Description: Retrieves count of records
*
*	Date:			Author:		Rev.#:		Comments:
*	18/08/2016		Ervin		1.0			Created
**************************************************************************************************************************************************/

CREATE FUNCTION tas.fnGetEmpOnLeaveSubstitute
(
	@empNo			INT,	
	@costCenter		VARCHAR(12),
	@wfAppCode		VARCHAR(20)
)
RETURNS int
AS

BEGIN

	DECLARE @substituteEmpNo	INT
	SELECT	@substituteEmpNo	= 0
	
	--Check if employee is on-leave
	IF EXISTS 
	(
		SELECT EmpNo FROM tas.Vw_EmployeeAvailability
		WHERE EmpNo = @empNo 
			AND CONVERT(DATETIME, GETDATE(), 101) BETWEEN FromDate AND ToDate
	)
	BEGIN

		--Get the substitute if it is defined in the Leave Requisition System
		SELECT @substituteEmpNo = SubEmpNo
		FROM tas.syJDE_LeaveRequisition 
		WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
			AND EmpNo = @substituteEmpNo
			AND RTRIM(LeaveType) = 'AL' 
			AND CONVERT(DATETIME, CONVERT(VARCHAR(10), GETDATE(), 101)) BETWEEN LeaveStartDate and LeaveEndDate 

		IF ISNULL(@substituteEmpNo, 0) = 0

			--No Substitute is defined in the leave Requisition, so get the substitute in the ISMS system
			SELECT	@substituteEmpNo = SubstituteEmpNo
			FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@empNo, @wfAppCode, @costCenter)
	END

	RETURN @substituteEmpNo
END

GO

/*	Debugging:

	SELECT tas.fnGetEmpOnLeaveSubstitute(10003662, '7600', 'WFEPA')

*/
