/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_NewReaderSwipeData
*	Description: This view fetches the swipe data from the Main Mill and Foil Mill Gate turnstile and barrier readers
*
*	Date:			Author:		Rev.#:		Comments:
*	11/11/2020		Ervin		1.0			Created
*	31/03/2021		Ervin		1.1			Added filter to return only swipe data from Main Gate and Foil Mill Gate readers
*	25/11/2021		Ervin		1.2			Fixed bug when the value of the field "C_Unique" is not numeric
*	08/08/2023		Ervin		1.3			Modified the logic for fetching the employee no.		
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_NewReaderSwipeData
AS

	SELECT 
		CASE WHEN ISNUMERIC(a.C_Unique) = 1 
			THEN 
				CASE WHEN LEN(a.C_Unique) = 5 AND NOT CAST(a.C_Unique AS INT) BETWEEN 10001 AND 10099 THEN CAST(a.C_Unique AS INT) ELSE CAST(a.C_Unique AS INT) + 10000000 END	--Rev. #1.3
			ELSE 0 
		END AS EmpNo,	
		RTRIM(a.C_Name) AS EmpName,
		CAST(a.C_date AS DATETIME) AS SwipeDate,
		CONVERT
		(
			TIME, 
			CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
			CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
			CONVERT(VARCHAR(2), a.C_Time % 100)          
		)  AS SwipeTime,
		CAST(a.C_date AS DATETIME) +
			CONVERT
			(
				TIME, 
				CONVERT(VARCHAR(2), a.C_Time / 10000) + ':' +
				CONVERT(VARCHAR(2), (a.C_Time % 10000) / 100) + ':' +
				CONVERT(VARCHAR(2), a.C_Time % 100)          
			)  AS SwipeDateTime,
		b.LocationCode,
		a.L_TID AS ReaderNo,
		CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
		'A' AS Source,
		CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'
			WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
			WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE b.SourceID = 1				--(Notes: 1 means setup us new reader that uses "UNIS_TENTER" database)
		AND b.LocationCode IN (1, 2)	--(Notes: 1 = Main Gate location; 2 = Foil Mill location)	--Rev. #1.1
GO

/*	Debug:

	SELECT * FROM tas.Vw_NewReaderSwipeData a
	WHERE a.EmpNo = 11110
	ORDER BY a.SwipeDate DESC

	SELECT * FROM tas.unis_tenter a WITH (NOLOCK)
	WHERE CAST(a.C_date AS DATETIME) = '03/31/2021'

*/


