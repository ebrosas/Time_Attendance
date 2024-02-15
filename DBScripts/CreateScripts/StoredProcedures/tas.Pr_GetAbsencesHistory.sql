/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAbsencesHistory
*	Description: This stored procedure is used to retrieve the employee absences history
*
*	Date			Author		Revision No.	Comments:
*	31/05/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAbsencesHistory
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

	SELECT	a.AutoID,
			a.Processed,
			a.EmpNo,
			--b.EmpName,
			a.DT,
			a.RemarkCode,
			'Absent' AS Remarks
			--b.PayStatus
	FROM tas.Tran_Timesheet a
		--INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE UPPER(RTRIM(ISNULL(RemarkCode, ''))) = 'A' 
		AND ISNULL(a.IsLastRow, 0) = 1		
		AND ISNULL(a.AbsenceReasonCode, '') = ''
		AND ISNULL(a.LeaveType, '') = ''	
		--AND ISNUMERIC(b.PayStatus) = 1	
		AND a.EmpNo = @EmpNo
		AND 
		(
			a.DT BETWEEN @startDate AND @endDate
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY a.DT DESC 

GO 

/*	Debugging:

	EXEC tas.Pr_GetAbsencesHistory 10003512, '16/03/2016', '15/04/2016'
	EXEC tas.Pr_GetAbsencesHistory 10003512, null, null

*/


