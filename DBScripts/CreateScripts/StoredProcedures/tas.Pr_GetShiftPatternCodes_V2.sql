/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPatternCodes_V2
*	Description: Retrieves all shift patterns
*
*	Date			Author		Revision No.	Comments:
*	22/10/2017		Ervin		1.1				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftPatternCodes_V2
(
	@shiftPatCode	VARCHAR(2) = NULL
)
AS

	IF ISNULL(@shiftPatCode, '') = ''
		SET @shiftPatCode = NULL

	SELECT	a.ShiftPatCode AS Code,
			a.ShiftPatDesc AS [Description],
			b.RestrictionType,
			b.RestrictedEmpNoArray,
			b.RestrictedCostCenterArray,
			RTRIM(b.ErrorMessage) AS RestrictionMessage
	FROM tas.Vw_ShiftPatternCodes a
		LEFT JOIN tas.ShiftPatternRestriction b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
	WHERE (RTRIM(a.ShiftPatCode) = RTRIM(@shiftPatCode) OR @shiftPatCode IS NULL)

GO 

/*	Debugging:

	EXEC tas.Pr_GetShiftPatternCodes_V2 'D'

*/


