/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetLeaveHistory
*	Description: This stored procedure is used to retrieve the employee's leave history
*
*	Date			Author		Revision No.	Comments:
*	01/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetLeaveHistory
(   
	@empNo			INT,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL
)
AS

	--Validate parameters
	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	SELECT	a.EmpNo,			
			a.RequisitionNo AS LeaveNo,
			a.LeaveStartDate, 
			a.LeaveEndDate, 
			a.RequisitionDate,
			a.LeaveType, 
			RTRIM(b.DRDL01) AS LeaveTypeDesc, 
			a.LeaveDuration
	FROM tas.syJDE_LeaveRequisition a
		LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.LeaveType) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) = '58' AND LTRIM(RTRIM(b.DRRT)) = 'VC'
	WHERE a.EmpNo = @empNo
		AND RTRIM(a.ApprovalFlag) NOT IN ('C', 'R', 'D')
		AND 
		(
			(a.LeaveStartDate >= @startDate AND a.LeaveEndDate <= @endDate)
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY a.LeaveStartDate DESC	  

GO 

/*	Debugging:

	EXEC tas.Pr_GetLeaveHistory 10003191, '16/12/2014', '15/01/2015'
	EXEC tas.Pr_GetLeaveHistory 10003632, null, null

*/


