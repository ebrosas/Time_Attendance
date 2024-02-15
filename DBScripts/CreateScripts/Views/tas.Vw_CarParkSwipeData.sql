/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_CarParkSwipeData
*	Description: This view fetches the swipe data from the Car Park #5 readers
*
*	Date:			Author:		Rev.#:		Comments:
*	08/08/2020		Ervin		1.0			Created
*	21/03/2021		Ervin		1.1			Added filter to return data from reader nos. 11 and 12 only
*	25/11/2021		Ervin		1.2			Fixed bug when the value of the field "C_Unique" is not numeric
*	15/01/2022		Ervin		1.3			Added filter by Location Code "1"
*
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_CarParkSwipeData
AS

	--Get the swipe date from the Car Park #5
	SELECT 
		CASE WHEN ISNUMERIC(a.C_Unique) = 1 THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,	--Rev. #1.2
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
			)  
		AS SwipeDateTime,
		1 AS LocationCode,
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
	WHERE a.L_TID IN	--Rev. #1.1
		(
			11,		--Car Park #5 Turnstile (IN)                  
			12		--Car Park #5 Turnstile (OUT)                  
		)
		AND b.LocationCode = 1	--Main Mill
GO

/*	Debug:

	SELECT * FROM tas.Vw_CarParkSwipeData a
	WHERE SwipeDate = '03/31/2021'

*/


