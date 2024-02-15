/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LeaveRequisition
*	Description: This view retrieves the employee leave requisitions
*
*	Date:			Author:		Rev. #:		Comments:
*	04/04/2016		Ervin		1.0			Created
*	15/04/2016		Ervin		1.1			Added "HalfDayLeaveFlag"
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_LeaveRequisition
AS

	SELECT	a.LRAN8 AS EmpNo, 
			LTRIM(RTRIM(a.LRMCU)) AS CostCenter,
			a.LRY58VCRQN AS LeaveNo,
			LTRIM(RTRIM(a.LRY58VCVCD)) AS LeaveType,
			tas.ConvertFromJulian(a.LRY58VCOFD) AS LeaveStartDate, 
			CASE
				WHEN tas.ConvertFromJulian(a.LRY58VCOTD) < tas.ConvertFromJulian(a.LRY58VCOFD) THEN --(Note: Check if Leave End Date < Leave Start Date. If true, then set Leave End Date = Leave Resume Date)
					CASE	
						WHEN ISNULL(a.LRY58VCOTD, 0) = 0 THEN tas.ConvertFromJulian(a.LRY58VCOFD)
						WHEN tas.ConvertFromJulian(a.LRY58VCOFD) = tas.ConvertFromJulian(a.LRY58VCOTD) THEN tas.ConvertFromJulian(a.LRY58VCOTD)
						ELSE DATEADD(dd, 1, tas.ConvertFromJulian(a.LRY58VCOTD))
					END
				ELSE tas.ConvertFromJulian(ISNULL(a.LRY58VCOTD, a.LRY58VCOFD))
			END AS LeaveEndDate,
			CASE		
				WHEN (tas.ConvertFromJulian(a.LRY58VCOFD) = tas.ConvertFromJulian(a.LRY58VCOTD) AND a.LREV02 IN ('1')) THEN DATEADD(dd, 1, tas.ConvertFromJulian(a.LRY58VCOTD))	
				WHEN (tas.ConvertFromJulian(a.LRY58VCOFD) = tas.ConvertFromJulian(a.LRY58VCOTD) AND a.LREV02 IN ('2', '3')) THEN tas.ConvertFromJulian(a.LRY58VCOTD)
				WHEN (tas.ConvertFromJulian(a.LRY58VCOFD) < tas.ConvertFromJulian(a.LRY58VCOTD) AND a.LREV02 IN ('2', '3')) THEN tas.ConvertFromJulian(a.LRY58VCOTD)
				WHEN ISNULL(a.LRY58VCOTD, 0) = 0 THEN tas.ConvertFromJulian(a.LRY58VCOFD)
				ELSE DATEADD(dd, 1, tas.ConvertFromJulian(a.LRY58VCOTD))
			END AS LeaveResumeDate,
			(ISNULL(a.LRY58VCVDR, 0) / 10000) AS LeaveDuration,
			ISNULL(a.LRY58VCHFD, 'F') AS HalfDayLeave, 
			ISNULL(a.LRAN81, 0) AS SubEmpNo, 
			a.LRY58VCAFG AS ApprovalFlag,
			a.LREV02 AS HalfDayLeaveFlag
	FROM tas.syJDE_F58LV13 a 

GO

/*	Debugging:

	SELECT * FROM tas.Vw_LeaveRequisition a
	WHERE a.EmpNo = 10003512
	ORDER BY a.LeaveStartDate DESC

*/