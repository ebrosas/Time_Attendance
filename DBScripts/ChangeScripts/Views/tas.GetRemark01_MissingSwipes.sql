/********************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.GetRemark01_MissingSwipes
*	Description: Get the attendance remarks
*
*	Date:			Author:		Rev. #:		Comments:
*	12/02/2012		Ervin		1.0			Created
*	21/08/2022		Ervin		1.1			Refactored the logic and enhanced the performance
*	25/08/2022		Ervin		1.2			Added condition to check if holiday or DIL holiday
*********************************************************************************************************************************************************************************/

ALTER VIEW tas.GetRemark01_MissingSwipes 
AS

	SELECT A.AutoID , B.One FROM 
	(
		SELECT AutoID, EmpNo, DT FROM tas.Tran_Timesheet WITH (NOLOCK)
	) A
	LEFT JOIN 
	(
		SELECT DISTINCT x.EmpNo, x.DT, 1 AS One
		FROM tas.Tran_Timesheet x WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND x.DT = y.DateX
		WHERE 
		(
			(dtIN IS NOT NULL AND dtOUT IS NOT NULL) 
			OR (dtIN IS NULL AND dtOUT IS NULL AND (y.Effective_ShiftCode = 'O' OR ISNULL(x.LeaveType, '') <> '' OR ISNULL(x.AbsenceReasonCode, '') <> '' OR ISNULL(x.IsPublicHoliday, 0) = 0 OR ISNULL(x.IsDILdayWorker, 0) = 0))		--Rev. #1.2
		)
	) B ON A.EmpNo = B.EmpNo AND a.DT = B.DT

GO


