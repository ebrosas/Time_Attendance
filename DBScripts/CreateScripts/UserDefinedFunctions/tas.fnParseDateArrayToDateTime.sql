/*********************************************************************************
*	Revision History
*
*	Name: tas.fnParseDateArrayToDateTime
*	Description: Converts array of date string into a datetime table 
*
*	Date:			Author:		Rev. #:		Comments:
*	25/03/2019		Ervin		1.0			Created
**********************************************************************************/

ALTER FUNCTION tas.fnParseDateArrayToDateTime
(
	@dateArray	VARCHAR(500), 
	@delim		CHAR(1)
)
RETURNS @paramtable 
TABLE 
(
	DateValue DATETIME
) 
AS 

BEGIN

    DECLARE @len		INT,
            @index		INT,
            @nextindex  INT

    SET @len = DATALENGTH(@dateArray)
    SET @index = 0
    SET @nextindex = 0

    WHILE (@len > @index )
    BEGIN

		SET @nextindex = CHARINDEX(@delim, @dateArray, @index)

		IF (@nextindex = 0 ) 
			SET @nextindex = @len + 2

		INSERT @paramtable
		SELECT CAST(SUBSTRING(@dateArray, @index, @nextindex - @index ) AS DATETIME)

		SET @index = @nextindex + 1
	END
    
	RETURN
END

/*	Debug:

PARAMETER:
	@dateArray	VARCHAR(500), 
	@delim		CHAR(1)

	SELECT * FROM tas.fnParseDateArrayToDateTime('151119,151123,151127,151201,151205', ',')
*/
