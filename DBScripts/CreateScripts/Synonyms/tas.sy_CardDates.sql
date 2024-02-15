/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_CardDates
*	Description: Get the details about the contractor's ID badge validity
*
*	Date:			Author:		Rev. #:		Comments:
*	15/07/2018		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_CardDates') IS NOT NULL
--	DROP SYNONYM tas.sy_CardDates
--GO

CREATE SYNONYM tas.sy_CardDates FOR GRMACC.AcsData.DSXUser.CardDates
GO

/*	Debugging:

	SELECT * FROM tas.sy_CardDates

*/

