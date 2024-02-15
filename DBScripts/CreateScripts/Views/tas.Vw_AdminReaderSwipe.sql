/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_AdminReaderSwipe
*	Description: This view fetches swipe raw data from new readers installed in Admin buildings
*
*	Date:			Author:		Rev.#:		Comments:
*	04/04/2022		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_AdminReaderSwipe
AS
	
	SELECT 
		CASE WHEN ISNULL(a.C_Unique, '') <> '' THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,
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
		8 AS LocationCode,
		a.L_TID AS ReaderNo,
		CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
		'A' AS Source,
		CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'	--Rev. #1.3
			WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
			WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE (a.L_TID BETWEEN 41 AND 70)

GO

/*	Debug:

	SELECT * FROM tas.Vw_AdminReaderSwipe a
	WHERE a.EmpNo = 10003631
		AND a.SwipeDate BETWEEN '04/01/2022' AND '04/04/2022'
	ORDER BY a.SwipeDate DESC

*/
