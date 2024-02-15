/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetUserFormAccessInfo
*	Description: This stored procedure is used to get the user form-level access information
*
*	Date			Author		Rev.#		Comments:
*	04/06/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetUserFormAccessInfo
(   
	@appCode	VARCHAR(10),	
	@empNo		INT = 0,
	@formCode	VARCHAR(10) = ''
)
AS
	
	--Validate parameters
	IF ISNULL(@formCode, '') = ''
		SET @formCode = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	b.UDCDesc1 AS ApplicationName,
			CASE WHEN ISNULL(c.UserFrmEmpNo, 0) > 0
				THEN c.UserFrmEmpNo
				ELSE CASE WHEN @empNo > 0 THEN @empNo ELSE NULL END 
			END AS EmpNo,
			CASE WHEN ISNULL(c.UserFrmEmpNo, 0) > 0
				THEN RTRIM(d.EmpName)
				ELSE CASE WHEN @empNo > 0 THEN RTRIM(g.EmpName) ELSE NULL END 
			END AS EmpName,
			CASE WHEN ISNULL(c.UserFrmEmpNo, 0) > 0
				THEN RTRIM(d.Position)
				ELSE CASE WHEN @empNo > 0 THEN RTRIM(g.Position) ELSE NULL END 
			END AS Position,
			CASE WHEN ISNULL(c.UserFrmEmpNo, 0) > 0
				THEN RTRIM(d.BusinessUnit)
				ELSE CASE WHEN @empNo > 0 THEN RTRIM(g.BusinessUnit) ELSE NULL END 
			END AS CostCenter,			
			a.FormCode,
			a.FormName,
			CASE WHEN a.FormPublic = 1 
				THEN '1111100000'
				ELSE ISNULL(c.UserFrmCRUDP, '0000000000')
			END AS UserFrmCRUDP, 
			a.FormPublic,
			RTRIM(c.UserFrmFormCode) AS UserFrmFormCode,
			c.UserFrmCreatedBy AS CreatedByEmpNo,		
			RTRIM(e.EmpName) AS CreatedByEmpName,	
			c.UserFrmCreatedDate AS CreatedDate,
			c.UserFrmModifiedBy AS LastUpdatedByEmpNo,
			RTRIM(f.EmpName) AS LastUpdatedByEmpName,
			c.UserFrmModifiedDate AS LastUpdatedDate
	FROM tas.sy_FormAccess a
		INNER JOIN tas.syJDE_UserDefinedCode b ON a.FormAppID = b.UDCID
		LEFT JOIN tas.sy_UserFormAccess c ON RTRIM(a.FormCode) = RTRIM(c.UserFrmFormCode) AND (c.UserFrmEmpNo = @empNo OR @empNo IS NULL)
		LEFT JOIN tas.Master_Employee_JDE_View_V2 d ON c.UserFrmEmpNo = d.EmpNo
		LEFT JOIN tas.Master_Employee_JDE_View e ON c.UserFrmCreatedBy = e.EmpNo
		LEFT JOIN tas.Master_Employee_JDE_View f ON c.UserFrmModifiedBy = f.EmpNo
		LEFT JOIN tas.Master_Employee_JDE_View_V2 g ON g.EmpNo = @empNo
	WHERE 
		RTRIM(b.UDCCode) = @appCode
		AND (RTRIM(a.FormCode) = @formCode OR @formCode IS NULL)
	ORDER BY a.FormName ASC
		

GO 

/*
	
PARAMETERS:
	@appCode	VARCHAR(10),	
	@empNo		INT = 0,
	@formCode	VARCHAR(10) = ''

	EXEC tas.Pr_GetUserFormAccessInfo 'TAS3'
	EXEC tas.Pr_GetUserFormAccessInfo 'TAS3', 10003632
	EXEC tas.Pr_GetUserFormAccessInfo 'TAS3', 10003632, 'ABSENCERPT'
	EXEC tas.Pr_GetUserFormAccessInfo 'TAS3', 0, 'VPASSENTRY'

*/