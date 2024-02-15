/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LicenseTypes
*	Description: Get the contractor information
*
*	Date:			Author:		Rev. #:		Comments:
*	11/10/2021		Ervin		1.0			Created
*	02/12/2021		Ervin		1.1			Added "JobCode" field
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_Contractor
AS
	
	SELECT	a.RegistryID,
			a.ContractorNo,
			a.RegistrationDate,
			a.IDNumber,
			a.IDType,
			a.FirstName,
			a.LastName,
			a.CompanyName,
			a.CompanyID,
			a.CompanyCRNo,
			a.PurchaseOrderNo,
			a.JobTitle AS JobCode,
			g.JobTitle,
			a.MobileNo,
			a.VisitedCostCenter,
			RTRIM(b.BUname) AS 'VisitedCostCenterName',
			a.SupervisorEmpNo,
			a.SupervisorEmpName,
			a.PurposeOfVisit,
			a.ContractStartDate,
			a.ContractEndDate,
			a.BloodGroup,
			RTRIM(e.UDCDesc1) AS 'BloodGroupDesc',
			a.Remarks,
			a.CreatedDate,
			a.CreatedByEmpNo,
			LTRIM(RTRIM(c.YAALPH)) AS 'CreatedByEmpName',
			a.CreatedByUser,
			a.LastUpdatedDate,
			a.LastUpdatedByEmpNo,
			LTRIM(RTRIM(d.YAALPH)) AS LastUpdatedByEmpName,
			a.LastUpdatedByUser,
			f.CardNo,
			a.WorkDurationHours,
			a.WorkDurationMins,
			a.CompanyContactNo
	FROM tas.ContractorRegistry a WITH (NOLOCK)
		LEFT JOIN tas.Master_BusinessUnit_JDE_view b ON RTRIM(a.VisitedCostCenter) = RTRIM(b.BU)
		LEFT JOIN tas.syJDE_F060116 c WITH (NOLOCK) ON a.CreatedByEmpNo = CAST(c.YAAN8 AS INT)
		LEFT JOIN tas.syJDE_F060116 d WITH (NOLOCK) ON a.LastUpdatedByEmpNo = CAST(d.YAAN8 AS INT)
		LEFT JOIN tas.sy_UserDefinedCode e WITH (NOLOCK) ON RTRIM(a.BloodGroup) = RTRIM(e.UDCCode)
		OUTER APPLY 
		(
			SELECT TOP 1 CardRefNo AS CardNo 
			FROM tas.IDCardHistory 
			WHERE EmpNo = a.ContractorNo
		) f
		OUTER APPLY
		(
			SELECT	RTRIM(UDCCode) AS JobCode,
					RTRIM(UDCDesc1) AS JobTitle
			FROM tas.sy_UserDefinedCode WITH (NOLOCK)
			WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'CONTJOBTLE')
				AND RTRIM(UDCCode) = RTRIM(a.JobTitle)
		) g

GO 

/*	Debug:

	SELECT * FROM tas.Vw_Contractor a
	WHERE a.ContractorNo = 60011

*/