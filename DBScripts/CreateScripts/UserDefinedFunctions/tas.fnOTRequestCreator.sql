/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnOTRequestCreator
*	Description: Get the email address of the overtime creator
*
*	Date:			Author:		Rev.#:		Comments:
*	31/12/2017		Ervin		1.0			Created
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnOTRequestCreator
(
	@assignedToEmpNo	INT,	
	@startDate			DATETIME,
	@endDate			DATETIME
)
RETURNS VARCHAR(1000)
AS
BEGIN

	--Initialize parameters
	IF ISNULL(@assignedToEmpNo, 0) = 0
		SET @assignedToEmpNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL
		
	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL		

	DECLARE	@empNo		INT,
			@empName	VARCHAR(100),
			@empEmail	VARCHAR(50),
			@result		VARCHAR(1000)

	--Initialize variables
	SELECT	@empNo		= 0,
			@empName	= '',
			@empEmail	= '',
			@result		= ''

	DECLARE OTRequestCreatorCursor CURSOR READ_ONLY FOR
	SELECT DISTINCT 
		b.CreatedByEmpNo,
		b.CreatedByEmpName,
		b.CreatedByEmail
	FROM tas.OvertimeWFEmailDelivery a
		INNER JOIN tas.OvertimeRequest b ON a.OTRequestNo = b.OTRequestNo
	WHERE 
		RTRIM(b.StatusHandlingCode) = 'Open'
		AND ISNULL(a.IsDelivered, 0) = 0
		AND a.CurrentlyAssignedEmpNo = @assignedToEmpNo 
		AND 
		(
			CONVERT(VARCHAR, a.CreatedDate, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			OR (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY b.CreatedByEmpNo

	OPEN OTRequestCreatorCursor
	FETCH NEXT FROM OTRequestCreatorCursor
	INTO @empNo, @empName, @empEmail

	WHILE @@FETCH_STATUS = 0
	BEGIN

		IF LEN(@result) = 0
			SET @result = RTRIM(@empEmail) 
		ELSE
			SET @result = @result + '; ' + RTRIM(@empEmail)

		--Retrieve next record
		FETCH NEXT FROM OTRequestCreatorCursor
		INTO @empNo, @empName, @empEmail
	END 

	--Close and deallocate
	CLOSE OTRequestCreatorCursor
	DEALLOCATE OTRequestCreatorCursor

	RETURN RTRIM(@result)

END

/*	Testing:

PARAMETERS:
	@assignedToEmpNo	INT,	
	@startDate			DATETIME,
	@endDate			DATETIME

	SELECT tas.fnOTRequestCreator(10003458, null, null)

*/
