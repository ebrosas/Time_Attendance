/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetMobileNo
*	Description: Get the employee's mobile no.
*
*	Date:			Author:		Rev.#:		Comments:
*	06/02/2017		Ervin		1.0			Created
*	06/02/2019		Ervin		1.1			Added filter condition that checks if WPCNLN = ''
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetMobileNo
(
	@empNo	INT
)
RETURNS VARCHAR(20)
AS
BEGIN

	DECLARE @mobileNo	VARCHAR(20)
	SET @mobileNo = ''

	SELECT TOP 1 @mobileNo = LTRIM(RTRIM(ISNULL(a.WPPH1, '')))
	FROM tas.syJDE_F0115 AS a
	WHERE a.WPAN8 = @empNo 
		AND UPPER(LTRIM(RTRIM(a.WPPHTP))) = 'MOBS'
		AND ISNULL(a.WPCNLN, '') = ''

	IF ISNULL(@mobileNo, '') = ''
	BEGIN
    
		SELECT TOP 1 @mobileNo = LTRIM(RTRIM(ISNULL(a.WPPH1, '')))
		FROM tas.syJDE_F0115 AS a
		WHERE a.WPAN8 = @empNo 
			AND UPPER(LTRIM(RTRIM(a.WPPHTP))) = 'MOBP'
			AND ISNULL(a.WPCNLN, '') = ''
	END 

	IF ISNULL(@mobileNo, '') = ''
	BEGIN
    
		SELECT TOP 1 @mobileNo = LTRIM(RTRIM(ISNULL(a.WPPH1, '')))
		FROM tas.syJDE_F0115 AS a
		WHERE a.WPAN8 = @empNo 
			AND UPPER(LTRIM(RTRIM(a.WPPHTP))) = 'MOB'
			AND ISNULL(a.WPCNLN, '') = ''
	END 
    
	IF ISNULL(@mobileNo, '')  <> ''
		SET @mobileNo = '+973' + @mobileNo

	RETURN @mobileNo

END

/*	Testing:

	SELECT tas.fnGetMobileNo(10003714)

*/
