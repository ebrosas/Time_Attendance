/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F01112
*	Description: Retrieves data from "F01112" table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/08/2018		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F01112') IS NOT NULL
		DROP SYNONYM tas.sy_F01112
	GO

	--CREATE SYNONYM tas.sy_F01112 FOR JDE_CRP.CRPDTA.F01112				--Test server
	CREATE SYNONYM tas.sy_F01112 FOR JDE_PRODUCTION.PRODDTA.F01112		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F01112

*/

