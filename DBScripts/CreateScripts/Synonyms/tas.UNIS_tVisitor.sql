/*********************************************************************************
*	Revision History
*
*	Name: tas.UNIS_tVisitor
*	Description: Retrieves data from "[unis].[dbo].[tVisitor]" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/12/2021		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.UNIS_tVisitor') IS NOT NULL
		DROP SYNONYM tas.UNIS_tVisitor
	GO
		
	CREATE SYNONYM tas.UNIS_tVisitor FOR GRMSQLDB.[UNIS].[dbo].[tVisitor]			--Test server
	--CREATE SYNONYM tas.UNIS_tVisitor FOR [unis].[dbo].[tVisitor]						--Production

GO


/*	Testing

	SELECT * FROM tas.UNIS_tVisitor

*/

