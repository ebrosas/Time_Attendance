/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_Vw_LIC_PrintedCards
*	Description: Get detailed information about a Contractor
*
*	Date:			Author:		Rev. #:		Comments:
*	13/10/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_Vw_LIC_PrintedCards') IS NOT NULL
--	DROP SYNONYM tas.sy_Vw_LIC_PrintedCards
--GO

CREATE SYNONYM tas.sy_Vw_LIC_PrintedCards FOR GRMACC.AcsData.DSXUser.Vw_LIC_PrintedCards
GO

/*	Debugging:

	SELECT * FROM tas.sy_Vw_LIC_PrintedCards

*/

