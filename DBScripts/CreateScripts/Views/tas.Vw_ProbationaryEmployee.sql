/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ProbationaryEmployee
*	Description: Fetches all probationary employees
*
*	Date:			Author:		Rev. #:		Comments:
*	18/08/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ProbationaryEmployee
AS
	
	SELECT     
		CAST(a.YAAN8 AS INT) AS EmpNo, 
		LTRIM(RTRIM(a.YAALPH)) AS EmpName, 
		LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,
		CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
			THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
			ELSE 0 
		END AS PayGrade,
		CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus,
		CASE WHEN ISNULL(d.T3EFT, 0) = 0 
			THEN tas.ConvertFromJulian(ISNULL(a.YADST, 0)) 
			ELSE tas.ConvertFromJulian(d.T3EFT) 
		END AS EmpJoinDate,
		CASE WHEN ISNULL(b.WorkingBusinessUnit, '') <> ''
			THEN LTRIM(RTRIM(b.WorkingBusinessUnit))
			ELSE
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END
		END AS CostCenter,
		CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
			WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
		END AS ActualCostCenter,
		LTRIM(RTRIM(a.YAHMCO)) AS Company, 		
		CAST(a.YAANPA AS INT) AS SupervisorNo,
		LTRIM(RTRIM(i.ABALPH)) AS SupervisorName,
		LTRIM(RTRIM(ISNULL(f.EAEMAL, ''))) AS SupervisorEmail,
		CAST(g.MCANPA AS INT) AS ManagerNo, 
		LTRIM(RTRIM(j.ABALPH)) AS ManagerName,
		LTRIM(RTRIM(ISNULL(h.EAEMAL, ''))) AS ManagerEmail,
		CASE WHEN EXISTS
		(
			SELECT EmpNo FROM tas.Vw_EmployeeAvailability
			WHERE EmpNo = CAST(a.YAANPA AS INT) 
				AND CONVERT(DATETIME, GETDATE(), 101) BETWEEN FromDate AND ToDate		
		) THEN 1 ELSE 0 END AS IsSupervisorOnLeave
	FROM tas.syJDE_F060116 a
		LEFT JOIN tas.Master_EmployeeAdditional b ON CAST(a.YAAN8 AS INT) = b.EmpNo
		LEFT JOIN tas.syJDE_F0101 c ON a.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d ON a.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F08001 e on LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
		LEFT JOIN tas.syJDE_F01151 f ON a.YAANPA = f.EAAN8 AND f.EAIDLN = 0 AND f.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(f.EAETP))) = 'E' 
		LEFT JOIN tas.syJDE_F0006 AS g ON 
			(CASE WHEN c.ABAT1 = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
				  WHEN c.ABAT1 = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) END) = LTRIM(RTRIM(g.MCMCU)) 
		LEFT JOIN tas.syJDE_F01151 h ON g.MCANPA = h.EAAN8 AND f.EAIDLN = 0 AND h.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(h.EAETP))) = 'E' 
		LEFT JOIN tas.syJDE_F0101 i ON a.YAANPA = i.ABAN8 
		LEFT JOIN tas.syJDE_F0101 j ON g.MCANPA = j.ABAN8 
	WHERE a.YAAN8 > 10000000


/*	Debugging:

	SELECT * FROM tas.Vw_ProbationaryEmployee a 
	WHERE a.EmpNo = 10003632

*/