/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F0902
*	Description: Retrieves data from "F0902" table
*
*	Date:			Author:		Rev.#:		Comments:
*	06/03/2018		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.sy_F0902') IS NOT NULL
	--	DROP SYNONYM tas.sy_F0902
	--GO

	--CREATE SYNONYM tas.sy_F0902 FOR JDE_CRP.CRPDTA.F0902				--Test server
	CREATE SYNONYM tas.sy_F0902 FOR JDE_PRODUCTION.PRODDTA.F0902		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F0902

*/

