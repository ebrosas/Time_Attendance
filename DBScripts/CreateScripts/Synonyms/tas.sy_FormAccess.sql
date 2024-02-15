/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_FormAccess
*	Description: Get all forms from the Common Admin System
*
*	Date:			Author:		Rev. #:		Comments:
*	10/11/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_FormAccess') IS NOT NULL
--	DROP SYNONYM tas.sy_FormAccess
--GO

CREATE SYNONYM tas.sy_FormAccess FOR Gen_Purpose.genuser.FormAccess
GO


/*	Debugging:

	SELECT * FROM tas.sy_FormAccess

*/

