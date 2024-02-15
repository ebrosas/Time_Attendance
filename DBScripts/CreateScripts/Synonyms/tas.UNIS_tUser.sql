/*********************************************************************************
*	Revision History
*
*	Name: tas.UNIS_tUser
*	Description: Retrieves data from "[unis].[dbo].[tUser]" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/12/2021		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.UNIS_tUser') IS NOT NULL
		DROP SYNONYM tas.UNIS_tUser
	GO
		
	CREATE SYNONYM tas.UNIS_tUser FOR GRMSQLDB.[UNIS].[dbo].[tUser]			--Test server
	--CREATE SYNONYM tas.UNIS_tUser FOR [unis].[dbo].[tUser]						--Production

GO


/*	Testing

	SELECT * FROM tas.UNIS_tUser

*/

