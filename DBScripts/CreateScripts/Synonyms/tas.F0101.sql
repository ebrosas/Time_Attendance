/*********************************************************************************
*	Revision History
*
*	Name: tas.F0101
*	Description: Retrieves data from "F0101" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/12/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.F0101') IS NOT NULL
	--	DROP SYNONYM tas.F0101
	--GO

	--CREATE SYNONYM tas.F0101 FOR JDE_CRP.CRPDTA.F0101					--Test server
	CREATE SYNONYM tas.F0101 FOR JDE_PRODUCTION.PRODDTA.F0101			--Production server

GO


/*	Testing

	SELECT * FROM tas.F0101

*/

