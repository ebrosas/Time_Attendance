/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F08336
*	Description: Retrieves data from "F08336" table
*
*	Date:			Author:		Rev.#:		Comments:
*	09/06/2016		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F08336') IS NOT NULL
		DROP SYNONYM tas.sy_F08336
	GO

	CREATE SYNONYM tas.sy_F08336 FOR JDE_CRP.CRPDTA.F08336				--Test server
	--CREATE SYNONYM tas.sy_F08336 FOR JDE_PRODUCTION.PRODDTA.F08336		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F08336

*/

