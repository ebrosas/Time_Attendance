/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContRegistrationLookup
*	Description: This stored procedure returns multiple resultsets for the list of all cost centers and employees
*
*	Date			Author		Rev. #		Comments:
*	22/08/2021		Ervin		1.0			Created
*	02/12/2021		Ervin		1.1			Added Contractor Job Titles
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContRegistrationLookup
AS
BEGIN
	
	SET NOCOUNT ON 
		
	--List of all cost centers
	SELECT	a.CompanyCode AS Company,
			RTRIM(a.BusinessUnit) AS CostCenter, 
			RTRIM(a.BusinessUnitName) AS CostCenterName,
			a.ParentBU,
			a.Superintendent,
			a.CostCenterManager 
	FROM tas.Master_BusinessUnit_JDE_V2 a WITH (NOLOCK)
	WHERE ISNULL(a.CostCenterManager, 0) > 0
	ORDER BY a.CompanyCode, a.BusinessUnit

	--Get License Types
	SELECT	RTRIM(a.UDCCode) AS LicenseCode,
			RTRIM(a.UDCDesc1) AS LicenseDesc,
			CAST(a.UDCAmount AS INT) AS SequenceNo
	FROM tas.sy_UserDefinedCode a WITH (NOLOCK)
	WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'LICENSETYP')
	ORDER BY A.UDCDesc1

	--List of all employees
	SELECT	a.EmpNo,
			a.EmpName,
			a.Company,
			RTRIM(a.BusinessUnit) AS CostCenter,
			a.GradeCode AS PayGrade,
			a.Position
	FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
	WHERE a.DateResigned IS NULL
		AND ISNUMERIC(a.PayStatus) = 1
		AND RTRIM(a.Company) IN ('00100')

	--Get Blood Groups
	SELECT	RTRIM(a.UDCCode) AS BloodGroupCode,
			RTRIM(a.UDCDesc1) AS BloodGroupDesc,
			CAST(a.UDCAmount AS INT) AS SequenceNo
	FROM tas.sy_UserDefinedCode a WITH (NOLOCK)
	WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'BLOODGROUP')
	ORDER BY CAST(a.UDCAmount AS INT)

	--Get Supplier list
	SELECT	LTRIM(RTRIM(a.ABALPH)) AS SupplierName, 
			LTRIM(RTRIM(a.ABDC)) AS SupplierDesc,
			a.ABAN8 AS SupplierCode
	FROM tas.syJDE_F0101 a WITH (NOLOCK)
	WHERE UPPER(LTRIM(RTRIM(a.ABAT1))) IN ('V')
		AND a.ABAN8 BETWEEN 10000 AND 99999
	ORDER BY LTRIM(RTRIM(a.ABALPH))

	--Get Contractor Job Titles (Rev. #1.1)
	SELECT	RTRIM(a.UDCCode) AS JobTitleCode,
			RTRIM(a.UDCDesc1) AS JobTitleDesc,
			CAST(a.UDCAmount AS INT) AS SequenceNo
	FROM tas.sy_UserDefinedCode a WITH (NOLOCK)
	WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'CONTJOBTLE')
	ORDER BY a.UDCDesc1

END 

/*	Debug:

	EXEC tas.Pr_GetContRegistrationLookup

*/