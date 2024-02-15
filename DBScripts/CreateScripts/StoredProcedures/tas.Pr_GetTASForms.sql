/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTASForms
*	Description: Retrieves the public holidays declared in GARMCO
*
*	Date			Author		Revision No.	Comments:
*	07/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTASForms
(   
	@formCode	VARCHAR(10) = ''
)
AS

	--Validate parameters
	IF ISNULL(@formCode, '') = ''
		SET @formCode = NULL

	SELECT	a.FormCode,
			a.FormName,
			a.FormAppID,
			a.FormMenuID,
			a.FormFilename,
			a.FormPublic,
			a.FormImgMouseOut,
			a.FormImgMouseOver,
			a.FormSeq,
			a.FormCreatedBy,
			a.FormCreatedDate,
			a.FormModifiedBy,
			a.FormModifiedDate
	FROM tas.sy_FormAccess a
	WHERE a.FormAppID = 
		(
			SELECT UDCID 
			FROM tas.syJDE_UserDefinedCode 
			WHERE RTRIM(UDCCode) = 'TAS3'
		) 
		AND (RTRIM(a.FormCode) = RTRIM(@formCode) OR @formCode IS NULL)
	ORDER BY a.FormName ASC

GO 

/*	Debugging:

PARAMETERS:
	@formCode	VARCHAR(10) = ''

	EXEC tas.Pr_GetTASForms 
	EXEC tas.Pr_GetTASForms 'GARMCOCAL'

*/


