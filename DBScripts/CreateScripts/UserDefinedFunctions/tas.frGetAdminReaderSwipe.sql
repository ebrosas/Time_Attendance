/*****************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.frGetAdminReaderSwipe
*	Description: This table-view function returns the processed Admin Bldg. reader swipe data for specific employee
*
*	Date:			Author:		Rev.#:		Comments:
*	12/04/2022		Ervin		1.0			Created
******************************************************************************************************************************************************************************************************/

ALTER FUNCTION tas.frGetAdminReaderSwipe
(
	@empNo				INT,
	@processDate		DATETIME
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo			INT,
	DT				DATETIME,
	LocationCode	INT,
	ReaderNo		INT,
	EventCode		INT,
	SwipeType		VARCHAR(10),
	SwipeLocation	VARCHAR(50),
	[Source]		CHAR(1)
)
AS
BEGIN

	DECLARE	@timeIN				DATETIME = NULL,
			@timeOUT			DATETIME = NULL,
			@locationCode		INT = NULL,
			@locationName		VARCHAR(50) = NULL,
			@readerNo			INT = NULL,
			@eventCode			INT = NULL,
			@swipeType			VARCHAR(10) = NULL,
			@swipeSource		CHAR(1) = NULL,
			@swipeCount			INT = 0,
			@mainGateLastSwipe	VARCHAR(3)			
	
	--Get the total swipe count for the day
	SELECT @swipeCount = COUNT(*)
	FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK) 
	WHERE a.EmpNo = @empNo
		AND a.SwipeDate = @processDate

	--Get the common details
	SELECT TOP 1 
		@locationCode = a.LocationCode,
		@readerNo = a.ReaderNo,
		@eventCode = a.EventCode,
		@locationName = RTRIM(a.LocationName),
		@swipeSource = a.[Source]
	FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK) 
	WHERE a.EmpNo = @empNo
		AND a.SwipeDate = @processDate

	--Get the first time-in			
	SELECT TOP 1 @timeIN = a.SwipeDateTime 
	FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK) 
	WHERE a.EmpNo = @empNo
		AND a.SwipeDate = @processDate
	ORDER BY a.SwipeTime

	--Get the last time-out
	SELECT TOP 1 @timeOUT = a.SwipeDateTime 
	FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK) 
	WHERE a.EmpNo = @empNo
		AND a.SwipeDate = @processDate
	ORDER BY a.SwipeTime DESC

	SELECT TOP 1 @mainGateLastSwipe = UPPER(RTRIM(a.SwipeType)) 
	FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK) 
	WHERE EmpNo = @empNo
		AND SwipeDate = @processDate
	ORDER BY SwipeTime DESC

	--Set time-out to null if there is no matching time-out at the main gate
	--IF NOT EXISTS
 --   (
	--	SELECT TOP 1 * FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK) 
	--	WHERE a.EmpNo = @empNo
	--		AND a.SwipeDate = @processDate
	--		AND RTRIM(a.SwipeType) = 'OUT'
	--	ORDER BY a.SwipeTime DESC
	--)
	IF @mainGateLastSwipe <> 'OUT' OR @swipeCount <= 1
	SET @timeOUT = NULL

	IF @timeIN IS NOT NULL AND @timeOUT IS NOT NULL	
	BEGIN
    
		INSERT INTO @rtnTable 
		SELECT	@empNo, 
				@timeIN, 
				@locationCode,
				@readerNo,
				@eventCode,
				'IN' AS SwipeType, 
				@locationName,
				@swipeSource

		UNION
    
		SELECT	@empNo, 
				@timeOUT, 
				@locationCode,
				@readerNo,
				@eventCode,
				'OUT' AS SwipeType, 				
				@locationName,
				@swipeSource
	END
	ELSE BEGIN

		IF @timeIN IS NOT NULL AND @timeOUT IS NULL
		BEGIN

			INSERT INTO @rtnTable 
			SELECT	@empNo, 
					@timeIN, 
					@locationCode,
					@readerNo,
					@eventCode,
					'IN' AS SwipeType, 
					@locationName,
					@swipeSource
        END 
		ELSE IF @timeIN IS NULL AND @timeOUT IS NOT NULL
		BEGIN

			INSERT INTO @rtnTable 
			SELECT	@empNo, 
					@timeOUT, 
					@locationCode,
					@readerNo,
					@eventCode,
					'OUT' AS SwipeType, 				
					@locationName,
					@swipeSource
        END 
    END 

	RETURN 

END


/*	Testing:

PARAMETERS:
	@empNo				INT,
	@processDate		DATETIME

	SELECT * FROM tas.frGetAdminReaderSwipe(10003605, '04/10/2022')
	SELECT * FROM tas.frGetAdminReaderSwipe(10003605, '04/04/2022')
	SELECT * FROM tas.frGetAdminReaderSwipe(10003632, '04/13/2022')

*/
