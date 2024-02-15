/****************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCalculateTimeDifference
*	Description: This function is used to calculate the time difference between start time and end time
*
*	Date			Author		Rev.#		Comments
*	14/05/2019		Ervin		1.0					Created
****************************************************************************************************************************************/

ALTER FUNCTION tas.fnCalculateTimeDifference
(
	@startTime	VARCHAR(8),
	@endTime	VARCHAR(8)
)
RETURNS INT
AS
BEGIN

	DECLARE	@timeDiff	INT = 0,
			@sTime		TIME = NULL,
			@eTime		TIME = NULL 	

	IF ISDATE(@startTime) = 1
		SET @sTime = CAST(@startTime AS TIME)

	IF ISDATE(@endTime) = 1
		SET @eTime = CAST(@endTime AS TIME)

	IF @sTime IS NOT NULL AND @eTime IS NOT NULL	
	BEGIN

		SET @timeDiff = DATEDIFF(MINUTE, @sTime, @eTime)
    END 

	RETURN @timeDiff
END 

/*	Debug:

	SELECT tas.fnCalculateTimeDifference('13:00:00', '16:30:00')

*/