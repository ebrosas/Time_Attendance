/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ShiftPatternCodes
*	Description: Fetches all shift pattern codes used in the company
*
*	Date:			Author:		Rev. #:		Comments:
*	22/10/2017		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ShiftPatternCodes
AS		

	SELECT	DISTINCT
			RTRIM(a.ShiftPatCode) AS ShiftPatCode,
			RTRIM(a.ShiftPatCode) + ' - ' + tas.getFullPattern(RTRIM(a.ShiftPatCode)) AS ShiftPatDesc
	FROM tas.Master_ShiftPattern a

GO 

/* Testing:

	SELECT * FROM tas.Vw_ShiftPatternCodes a
	
*/