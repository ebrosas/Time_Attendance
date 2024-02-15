/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_PopulateMainGateSwipe
*	Description: This stored procedure is used to populate records in tas.MainGateTodaySwipeLog table
*
*	Date			Author		Revision No.	Comments:
*	18/04/2019		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_PopulateMainGateSwipe
(	
	@processDate	DATETIME
)
AS	

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT,
			@CONST_MARO				VARCHAR(10)

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0

	IF 
	(
		SELECT COUNT(*) FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
		WHERE a.SwipeDate = @processDate
	) > 0		
	BEGIN
    
		--Delete existing records
		DELETE FROM tas.MainGateTodaySwipeLog		
		WHERE SwipeDate = @processDate
	END 

	INSERT INTO tas.MainGateTodaySwipeLog
    (
		EmpNo,
		SwipeDate,
		SwipeTime,
		SwipeLocation,
		SwipeType,
		ShiftPatCode,
		ShiftCode		
	)
	SELECT	DISTINCT
			EmpNo,
			SwipeDate,
			SwipeTime,
			SwipeLocation,
			SwipeType,
			ShiftPatCode,
			ShiftCode	 
	FROM tas.fnGetAllEmployeeSwipe(@processDate, 0)
	ORDER BY EmpNo, SwipeTime ASC	

	--Get the number of affected records 
	SELECT @rowsAffected = @@rowcount 

	--Checks for error
	IF @@ERROR <> @CONST_RETURN_OK
	BEGIN
				
		SELECT	@retError = @CONST_RETURN_ERROR,
				@retErrorDesc = 'An unspecified error occured while populating the data in MainGateTodaySwipeLog table.',
				@hasError = 1
	END

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected

GO 


/*	Debug:

PARAMETERS:
	@processDate	DATETIME
	
	EXEC tas.Pr_PopulateMainGateSwipe '04/18/2019'

	SELECT * FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
	ORDER BY EmpNo, SwipeTime

*/