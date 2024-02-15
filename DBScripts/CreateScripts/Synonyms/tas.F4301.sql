/*********************************************************************************
*	Revision History
*
*	Name: tas.F4301
*	Description: Retrieves data from "F4301" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/12/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.F4301') IS NOT NULL
	--	DROP SYNONYM tas.F4301
	--GO

	--CREATE SYNONYM tas.F4301 FOR JDE_CRP.CRPDTA.F4301					--Test server
	CREATE SYNONYM tas.F4301 FOR JDE_PRODUCTION.PRODDTA.F4301			--Production server

GO


/*	Testing

	SELECT * FROM tas.F4301

*/

