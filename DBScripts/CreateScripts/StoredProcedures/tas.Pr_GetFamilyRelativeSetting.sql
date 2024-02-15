/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetFamilyRelativeSetting
*	Description: Get the differect types of family relative
*
*	Date			Author		Rev. #		Comments:
*	30/06/2019		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

CREATE PROCEDURE tas.Pr_GetFamilyRelativeSetting
(   
	@degreeLevel		TINYINT = 0,
	@relativeTypeCode	VARCHAR(15) = ''
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Validate parameters
	IF ISNULL(@degreeLevel, 0) = 0
		SET @degreeLevel = NULL

	IF ISNULL(@relativeTypeCode, '') = ''
		SET @relativeTypeCode = NULL

	SELECT * FROM tas.FamilyRelativeSetting a WITH (NOLOCK)
	WHERE (a.DegreeLevel = @degreeLevel OR @degreeLevel IS NULL)
		AND (RTRIM(a.RelativeTypeCode) = @relativeTypeCode OR @relativeTypeCode IS NULL)
	ORDER BY a.DegreeLevel, a.SequenceNo

/*	Debug:

PARAMETERS:
	@degreeLevel		TINYINT = 0,
	@relativeTypeCode	VARCHAR(15) = ''

	EXEC tas.Pr_GetFamilyRelativeSetting 
	EXEC tas.Pr_GetFamilyRelativeSetting 1
	EXEC tas.Pr_GetFamilyRelativeSetting 0, 'DEG1FATMOT'

*/