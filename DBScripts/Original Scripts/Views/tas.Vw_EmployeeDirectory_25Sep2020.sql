USE [tas2]
GO

/****** Object:  View [tas].[Vw_EmployeeDirectory]    Script Date: 22/09/2020 09:17:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmployeeDirectory
*	Description: Get the employee directory information
*
*	Date:			Author:		Rev. #:		Comments:
*	28/11/2016		Ervin		1.0			Created
*	18/01/2017		Ervin		1.1			Added link to "Master_Employee_JDE" table to remove already resigned employees
*	06/02/2017		Ervin		1.2			Added link to "fnGetMobileNo" to get the employees mobile no.
*	15/08/2018		Ervin		1.3			Added  WITH (NOLOCK) clause in all joint tables to enhance data retrieval performance
*	18/08/2019		Ervin		1.4			Added filter by company codes "00100" and "00600"
*	13/10/2019		Ervin		1.5			Added link to "F0005" in JDE to fetch the employee's job title
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_EmployeeDirectory]
AS
	
	SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
			LTRIM(RTRIM(a.YAALPH)) AS EmpName,

			--LTRIM(RTRIM(ISNULL(b.JMDL01, ''))) AS Position,
			LTRIM(RTRIM(p.DRDL01)) + ' ' + LTRIM(RTRIM(p.DRDL02)) AS Position,
			
			--CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
			--	WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
			--END AS ActualCostCenter,
			CASE WHEN ISNULL(k.WorkingBusinessUnit, '') <> ''
				THEN LTRIM(RTRIM(k.WorkingBusinessUnit))
				ELSE
					CASE WHEN LTRIM(RTRIM(j.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
						WHEN LTRIM(RTRIM(j.ABAT1)) = 'UG' THEN LTRIM(RTRIM(j.ABMCU)) 
					END
			END AS BusinessUnit,

			LTRIM(RTRIM(a.YAEEOM)) AS ReligionCode, 
			LTRIM(RTRIM(l.DRDL01)) AS Religion,
			LTRIM(RTRIM(a.YASEX)) AS SexCode, 
			LTRIM(RTRIM(m.DRDL01)) AS Sex,
			LTRIM(RTRIM(a.YAEEOJ)) AS JobCategoryCode, 
			LTRIM(RTRIM(n.DRDL01)) AS JobCategory,
			CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
				THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
				ELSE 0 
			END AS GradeCode,
			CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus,
			CASE WHEN ISNULL(c.T3EFT, 0) = 0 
				THEN tas.ConvertFromJulian(ISNULL(a.YADST, 0)) 
				ELSE tas.ConvertFromJulian(c.T3EFT) 
			END AS DateJoined,
			ROUND
			(
				CONVERT(FLOAT,
				DATEDIFF
				(
					MONTH, 
					CASE WHEN ISNULL(c.T3EFT, 0) = 0 
						THEN tas.ConvertFromJulian(ISNULL(a.YADST, 0)) 
						ELSE tas.ConvertFromJulian(c.T3EFT) 
					END,
					GETDATE() 
				)) 
			/ 12, 2) AS YearsOfService,
			CASE WHEN a.YADOB IS NOT NULL 
				THEN tas.ConvertFromJulian(a.YADOB)
				ELSE NULL
			END AS DateOfBirth,
			ROUND(CONVERT(FLOAT, DATEDIFF(MONTH, CASE WHEN a.YADOB IS NOT NULL THEN tas.ConvertFromJulian(a.YADOB) ELSE NULL END, GETDATE())) / CONVERT(FLOAT, 12), 2) AS Age,
			ISNULL(e.WPPH1, '') AS TelephoneExt,
			
			tas.fnGetMobileNo(CAST(a.YAAN8 AS INT)) AS MobileNo,
			--CASE WHEN ISNULL(f.WPPH1, '') <> '' THEN '+973' + LTRIM(RTRIM(f.WPPH1)) ELSE '' END AS MobileNo,

			CASE WHEN ISNULL(g.WPPH1, '') <> '' THEN '+973' + LTRIM(RTRIM(g.WPPH1)) ELSE '' END AS TelNo,
			CASE WHEN ISNULL(h.WPPH1, '') <> '' THEN '+973' + LTRIM(RTRIM(h.WPPH1)) ELSE '' END AS FaxNo,
			LTRIM(RTRIM(ISNULL(i.EAEMAL, ''))) AS EmpEmail,
			CAST(a.YAANPA AS INT) AS SupervisorNo
	FROM tas.syJDE_F060116 a WITH (NOLOCK)
		--LEFT JOIN tas.syJDE_F08001 b WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.JMJBCD))
		LEFT JOIN tas.syJDE_F00092 c WITH (NOLOCK) ON a.YAAN8 = c.T3SBN1 AND LTRIM(RTRIM(c.T3TYDT)) = 'WH' AND LTRIM(RTRIM(c.T3SDB)) = 'E'
		--LEFT JOIN tas.syJDE_F0101 d ON a.YAAN8 = d.ABAN8
		LEFT JOIN tas.syJDE_F0115 e WITH (NOLOCK) ON a.YAAN8 = e.WPAN8 AND LTRIM(RTRIM(e.WPPHTP)) = 'EXT' 
		--LEFT JOIN tas.syJDE_F0115 f ON a.YAAN8 = f.WPAN8 AND LTRIM(RTRIM(f.WPPHTP)) = 'MOBS' 		
		LEFT JOIN tas.syJDE_F0115 g WITH (NOLOCK) ON a.YAAN8 = g.WPAN8 AND LTRIM(RTRIM(g.WPPHTP)) = 'DL' 		
		LEFT JOIN tas.syJDE_F0115 h WITH (NOLOCK) ON a.YAAN8 = h.WPAN8 AND LTRIM(RTRIM(h.WPPHTP)) = 'F' 		
		LEFT JOIN tas.syjde_F01151 i WITH (NOLOCK) ON a.YAAN8 = i.EAAN8 AND i.EAIDLN = 0 AND i.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(i.EAETP))) = 'E' 
		LEFT JOIN tas.syJDE_F0101 j WITH (NOLOCK) ON a.YAAN8 = j.ABAN8
		LEFT JOIN tas.Master_EmployeeAdditional k WITH (NOLOCK) ON CAST(a.YAAN8 AS INT) = k.EmpNo		
		LEFT JOIN tas.syJDE_F0005 l WITH (NOLOCK) ON LTRIM(RTRIM(a.YAEEOM)) = LTRIM(RTRIM(l.DRKY)) AND RTRIM(LTRIM(l.DRSY)) = '06' AND RTRIM(LTRIM(l.DRRT)) = 'M'
		LEFT JOIN tas.syJDE_F0005 m WITH (NOLOCK) ON LTRIM(RTRIM(a.YASEX)) = LTRIM(RTRIM(m.DRKY)) AND RTRIM(LTRIM(m.DRSY)) = '01' AND RTRIM(LTRIM(m.DRRT)) = '29'
		LEFT JOIN tas.syJDE_F0005 n WITH (NOLOCK) ON LTRIM(RTRIM(a.YAEEOJ)) = LTRIM(RTRIM(n.DRKY)) AND RTRIM(LTRIM(n.DRSY)) = '06' AND RTRIM(LTRIM(n.DRRT)) = 'J'
		INNER JOIN tas.Master_Employee_JDE o WITH (NOLOCK) ON CAST(a.YAAN8 AS INT) = o.EmpNo	
		LEFT JOIN tas.syJDE_F0005 p WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(p.DRKY)) AND RTRIM(LTRIM(p.DRSY)) = '06' AND RTRIM(LTRIM(p.DRRT)) = 'G'	--Rev. #1.5
	WHERE o.DateResigned IS NULL
		AND LTRIM(RTRIM(a.YAHMCO)) IN ('00100', '00600')

GO


