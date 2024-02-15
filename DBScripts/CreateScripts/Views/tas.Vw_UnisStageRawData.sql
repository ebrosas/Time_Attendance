/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_UnisStageRawData
*	Description: This view returns the swipe data from the UNIS staging table
*
*	Date:			Author:		Rev.#:		Comments:
*	12/07/2022		Ervin		1.0			Created
*
**********************************************************************************************************************************************************************************************/

CREATE VIEW tas.Vw_UnisStageRawData
AS

	SELECT * FROM
    (
		SELECT 
			CASE WHEN ISNUMERIC(a.C_Unique) = 1 
				THEN CASE WHEN LEN(RTRIM(a.C_Unique)) = 5 THEN CAST(a.C_Unique AS INT) ELSE CAST(a.C_Unique AS INT) + 10000000 END 
				ELSE 0 
			END AS EmpNo,	--Rev. #1.2
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
			a.L_UID AS UserID,
			b.SourceID		
		FROM [unis].[dbo].[tEnter_stg] a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo --AND b.SourceID = 1		
		WHERE b.LocationCode IN (1, 2)	
	) x

GO 

/*	Debug:

	SELECT TOP 10 * FROM tas.Vw_UnisStageRawData a

*/