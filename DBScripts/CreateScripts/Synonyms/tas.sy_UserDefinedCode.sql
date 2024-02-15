/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_UserDefinedCode
*	Description: Retrieves data from "Gen_Purpose.genuser.UserDefinedCode" table
*
*	Date:			Author:		Rev.#:		Comments:
*	23/08/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.sy_UserDefinedCode') IS NOT NULL
	--	DROP SYNONYM tas.sy_UserDefinedCode
	--GO

	CREATE SYNONYM tas.sy_UserDefinedCode FOR Gen_Purpose.genuser.UserDefinedCode

GO


/*	Testing

	SELECT * FROM tas.sy_UserDefinedCode

*/

