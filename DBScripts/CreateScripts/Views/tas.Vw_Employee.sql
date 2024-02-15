/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LicenseTypes
*	Description: Get the employee information
*
*	Date:			Author:		Rev. #:		Comments:
*	07/10/2021		Ervin		1.0			Created
*
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_Employee
AS
	
	SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
			LTRIM(RTRIM(a.YAALPH)) AS EmpName,
			LTRIM(RTRIM(c.DRDL01)) + ' ' + LTRIM(RTRIM(c.DRDL02)) AS Position,
			LTRIM(RTRIM(a.YAMCU)) AS CostCenter,
			LTRIM(RTRIM(b.MCDC)) AS CostCenterName,
			CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
				THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
				ELSE 0 
			END AS PayGrade,			
			CAST(a.YAANPA AS INT) AS SupervisorNo,
			LTRIM(RTRIM(d.YAALPH)) AS SupervisorName,
			CAST(b.MCANPA AS INT) AS ManagerNo,
			LTRIM(RTRIM(e.YAALPH)) AS ManagerName
			--g.CPRNo,
			--h.BloodGroup,
			--h.BloodGroupDesc,
			--f.CardNo
	FROM tas.syJDE_F060116 a WITH (NOLOCK)
		INNER JOIN tas.syJDE_F0006 b WITH (NOLOCK) ON LTRIM(RTRIM(a.YAMCU)) = LTRIM(RTRIM(b.MCMCU))
		LEFT JOIN tas.syJDE_F0005 c WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(c.DRKY)) AND RTRIM(LTRIM(c.DRSY)) = '06' AND RTRIM(LTRIM(c.DRRT)) = 'G'
		LEFT JOIN tas.syJDE_F060116 d ON a.YAANPA = d.YAAN8
		LEFT JOIN tas.syJDE_F060116 e ON b.MCANPA = e.YAAN8
		--OUTER APPLY 
		--(
		--	SELECT TOP 1 CardRefNo AS CardNo 
		--	FROM tas.IDCardHistory 
		--	WHERE EmpNo = CAST(a.YAAN8 AS INT)
		--) f
		--OUTER APPLY
		--(
		--	SELECT TOP 1 LTRIM(RTRIM(T3RMK)) AS CPRNo 
		--	FROM tas.syJDE_F00092 WITH (NOLOCK)
		--	WHERE LTRIM(RTRIM(T3TYDT)) = 'LD' 
		--		AND LTRIM(RTRIM(T3KY)) = 'CP-EMP'
		--		AND T3SBN1 = a.YAAN8
		--) g
		--OUTER APPLY
		--(
		--	SELECT TOP 1 x.BloodGroup, RTRIM(y.UDCDesc1) AS 'BloodGroupDesc' 
		--	FROM tas.IDCardRegistry x WITH (NOLOCK) 
		--		LEFT JOIN tas.sy_UserDefinedCode y WITH (NOLOCK) ON RTRIM(x.BloodGroup) = RTRIM(y.UDCCode)
		--	WHERE x.EmpNo = CAST(a.YAAN8 AS INT) 
		--		AND ISNULL(x.IsContractor, 0) = 0
		--) h
	WHERE ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) IN ('I', 'A', 'P')) THEN '0' ELSE a.YAPAST END) = 1
		AND CAST(a.YAAN8 AS INT) > 10000000

GO  

/*	Debug:

	SELECT * FROM tas.Vw_Employee a
	WHERE a.EmpNo = 10003632

*/