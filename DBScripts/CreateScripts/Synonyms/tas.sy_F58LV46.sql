/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F58LV46
*	Description: Retrieves data from "F58LV46" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/06/2016		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F58LV46') IS NOT NULL
		DROP SYNONYM tas.sy_F58LV46
	GO

	CREATE SYNONYM tas.sy_F58LV46 FOR JDE_CRP.CRPDTA.F58LV46				--Test server
	--CREATE SYNONYM tas.sy_F58LV46 FOR JDE_PRODUCTION.PRODDTA.F58LV46		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F58LV46

*/

