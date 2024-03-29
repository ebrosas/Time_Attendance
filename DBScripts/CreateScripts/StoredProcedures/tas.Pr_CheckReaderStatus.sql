/***************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_CheckReaderStatus
*	Description: This stored procedure is used to check the status of all readers at the workplace before running the Timesheet Process.
*
*	Date:			Author:		Rev.#:		Comments:
*	08/03/2016		Ervin		1.0			Created
*	09/11/2016		Ervin		1.1			Changed the value of @CONST_TIMESHEET_TIME constant into 09:10 AM
*	24/01/2021		Ervin		1.2			Set the cut-off time from 09:10 AM to 08:30 AM
*	01/02/2021		Ervin		1.3			Fixed the bug reported by HR through Helpdesk No. 122294 wherein overtime is calculated incorrectly once modified by HR 
*	19/03/2021		Ervin		1.4			Commented condition that checks if all workplace readers are offline since all readers are now using the new database
***************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_CheckReaderStatus
(
	@processDate	DATETIME = NULL,
	@readerNo		INT	= 0 
)
AS	
    
	SET NOCOUNT ON

	IF ISNULL(@processDate, '') = ''
		SET @processDate = NULL

	IF ISNULL(@readerNo, 0) = 0
		SET @readerNo = NULL

	--Define variables
	DECLARE	@CONST_TIMESHEET_TIME			DATETIME,
			@totalReaders					INT,
			@totalFaultyReaders				INT,
			@maxLastRead					DATETIME,
			@proceedExecuteTS				BIT,
			@isTimesheetSuccess				BIT,
			@shiftPatternLastUpdated		DATETIME,
			@swipeLastProcessed				DATETIME

	--Initialize variables
	SELECT	--@CONST_TIMESHEET_TIME		= DATEADD(n, 10, DATEADD(hh, 9, CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)))),		--Cut-off Time: 09:10 AM
			--@CONST_TIMESHEET_TIME		= DATEADD(n, 30, DATEADD(hh, 8, CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)))),		--Cut-off Time: 08:30 AM		--Rev. #1.2
			@CONST_TIMESHEET_TIME		= DATEADD(hh, 8, CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))),						--Cut-off Time: 08:00 AM		--Rev. #1.3
			@totalReaders				= 0,
			@totalFaultyReaders			= 0,
			@maxLastRead				= NULL,
			@proceedExecuteTS			= 1,
			@isTimesheetSuccess			= 0,
			@shiftPatternLastUpdated	= NULL,
			@swipeLastProcessed			= NULL 

	--Get total count of all readers
	SELECT @totalReaders = COUNT(ReaderID) 
	FROM tas.sy_ReaderStatus a WITH (NOLOCK)
	WHERE a.ReaderID NOT In 
		(
			2,		--Remelt Control Room2
			3,		--CM1 Floor Intercom 
			4,		--CM2 Floor Intercom 
			13,		--EMD Workshop                  
			14,		--MMD Workshop
			15,		--Roll Grinder 1           
			16,		--Water Treatment       
			17,		--Annealling 123
			18,		--Annealling 456
			19		--Services
		)

	--Get total count of all faulty readers
	SELECT @totalFaultyReaders = COUNT(ReaderID) 
	FROM tas.sy_ReaderStatus a WITH (NOLOCK)
	WHERE a.ReaderID NOT In 
		(
			2,		--Remelt Control Room2
			3,		--CM1 Floor Intercom 
			4,		--CM2 Floor Intercom 
			13,		--EMD Workshop                  
			14,		--MMD Workshop
			15,		--Roll Grinder 1           
			16,		--Water Treatment       
			17,		--Annealling 123
			18,		--Annealling 456
			19		--Services
		)
		AND a.LastRead < @CONST_TIMESHEET_TIME

	--Get the maximum value of "LastRead" field
	SELECT TOP 1 @maxLastRead = LastRead 
	FROM tas.sy_ReaderStatus a WITH (NOLOCK) 
	WHERE a.ReaderID NOT In 
		(
			2,		--Remelt Control Room2
			3,		--CM1 Floor Intercom 
			4,		--CM2 Floor Intercom 
			13,		--EMD Workshop                  
			14,		--MMD Workshop
			15,		--Roll Grinder 1           
			16,		--Water Treatment       
			17,		--Annealling 123
			18,		--Annealling 456
			19		--Services
		)
	ORDER BY a.LastRead DESC 

	--Check if Timesheet Processing Service completed successfully
	SELECT	@shiftPatternLastUpdated = a.DT_ShiftPatternLastUpdated,
			@swipeLastProcessed = CONVERT(DATETIME, CONVERT(VARCHAR, a.DT_SwipeLastProcessed, 12))
	FROM tas.System_Values  a WITH (NOLOCK)

	--Check if need to execute Timesheet and Plant Swipe related services
	IF	--@totalReaders = @totalFaultyReaders OR					--Notes: Disable processing if all workplace readers failed to synchronize data to the database server		--Rev. #1.4
		@shiftPatternLastUpdated <> @swipeLastProcessed			--Notes: Disable processing if Timesheet Processing Service failed to execute
		SET @proceedExecuteTS = 0

	--For testing purpose uncomment the code below
	--SET @proceedExecuteTS = 0

	SELECT	@totalReaders		AS TotalReaders,
			@totalFaultyReaders	AS TotalFaultyReaders,
			@maxLastRead		AS MaxLastRead,
			@proceedExecuteTS	AS ProceedExecuteTS


/*	Debugging:

PARAMETER:
	@processDate	DATETIME = NULL,
	@readerNo		INT	= 0 

	SELECT a.DT_ShiftPatternLastUpdated, CONVERT(DATETIME, CONVERT(VARCHAR, a.DT_SwipeLastProcessed, 12)) AS DT_SwipeLastProcessed FROM tas.System_Values  a

	EXEC tas.Pr_CheckReaderStatus '03/19/2021', 0

*/