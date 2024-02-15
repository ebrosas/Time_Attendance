/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F07350
*	Description: Retrieves data from "F07350" table
*
*	Date:			Author:		Rev.#:		Comments:
*	23/01/2018		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.sy_F07350') IS NOT NULL
	--	DROP SYNONYM tas.sy_F07350
	--GO

	--CREATE SYNONYM tas.sy_F07350 FOR JDE_CRP.CRPDTA.F07350				--Test server
	CREATE SYNONYM tas.sy_F07350 FOR JDE_PRODUCTION.PRODDTA.F07350		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F07350 a
	SELECT TOP 1 a.Y0AN8, * FROM tas.sy_F07350 a

*/

