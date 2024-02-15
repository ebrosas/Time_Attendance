/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmpContractorIDBadgeInfo
*	Description: Fetches all probationary employees
*
*	Date:			Author:		Rev. #:		Comments:
*	18/08/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_EmpContractorIDBadgeInfo
AS
	
	SELECT	
		CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
			END AS EmpNo,
			RTRIM(a.LName) AS EmpName,
			c.CostCenter,
			d.BusinessUnitName AS CostCenterName     
	FROM tas.sy_NAMES a
		INNER JOIN tas.sy_COMPANY b ON a.Company = b.Company
		LEFT JOIN tas.AccessSystemCostCenterMapping c ON b.Company = c.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE d ON LTRIM(RTRIM(c.CostCenter)) = LTRIM(RTRIM(d.BusinessUnit))

/*	Debugging:

	SELECT * FROM tas.Vw_EmpContractorIDBadgeInfo a
	WHERE a.EmpNo IN (53599, 10003632)

*/