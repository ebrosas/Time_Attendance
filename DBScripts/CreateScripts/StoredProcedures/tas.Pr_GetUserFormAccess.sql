/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetUserFormAccess
*	Description: Get the cost center permissions given to a user
*
*	Date			Author		Rev. #		Comments:
*	18/01/2017		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetUserFormAccess
(   		
	@appCode		VARCHAR(10),
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL


	SELECT	RTRIM(c.UDCDesc1) AS ApplicatioName,
			RTRIM(c.UDCCode) AS ApplicationCode,
			b.FormAppID AS ApplicationID,
			a.UserFrmFormCode,
			b.FormName,
			a.UserFrmEmpNo AS EmpNo,
			RTRIM(d.EmpName) AS EmpName,
			a.UserFrmCreatedBy AS CreatedByEmpNo,
			RTRIM(e.EmpName) AS CreatedByEmpName,
			a.UserFrmCreatedDate AS CreatedDate,
			a.UserFrmModifiedBy AS ModifiedByEmpNo,
			RTRIM(f.EmpName) AS ModifiedByEmpName,
			a.UserFrmModifiedDate AS ModifiedDate
	FROM tas.sy_UserFormAccess a
		INNER JOIN tas.sy_FormAccess b ON RTRIM(a.UserFrmFormCode) = RTRIM(b.FormCode)
		INNER JOIN tas.syJDE_UserDefinedCode c ON b.FormAppID = c.UDCID AND UDCUDCGID = 17
		INNER JOIN tas.Master_Employee_JDE_View_V2 d ON a.UserFrmEmpNo = d.EmpNo
		LEFT JOIN tas.Master_Employee_JDE e ON a.UserFrmCreatedBy = e.EmpNo
		LEFT JOIN tas.Master_Employee_JDE f ON a.UserFrmModifiedBy = f.EmpNo
	WHERE 
		RTRIM(UDCCode) = @appCode
		AND (a.UserFrmEmpNo = @empNo OR @empNo IS NULL)

	

GO 

/*	Debug:

PARAMETERS:
	@appCode		VARCHAR(10),
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''

	EXEC tas.Pr_GetUserFormAccess 'TAS3'
	EXEC tas.Pr_GetUserFormAccess 1, 10003632			--By Emp. No.
	EXEC tas.Pr_GetUserFormAccess 0, 0, '7500'		--By Cost Center

*/