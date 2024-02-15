/*********************************************************************************
*	Revision History
*
*	Name: tas.unis_tenter_alpeta
*	Description: Retrieves data from "unis.dbo.tEnter_stg_alpeta" table
*
*	Date:			Author:		Rev.#:		Comments:
*	10/04/2022		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.tEnter_stg_alpeta') IS NOT NULL
	--	DROP SYNONYM tas.unis_tenter_alpeta
	--GO
		
	CREATE SYNONYM tas.unis_tenter_alpeta FOR unis.dbo.tEnter_stg_alpeta		--Staging table that contains unique swipe data in UNIS system (Notes: Implemented pointing to the staging table starting on 07-Dec-2021

GO


/*	Testing

	SELECT * FROM tas.unis_tenter_alpeta

*/

