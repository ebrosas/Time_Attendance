/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPointer
*	Description: Retrieves shift pointer information
*
*	Date			Author		Revision No.	Comments:
*	13/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

CREATE PROCEDURE tas.Pr_GetShiftPointer
(
	@shiftPatCode	VARCHAR(2) 
)
AS

	SELECT	a.ShiftPointer, 
			tas.lpad(a.ShiftPointer, 2, CHAR(160)) + ' ' + RTRIM(a.ShiftCode) AS PointerCode 
	FROM tas.Master_ShiftPattern a 
	WHERE 
		RTRIM(a.ShiftPatCode) = RTRIM(@shiftPatCode) 
	ORDER BY a.ShiftPointer

GO 

/*	Debugging:

	EXEC tas.Pr_GetShiftPointer 'D5'

*/


