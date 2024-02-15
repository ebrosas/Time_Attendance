/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_FireWatchMembers
*	Description: Get the Fire Watch nenber employees
*
*	Date:			Author:		Rev. #:		Comments:
*	11/02/2018		Ervin		1.0			Created
*	07/02/2019		Ervin		1.1			Added filter condition that exclude employees where DateResigned is not null
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_FireWatchMembers
AS		
	
	SELECT	CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN CONVERT(INT, a.FName)
					ELSE CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END AS EmpNo,
			c.EmpName,
			c.Position,
			c.GradeCode,
			RTRIM(c.BusinessUnit) AS CostCenter,
			RTRIM(d.BUname) AS CostCenterName,
			c.SupervisorNo AS SupervisorEmpNo,
			RTRIM(e.EmpName) AS SupervisorEmpName,
			ISNULL(CONVERT(VARCHAR(20), f.WPPH1),'') AS Extension,
			ISNULL(LTRIM(RTRIM(g.WPPH1)), '') AS MobileNo,
			LTRIM(RTRIM(CONVERT(VARCHAR(500), a.Notes))) AS Notes
	FROM tas.sy_NAMES a 
		INNER JOIN tas.sy_UDF b ON a.ID = b.NameID	
		INNER JOIN tas.Master_Employee_JDE_View_V2 c ON 
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN CONVERT(INT, a.FName)
					ELSE CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END = c.EmpNo AND ISNUMERIC(c.PayStatus) = 1
		LEFT JOIN tas.Master_BusinessUnit_JDE_view d ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
		LEFT JOIN tas.Master_Employee_JDE_View e ON c.SupervisorNo = e.EmpNo
		LEFT JOIN tas.syJDE_F0115 f on c.EmpNo = CAST(f.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(f.WPPHTP))) = 'EXT'
		LEFT JOIN tas.syJDE_F0115 g on c.EmpNo = CAST(g.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(g.WPPHTP))) = 'MOBS'
	WHERE b.UdfNum = 14 
		AND UPPER(LTRIM(RTRIM(b.UdfText))) = 'Y' 
		AND c.DateResigned IS NULL	--Rev. #1.1

GO

/* Testing:

	SELECT * FROM tas.Vw_FireWatchMembers a
	
*/
