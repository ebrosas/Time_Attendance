/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetLeaveToOffsetAbsent
*	Description: This functions fetched the leave requisition information based on the speicifed employee no. and date
*
*	Date:			Author:		Rev.#:		Comments:
*	08/04/2019		Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetLeaveToOffsetAbsent
(
	@empNo				INT,
	@attendanceDate		DATETIME
)
RETURNS  @rtnTable TABLE  
(     
	RequisitionNo		INT,
	LeaveStartDate		DATETIME,
	LeaveEndDate		DATETIME
) 
AS
BEGIN

	DECLARE @requisitionNo		INT,
			@leaveStartDate		DATETIME,
			@leaveEndDate		DATETIME

	SELECT	@requisitionNo		= 0,
			@leaveStartDate		= NULL,
			@leaveEndDate		= NULL

	SELECT	@requisitionNo = a.RequisitionNo,
			@leaveStartDate = a.LeaveStartDate,
			@leaveEndDate = LeaveEndDate 
	FROM tas.sy_LeaveRequisition a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND @attendanceDate BETWEEN LeaveStartDate AND LeaveEndDate
		AND ApprovalFlag NOT IN ('D', 'C', 'R')

	INSERT INTO @rtnTable 
	SELECT @requisitionNo, @leaveStartDate, @leaveEndDate

	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@empNo				INT,
	@attendanceDate		DATETIME

	SELECT * FROM tas.fnGetLeaveToOffsetAbsent(10001414, '04/07/2019')

*/
