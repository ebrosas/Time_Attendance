/*********************************************************************************
*	Revision History
*
*	Name: tas.UNIS_iUserCard
*	Description: Retrieves data from "[unis].[dbo].[iUserCard]" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/12/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.UNIS_iUserCard') IS NOT NULL
	--	DROP SYNONYM tas.UNIS_iUserCard
	--GO
		
	CREATE SYNONYM tas.UNIS_iUserCard FOR GRMSQLDB.[UNIS].[dbo].[iUserCard]			--Test server
	--CREATE SYNONYM tas.UNIS_iUserCard FOR [unis].[dbo].[iUserCard]						--Production

GO


/*	Testing

	SELECT * FROM tas.UNIS_iUserCard

*/

