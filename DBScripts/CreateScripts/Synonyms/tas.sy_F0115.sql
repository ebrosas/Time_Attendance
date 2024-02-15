/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F0115
*	Description: Retrieves data from "F0115" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/08/2018		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.sy_F0115') IS NOT NULL
	--	DROP SYNONYM tas.sy_F0115
	--GO

	--CREATE SYNONYM tas.sy_F0115 FOR JDE_CRP.CRPDTA.F0115				--Test server
	CREATE SYNONYM tas.sy_F0115 FOR JDE_PRODUCTION.PRODDTA.F0115		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F0115

*/

