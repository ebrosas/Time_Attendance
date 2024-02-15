/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetUDCListItem
*	Description: Get user-defined code items
*
*	Date			Author		Revision No.	Comments:
*	20/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetUDCListItem
(   
	@udcKey	varchar(10) = ''
)
AS

	--Validate parameters
	IF ISNULL(@udcKey, '') = ''
		SET @udcKey = NULL

	SELECT * FROM tas.Master_UDCValues a
	WHERE (RTRIM(a.UDCKey) = RTRIM(@udcKey) OR @udcKey IS NULL)
	ORDER BY a.UDCKey, a.[Description]

GO 

/*	Debugging:

	EXEC tas.Pr_GetUDCListItem
	EXEC tas.Pr_GetUDCListItem '55-SJ'

*/


