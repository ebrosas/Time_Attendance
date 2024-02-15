/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_FireTeamFireWatchMembers
*	Description: Get the Fire Team and Fire Watch member employees
*
*	Date:			Author:		Rev. #:		Comments:
*	11/02/2018		Ervin		1.0			Created
*	07/02/2019		Ervin		1.1			Added filter condition that exclude employees where DateResigned is not null
*	07/09/2020		Ervin		1.2			Refactored the code to enhance data rerieval performance
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_FireTeamFireWatchMembers
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
	FROM tas.sy_NAMES a WITH (NOLOCK) 
		INNER JOIN tas.sy_UDF b WITH (NOLOCK) ON a.ID = b.NameID	
		INNER JOIN tas.Master_Employee_JDE_View_V2 c WITH (NOLOCK) ON 
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN CONVERT(INT, a.FName)
					ELSE CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END = c.EmpNo AND ISNUMERIC(c.PayStatus) = 1
		LEFT JOIN tas.Master_BusinessUnit_JDE_view d WITH (NOLOCK) ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
		LEFT JOIN tas.Master_Employee_JDE_View e WITH (NOLOCK) ON c.SupervisorNo = e.EmpNo
		LEFT JOIN tas.syJDE_F0115 f WITH (NOLOCK) ON c.EmpNo = CAST(f.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(f.WPPHTP))) = 'EXT'
		LEFT JOIN tas.syJDE_F0115 g WITH (NOLOCK) ON c.EmpNo = CAST(g.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(g.WPPHTP))) = 'MOBS'
	WHERE b.UdfNum IN (11, 14) 
		AND UPPER(LTRIM(RTRIM(b.UdfText))) = 'Y' 
		AND c.DateResigned IS NULL	--Rev. #1.1

GO


