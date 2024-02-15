USE [tas2]
GO

/****** Object:  View [tas].[Vw_FireTeamFireWatchMembers]    Script Date: 07/02/2019 11:27:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_FireTeamFireWatchMembers
*	Description: Get the Fire Team and Fire Watch member employees
*
*	Date:			Author:		Rev. #:		Comments:
*	11/02/2018		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_FireTeamFireWatchMembers]
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
		LEFT JOIN tas.syJDE_F0115 f ON c.EmpNo = CAST(f.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(f.WPPHTP))) = 'EXT'
		LEFT JOIN tas.syJDE_F0115 g ON c.EmpNo = CAST(g.WPAN8 AS INT) AND UPPER(LTRIM(RTRIM(g.WPPHTP))) = 'MOBS'
	WHERE b.UdfNum IN (11, 14) 
		AND UPPER(LTRIM(RTRIM(b.UdfText))) = 'Y' 

GO


