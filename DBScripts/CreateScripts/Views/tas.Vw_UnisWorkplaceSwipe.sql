/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_UnisWorkplaceSwipe
*	Description: This view fetches workplace swipe data from Unis system
*
*	Date:			Author:		Rev.#:		Comments:
*	15/01/2022		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_UnisWorkplaceSwipe
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
	WHERE ISNULL(b.SourceID, 0) = 0
		AND b.LocationCode = 8
		AND a.L_TID BETWEEN 41 AND 70

GO 