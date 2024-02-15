/****************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Tran_SwipeData_dsx1
*	Description: Retrieves the swipe data from the Main Gate, Foil Mill, and Car Park #5 readers
*
*	Date:			Author:		Rev. #:		Comments:
*	03/09/2013		Ervin		1.0			Created
*	22/03/2020		Ervin		1.1			Refactored the logic in fetching the employee no. value
*	08/08/2020		Ervin		1.2			Added union to "Vw_CarParkSwipeData" view
*	11/11/2020		Ervin		1.3			Added union to "Vw_NewReaderSwipeData" view
*	06/01/2021		Ervin		1.4			Return the top 500 records from Vw_NewReaderSwipeData
*	31/03/2021		Ervin		1.5			Refactored the logic in fetching the value for "EventCode" field
*	22/02/2022		Ervin		1.6			Commented code that fetch data from the old Access System database
*	02/03/2022		Ervin		1.7			Added filter to return the last 3 months of swipe data
*****************************************************************************************************************************************/

ALTER VIEW tas.Tran_SwipeData_dsx1
AS 

	/*	Rev. #1.6
	SELECT TOP 5000	
		EmpNo = CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN CONVERT(INT, a.FName) <= 9999 
					THEN CONVERT(INT, a.FName) + 10000000
					ELSE CONVERT(INT, a.FName) END
				ELSE 0 END,
		a.TimeDate AS DT,
		a.Loc AS LocationCode,
		a.Dev AS ReaderNo,
		a.[Event] AS EventCode,
		'A' AS 'Source'
	FROM tas.External_DSX_evnlog a WITH (NOLOCK)
	WHERE ISNULL(a.FName,'') <> ''
	ORDER BY a.TimeDate DESC

	UNION ALL  
	*/

	--Get swipe data from the Car Park #5
	SELECT	--TOP 5000
			a.EmpNo, 
			a.SwipeDateTime AS DT,
			a.LocationCode,
			a.ReaderNo,
			a.EventCode,
			a.[SOURCE]
	FROM tas.Vw_CarParkSwipeData a WITH (NOLOCK)
	WHERE a.EmpNo > 0
		AND MONTH(a.SwipeDate) BETWEEN MONTH(GETDATE()) - 3 AND MONTH(GETDATE())	--Rev. #1.7
	--ORDER BY a.SwipeDateTime DESC 

	UNION 
    
	--Get swipe data from the new readers that use "UNIS_TENTER" database (Rev. #1.3)
	SELECT	--TOP 5000
			a.EmpNo, 
			a.SwipeDateTime AS DT,
			a.LocationCode,
			a.ReaderNo,
			a.EventCode,
			a.[SOURCE]
	FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
	WHERE a.EmpNo > 0
		AND MONTH(a.SwipeDate) BETWEEN MONTH(GETDATE()) - 3 AND MONTH(GETDATE())	--Rev. #1.7
	--ORDER BY a.SwipeDateTime DESC

GO

/*	Debug:

	SELECT * FROM tas.Tran_SwipeData_dsx1 a
	ORDER BY DT

	SELECT * FROM tas.Tran_SwipeData_dsx1 a
	WHERE a.DT BETWEEN '02/21/2022' AND '02/22/2022'

	SELECT * FROM tas.Tran_SwipeData_dsx4 WITH (NOLOCK)  
	ORDER BY EmpNo DESC, DT DESC, Direction DESC 

*/
