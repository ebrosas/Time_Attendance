/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F060120
*	Description: Retrieves data from "F060120" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/08/2018		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.sy_F060120') IS NOT NULL
	--	DROP SYNONYM tas.sy_F060120
	--GO

	--CREATE SYNONYM tas.sy_F060120 FOR JDE_CRP.CRPDTA.F060120				--Test server
	CREATE SYNONYM tas.sy_F060120 FOR JDE_PRODUCTION.PRODDTA.F060120		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F060120

*/

