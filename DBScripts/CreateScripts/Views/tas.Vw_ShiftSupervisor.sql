/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ShiftSupervisor
*	Description: Get the list of all Shift Supervisors
*
*	Date:			Author:		Rev. #:		Comments:
*	12/09/2017		Ervin		1.0			Created
*	06/11/2017		Ervin		1.1			Added join to "tas.SpecialSupervisor" table to fetch the list of special Supervisors
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ShiftSupervisor
AS		

	SELECT DISTINCT * FROM 
	(
		SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
				LTRIM(RTRIM(a.YAALPH)) AS EmpName,
				LTRIM(RTRIM(b.JMDL01)) AS Position,
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END AS CostCenter,
				CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus,
				'Normal Supervisor' AS SupervisorType
		FROM tas.syJDE_F060116 a
			LEFT JOIN tas.syJDE_F08001 b on LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.JMJBCD))
			LEFT JOIN tas.syJDE_F0101 c ON a.YAAN8 = c.ABAN8
		WHERE 
			LTRIM(RTRIM(ISNULL(b.JMDL01, ''))) LIKE '%' + 'SUPERVISOR' + '%'
			AND ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END) = 1

		UNION
    
		SELECT	a.EmpNo, 
				LTRIM(RTRIM(b.YAALPH)) AS EmpName,
				LTRIM(RTRIM(c.JMDL01)) AS Position,
				CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
					WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
				END AS CostCenter,
				CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END AS PayStatus,
				'Special Supervisor' AS SupervisorType
		FROM tas.SpecialSupervisor a 
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = CAST(b.YAAN8 AS INT)
			LEFT JOIN tas.syJDE_F08001 c on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(c.JMJBCD))
			LEFT JOIN tas.syJDE_F0101 d ON b.YAAN8 = d.ABAN8
		WHERE 
			a.IsEnabled = 1
			AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END) = 1
	) a

GO 

/* Testing:

	SELECT * FROM tas.Vw_ShiftSupervisor a
	
*/