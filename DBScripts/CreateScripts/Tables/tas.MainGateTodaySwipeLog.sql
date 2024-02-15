/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.MainGateTodaySwipeLog
*	Description: This table stores the swipe data of all employees in the Main Gate and Foil Mill Gate barrier and turnstile
*
*	Date			Author		Rev.#		Comments
*	18/04/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.MainGateTodaySwipeLog') IS NOT NULL
BEGIN	

	DROP TABLE tas.MainGateTodaySwipeLog
END

	CREATE TABLE tas.MainGateTodaySwipeLog
	(
		LogID			INT IDENTITY(1,1) NOT NULL,	
		EmpNo			INT,
		SwipeDate		DATETIME,
		SwipeTime		DATETIME,
		SwipeLocation	VARCHAR(100),
		SwipeType		VARCHAR(3),
		ShiftPatCode	VARCHAR(10),
		ShiftCode		VARCHAR(10),
		CreatedDate		DATETIME DEFAULT GETDATE()

		CONSTRAINT [PK_MainGateTodaySwipeLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			SwipeDate,
			SwipeTime,
			SwipeType
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.MainGateTodaySwipeLog a
	ORDER BY EmpNo, SwipeTime


	TRUNCATE TABLE tas.MainGateTodaySwipeLog

	BEGIN TRAN T1
	
	INSERT INTO [tas].[MainGateTodaySwipeLog]
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
	FROM tas.fnGetAllEmployeeSwipe('04/18/2019', 0)
	ORDER BY EmpNo, SwipeTime ASC	

	COMMIT TRAN T1
	ROLLBACK TRAN T1

*/
