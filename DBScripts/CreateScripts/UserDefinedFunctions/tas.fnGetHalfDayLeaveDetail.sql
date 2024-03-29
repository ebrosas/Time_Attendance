/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetHalfDayLeaveDetail
*	Description: This function is used to get the hald day leave details
*
*	Date:			Author:		Rev.#:		Comments:
*	27/11/2019		Ervin		1.0			Created
*	05/02/2020		Ervin		1.1			Refactored the logic to fix the wrong attendance status displayed in the Attendance Dashboard form in TAS
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetHalfDayLeaveDetail
(
	@empNo		INT,
	@leaveDate	DATETIME
)
RETURNS VARCHAR(100)
AS
BEGIN

	DECLARE @leaveDescription VARCHAR(100) = ''

	IF EXISTS
    (
		SELECT 1 
		FROM tas.sy_LeaveRequisition2 a WITH (NOLOCK)
			INNER JOIN tas.sy_LeaveRequisitionDetail b WITH (NOLOCK) ON a.RequisitionNo = b.LeaveNo AND a.EmpNo = b.LeaveEmpNo
		WHERE a.EmpNo = @empNo
			AND @leaveDate BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.LeaveStartDate, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, a.LeaveEndDate, 12))
			AND RTRIM(a.HalfDayLeaveFlag) IN 
			(
				'1',	--Half Day on Leave Start Date 
				'2',	--Half Day on Leave Resume Date 
				'3'		--Half Day on both Leave Start Date and Leave Resume date  
			)
			AND RTRIM(a.ApprovalFlag) IN 
			(
				'A',	--Approved / Paid
				'N'		--Approved / Not Paid
			)
	)
	BEGIN

		SELECT @leaveDescription = CASE 
										WHEN 
											(
												(RTRIM(b.LeaveStartType) = 'LVSTARTHD1' AND a.LeaveDuration = 0.5)
												OR 
												(RTRIM(b.LeaveStartType) = 'LVSTARTHD1' AND a.LeaveDuration > 0.5 AND @leaveDate = a.LeaveStartDate)
												OR 
												(RTRIM(b.LeaveResumeType) = 'LVRESMEHD1' AND a.LeaveDuration > 0.5 AND @leaveDate = a.LeaveResumeDate)
											) THEN 'Half Day Leave - First half of the day'
										WHEN 
											(
												(RTRIM(b.LeaveStartType) = 'LVSTARTHD2' AND a.LeaveDuration = 0.5)
												OR 
												(RTRIM(b.LeaveStartType) = 'LVSTARTHD2' AND a.LeaveDuration > 0.5 AND @leaveDate = a.LeaveStartDate)
												OR 
												(RTRIM(b.LeaveResumeType) = 'LVRESMEHD2' AND a.LeaveDuration > 0.5 AND @leaveDate = a.LeaveResumeDate)
											) THEN 'Half Day Leave - Second half of the day'
										ELSE ''
									END
		FROM tas.sy_LeaveRequisition2 a WITH (NOLOCK)
			INNER JOIN tas.sy_LeaveRequisitionDetail b WITH (NOLOCK) ON a.RequisitionNo = b.LeaveNo AND a.EmpNo = b.LeaveEmpNo 
		WHERE a.EmpNo = @empNo
			AND @leaveDate BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.LeaveStartDate, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, a.LeaveEndDate, 12))
			AND RTRIM(a.HalfDayLeaveFlag) IN 
			(
				'1',	--Half Day on Leave Start Date 
				'2',	--Half Day on Leave Resume Date 
				'3'		--Half Day on both Leave Start Date and Leave Resume date  
			)
			AND RTRIM(a.ApprovalFlag) IN 
			(
				'A',	--Approved / Paid
				'N'		--Approved / Not Paid
			)
    END 

	RETURN @leaveDescription

END

/*	Testing:

	SELECT tas.fnGetHalfDayLeaveDetail(10003191, '03/31/2016')
	SELECT tas.fnGetHalfDayLeaveDetail(10001525, '03/31/2016')
	SELECT tas.fnGetHalfDayLeaveDetail(10003191, '11/27/2019')
	SELECT tas.fnGetHalfDayLeaveDetail(10003673, '11/27/2019')

*/

