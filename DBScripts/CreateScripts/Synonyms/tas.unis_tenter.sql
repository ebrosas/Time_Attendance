/*********************************************************************************
*	Revision History
*
*	Name: tas.unis_tenter
*	Description: Retrieves data from "[unis].[dbo].[tEnter_stg]" table
*
*	Date:			Author:		Rev.#:		Comments:
*	07/12/2021		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.unis_tenter') IS NOT NULL
		DROP SYNONYM tas.unis_tenter
	GO
		
	--CREATE SYNONYM tas.unis_tenter FOR GRMSQLDB.[UNIS].[dbo].[tEnter]			--Actual table that contains the original swipe data in UNIS system
	CREATE SYNONYM tas.unis_tenter FOR [unis].[dbo].[tEnter_stg]		--Staging table that contains unique swipe data in UNIS system (Notes: Implemented pointing to the staging table starting on 07-Dec-2021

GO


/*	Testing

	SELECT * FROM tas.unis_tenter

*/

