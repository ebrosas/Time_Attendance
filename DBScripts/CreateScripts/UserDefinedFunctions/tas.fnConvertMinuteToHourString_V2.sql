/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnConvertMinuteToHourString_V2
*	Description: Converts minute input value into hours with the following format: HH:mm
*
*	Date:			Author:		Rev.#:		Comments:
*	11/07/2017		Ervin		1.0			Created
*******************************************************************************************************************************************************/

CREATE FUNCTION tas.fnConvertMinuteToHourString_V2
(
	@minuteValue INT 
)
RETURNS VARCHAR(10)
AS 
BEGIN

	DECLARE @result		VARCHAR(10),
			@hour		VARCHAR(5),
			@minute		VARCHAR(5)

	--Initialize the return value
	SET @result = ''

	IF @minuteValue > 0
	BEGIN
    
		SET	@hour = @minuteValue / 60
		SET @minute = @minuteValue - (@hour * 60)

		SET @result = tas.lpad(@hour, 5, '0') + ':' + tas.lpad(@minute, 2, '0')
	END 

	--Remove leading zeros
	RETURN SUBSTRING(@result, PATINDEX('%[^0]%', @result), 10)

END

/*	Debug:

	SELECT tas.fnConvertMinuteToHourString_V2(10)

*/

