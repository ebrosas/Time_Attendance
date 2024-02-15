/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fmtMIN_HHmm
*	Description: Converts minute to HH:mm format
*
*	Date:			Author:		Rev. #:		Comments:
*	12/12/2011		Zaharan		1.0			Created
*	09/08/2019		Ervin		1.1			Refactored the code and implemented special holiday where type is "HE"
************************************************************************************************************************************************/

ALTER FUNCTION tas.isDateOnPublicHoliday
(
	@DT AS DATETIME
) 
RETURNS BIT
AS
BEGIN

	DECLARE @ret BIT,
			@cnt INT

	IF EXISTS
	(
		SELECT TOP 1 1 
		FROM tas.Master_Calendar a WITH (NOLOCK)
		WHERE a.HolidayDate = @DT
			AND UPPER(LTRIM(RTRIM(a.HolidayType))) IN ('H', 'HE') --=S.Code_PublicHolidy
	)
		SET @ret = 1
	ELSE
		SET @ret = 0

	RETURN @ret
END

/*	Debug:

	SELECT tas.isDateOnPublicHoliday('08/10/2019')
	SELECT tas.isDateOnPublicHoliday('08/11/2019')
	SELECT tas.isDateOnPublicHoliday('08/12/2019')
	SELECT tas.isDateOnPublicHoliday('08/13/2019')
	SELECT tas.isDateOnPublicHoliday('08/14/2019')

*/


