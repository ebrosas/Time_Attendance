/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_F55LVINQ
*	Description: Retrieves data from "F55LVINQ" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/06/2016		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_F55LVINQ') IS NOT NULL
		DROP SYNONYM tas.sy_F55LVINQ
	GO

	CREATE SYNONYM tas.sy_F55LVINQ FOR JDE_CRP.CRPDTA.F55LVINQ				--Test server
	--CREATE SYNONYM tas.sy_F55LVINQ FOR JDE_PRODUCTION.PRODDTA.F55LVINQ		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_F55LVINQ

*/

