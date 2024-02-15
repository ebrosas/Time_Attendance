/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_COMPANY
*	Description: Retrieves the cost centers
*
*	Date:			Author:		Rev. #:		Comments:
*	31/08/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_COMPANY') IS NOT NULL
--	DROP SYNONYM tas.sy_COMPANY
--GO

CREATE SYNONYM tas.sy_COMPANY FOR GRMACC.AcsData.dbo.COMPANY
--CREATE SYNONYM tas.sy_COMPANY FOR SWIPELNK.AcsData.dbo.COMPANY

GO



/*	Debugging:

	SELECT * FROM tas.sy_COMPANY

*/

