/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F069096
*	Description: Retrieves data from "F069096" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/06/2016		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F069096') IS NOT NULL
		DROP SYNONYM tas.sy_F069096
	GO

	CREATE SYNONYM tas.sy_F069096 FOR JDE_CRP.CRPDTA.F069096				--Test server
	--CREATE SYNONYM tas.sy_F069096 FOR JDE_PRODUCTION.PRODDTA.F069096		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F069096

*/

