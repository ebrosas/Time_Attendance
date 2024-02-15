/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F08901
*	Description: Retrieves data from "F08901" table
*
*	Date:			Author:		Rev.#:		Comments:
*	09/06/2016		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F08901') IS NOT NULL
		DROP SYNONYM tas.sy_F08901
	GO

	CREATE SYNONYM tas.sy_F08901 FOR JDE_CRP.CRPDTA.F08901				--Test server
	--CREATE SYNONYM tas.sy_F08901 FOR JDE_PRODUCTION.PRODDTA.F08901		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F08901

*/

