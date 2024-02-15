/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetRequiredWorkDuration
*	Description: Get the total required work duration in minutes
*
*	Date:			Author:		Rev.#:		Comments:
*	09/11/2016		Ervin		1.0			Created
*	15/01/2017		Ervin		1.1			Added condition that checks if "@shiftPatCode" or "@shiftCode" is NULL
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetRequiredWorkDuration
(
	@shiftPatCode	VARCHAR(2),
	@shiftCode		VARCHAR(10)
)
RETURNS INT
AS
BEGIN

	DECLARE @duration	INT
	SET @duration = 0

	IF ISNULL(@shiftPatCode, '') <> '' AND ISNULL(@shiftCode, '') <> ''
	BEGIN
    
		SELECT	@duration = DATEDIFF(n, a.ArrivalTo, a.DepartFrom)
		FROM tas.Master_ShiftTimes a
		WHERE RTRIM(a.ShiftPatCode) = RTRIM(@shiftPatCode)
			AND RTRIM(a.ShiftCode) = RTRIM(@shiftCode)

		IF @duration < 0
		BEGIN
    
			SELECT	@duration = DATEDIFF(n, a.ArrivalTo, a.DepartFrom) + (24 * 60)
			FROM tas.Master_ShiftTimes a
			WHERE RTRIM(a.ShiftPatCode) = RTRIM(@shiftPatCode)
				AND RTRIM(a.ShiftCode) = RTRIM(@shiftCode)
		END
	END 
    
	RETURN ISNULL(@duration, 0)

END

/*	Testing:

PARAMETERS:
	@shiftPatCode	VARCHAR(2),
	@shiftCode		VARCHAR(10)
	
	SELECT tas.fnGetRequiredWorkDuration('I', 'O')

*/
