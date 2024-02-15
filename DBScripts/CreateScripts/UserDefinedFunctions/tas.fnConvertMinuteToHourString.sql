/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnConvertMinuteToHourString
*	Description: Converts minute input value into hours with the following format: HH:mm
*
*	Date:			Author:		Rev.#:		Comments:
*	30/11/2016		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER FUNCTION tas.fnConvertMinuteToHourString
(
	@minuteValue INT 
)
RETURNS VARCHAR(5)
AS 
BEGIN

	DECLARE @result		VARCHAR(5),
			@hour		VARCHAR(2),
			@minute		VARCHAR(2)

	--Initialize return value
	SET @result = ''

	IF @minuteValue > 0
	BEGIN
    
		SET	@hour = @minuteValue / 60
		SET @minute = @minuteValue - (@hour * 60)

		SET @result = tas.lpad(@hour, 2, '0') + ':' + tas.lpad(@minute, 2, '0')
	END 

	RETURN @result

END

/*	Debug:

	SELECT tas.fnConvertMinuteToHourString(10)

*/

