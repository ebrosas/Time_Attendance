/*********************************************************************************
*	Revision History
*
*	Name: tas.UNIS_iUserPicture
*	Description: Retrieves data from "[unis].[dbo].[iUserPicture]" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/12/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.UNIS_iUserPicture') IS NOT NULL
	--	DROP SYNONYM tas.UNIS_iUserPicture
	--GO
		
	CREATE SYNONYM tas.UNIS_iUserPicture FOR GRMSQLDB.[UNIS].[dbo].[iUserPicture]			--Test server
	--CREATE SYNONYM tas.UNIS_iUserPicture FOR [unis].[dbo].[iUserPicture]						--Production

GO


/*	Testing

	SELECT * FROM tas.UNIS_iUserPicture

*/

