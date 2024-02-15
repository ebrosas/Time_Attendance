/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_UserFormAccess
*	Description: Fetch data from "genuser.UserFormAccess" table
*
*	Date:			Author:		Rev. #:		Comments:
*	14/03/2017		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_UserFormAccess') IS NOT NULL
--	DROP SYNONYM tas.sy_UserFormAccess
--GO

CREATE SYNONYM tas.sy_UserFormAccess FOR Gen_Purpose.genuser.UserFormAccess
GO


/*	Debugging:

	SELECT * FROM tas.sy_UserFormAccess

*/

