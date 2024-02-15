/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_SearchContractors
*	Description: This stored procedure is used to perform CRUD operations in "ContractorRegistry" table
*
*	Date			Author		Revision No.	Comments:
*	07/09/2021		Ervin		1.0				Created
*	02/12/2021		Ervin		1.1				Refactored the logic in fetching the the Job Title
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_SearchContractors
(
	@contractorNo			INT = NULL,
	@idNumber				VARCHAR(20) = NULL,
	@contractorName			VARCHAR(60) = NULL,
	@companyName			VARCHAR(50) = NULL,
	@costCenter				VARCHAR(12) = NULL,
	@jobTitle				VARCHAR(10) = NULL,
	@supervisorName			VARCHAR(100) = NULL, 
	@contractStartDate		SMALLDATETIME = NULL,
	@contractEndDate		SMALLDATETIME = NULL 
)
AS	
BEGIN

	SET NOCOUNT ON 

	--Validate parameters
	IF ISNULL(@contractorNo, 0) = 0
		SET @contractorNo = NULL

	IF ISNULL(@idNumber, '') = ''
		SET @idNumber = NULL

	IF ISNULL(@contractorName, '') = ''
		SET @contractorName = NULL

	IF ISNULL(@companyName, '') = ''
		SET @companyName = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@jobTitle, '') = ''
		SET @jobTitle = NULL

	IF ISNULL(@supervisorName, '') = ''
		SET @supervisorName = NULL

	IF ISNULL(@contractStartDate, '') = '' OR @contractStartDate = CAST(NULL AS SMALLDATETIME) 
		SET @contractStartDate = NULL 

	IF ISNULL(@contractEndDate, '') = '' OR @contractEndDate = CAST(NULL AS SMALLDATETIME) 
		SET @contractEndDate = NULL 
		
	SELECT	a.RegistryID,
			a.ContractorNo,
			a.RegistrationDate,
			a.IDNumber,
			CASE WHEN a.IDType = 0 THEN 'CPR' ELSE 'Passport' END AS IDType,
			a.FirstName,
			a.LastName,
			a.CompanyName,
			a.CompanyID,
			a.CompanyCRNo,
			a.PurchaseOrderNo,
			d.JobTitle,
			a.MobileNo,
			a.VisitedCostCenter,
			RTRIM(b.BUname) AS 'VisitedCostCenterName',
			a.SupervisorEmpNo,
			a.SupervisorEmpName,
			a.PurposeOfVisit,
			a.ContractStartDate,
			a.ContractEndDate,
			a.BloodGroup,
			a.Remarks,
			a.WorkDurationHours,
			a.WorkDurationMins,
			a.CompanyContactNo,
			a.CreatedDate,
			a.CreatedByEmpNo,
			LTRIM(RTRIM(c.YAALPH)) AS 'CreatedByEmpName',
			a.CreatedByUser
	FROM tas.ContractorRegistry a WITH (NOLOCK)
		LEFT JOIN tas.Master_BusinessUnit_JDE_view b ON RTRIM(a.VisitedCostCenter) = RTRIM(b.BU)
		LEFT JOIN tas.syJDE_F060116 c WITH (NOLOCK) ON a.CreatedByEmpNo = CAST(c.YAAN8 AS INT)
		OUTER APPLY
		(
			SELECT	RTRIM(UDCCode) AS JobCode,
					RTRIM(UDCDesc1) AS JobTitle
			FROM tas.sy_UserDefinedCode WITH (NOLOCK)
			WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'CONTJOBTLE')
				AND RTRIM(UDCCode) = RTRIM(a.JobTitle)
		) d
	WHERE (a.ContractorNo = @contractorNo OR @contractorNo IS NULL)
		AND (UPPER(RTRIM(a.IDNumber)) LIKE '%' + UPPER(RTRIM(@idNumber)) + '%' OR @idNumber IS NULL)
		AND (UPPER(RTRIM(a.FirstName)) + UPPER(RTRIM(a.LastName)) LIKE '%' + UPPER(RTRIM(@contractorName)) + '%' OR @contractorName IS NULL)
		AND (UPPER(RTRIM(a.CompanyName)) LIKE '%' + UPPER(RTRIM(@companyName)) + '%' OR @companyName IS NULL)
		AND (RTRIM(a.VisitedCostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND (UPPER(RTRIM(d.JobTitle)) LIKE '%' + UPPER(RTRIM(@jobTitle)) + '%' OR @jobTitle IS NULL)
		AND (UPPER(RTRIM(a.SupervisorEmpName)) LIKE '%' + UPPER(RTRIM(@supervisorName)) + '%' OR @supervisorName IS NULL)
		AND 
		(
			(a.ContractStartDate BETWEEN @contractStartDate AND a.ContractStartDate AND a.ContractEndDate BETWEEN @contractStartDate AND @contractEndDate AND @contractStartDate IS NOT NULL AND @contractEndDate IS NOT NULL)
			OR (@contractStartDate IS NULL AND @contractEndDate IS NULL)
		)
	ORDER BY a.ContractorNo

END 

/*	Debug:

PARAMETERS:
	@contractorNo			INT = NULL,
	@idNumber				VARCHAR(20) = NULL,
	@contractorName			VARCHAR(60) = NULL,
	@companyName			VARCHAR(50) = NULL,
	@costCenter				VARCHAR(12) = NULL,
	@jobTitle				VARCHAR(50) = NULL,
	@supervisorName			VARCHAR(100) = NULL, 
	@contractStartDate		SMALLDATETIME = NULL,
	@contractEndDate		SMALLDATETIME = NULL 

	EXEC tas.Pr_SearchContractors 
	EXEC tas.Pr_SearchContractors 60002, '', 'antonina'

*/