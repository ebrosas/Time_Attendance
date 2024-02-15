/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPatternCodes
*	Description: Retrieves all shift patterns
*
*	Date			Author		Revision No.	Comments:
*	13/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftPatternCodes
(
	@shiftPatCode	VARCHAR(2) = NULL
)
AS

	IF ISNULL(@shiftPatCode, '') = ''
		SET @shiftPatCode = NULL

	SELECT a.Code, a.[Description]
	FROM tas.vuDropdown_ShiftPattern a
	WHERE 
		(RTRIM(a.Code) = @shiftPatCode OR @shiftPatCode IS NULL)
	ORDER BY a.Code

GO 

/*	Debugging:

	EXEC tas.Pr_GetShiftPatternCodes 'D'

*/


