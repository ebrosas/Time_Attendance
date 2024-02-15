/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_Master_ShiftPatternTitles
*	Description: Get the shift pattern information
*
*	Date:			Author:		Rev. #:		Comments:
*	22/03/2017		Ervin		1.0			Created
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_Master_ShiftPatternTitles
AS
	
	SELECT [AutoID] AS Shift_AutoID
		  ,[ShiftPatCode]
		  ,[ShiftPatDescription]
		  ,[IsDayShift]
		  ,[LastUpdateUser]
		  ,[LastUpdateTime]
	FROM tas.Master_ShiftPatternTitles a

GO 

/*	Debugging:

	SELECT * FROM tas.Vw_Master_ShiftPatternTitles

*/