/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetCommonAdminCombo
*	Description: This stored procedure is used to retrieve the list of applications and forms from the Common Admin System
*
*	Date			Author		Rev.#		Comments:
*	05/06/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetCommonAdminCombo
(   
	@loadType	TINYINT,
	@appCode	VARCHAR(10) = '',
	@formCode	VARCHAR(10) = ''
)
AS
	
	--Validate parameters
	IF ISNULL(@appCode, '') = ''
		SET @appCode = NULL

	IF ISNULL(@formCode, '') = ''
		SET @formCode = NULL

	IF @loadType = 0		--Get the list of all GARMCO applications registered in the Common Admin System
	BEGIN

		SELECT	a.UDCID AS ApplicationID,
				a.UDCCode AS ApplicationCode,
				RTRIM(a.UDCDesc1) AS ApplicationName 
		FROM tas.syJDE_UserDefinedCode a
		WHERE a.UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE RTRIM(UDCGCode) = 'APP')
		ORDER BY a.UDCDesc1
    END 

	ELSE IF @loadType = 1	--Get the list of all forms for the specified application
	BEGIN

		SELECT	a.FormAppID,
				a.FormCode,
				a.FormName,
				a.FormPublic,
				a.FormSeq 
		FROM tas.sy_FormAccess a
		WHERE ((a.FormAppID = (SELECT UDCID FROM tas.syJDE_UserDefinedCode WHERE RTRIM(UDCCode) = @appCode) OR @appCode IS NULL))
		ORDER BY a.FormAppID, a.FormName
    END	

	ELSE IF @loadType = 2	--Get the details about the specific form
	BEGIN

		SELECT	a.FormCode,
				a.FormName,
				a.FormPublic,
				a.FormSeq 
		FROM tas.sy_FormAccess a
		WHERE RTRIM(a.FormCode) = @formCode
		ORDER BY a.FormName
    END	

GO 

/*
	
PARAMETERS:
	@loadType	TINYINT,
	@appCode	VARCHAR(10) = '',
	@formCode	VARCHAR(10) = ''

	EXEC tas.Pr_GetCommonAdminCombo 0						--Get all application names
	EXEC tas.Pr_GetCommonAdminCombo 1, 'TAS3'				--Get all form names
	EXEC tas.Pr_GetCommonAdminCombo 2, '', 'SHFTPATINQ'		--Get specific form

*/