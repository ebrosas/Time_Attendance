USE [tas2]
GO

/****** Object:  View [tas].[Vw_WorkplaceReaderSwipe]    Script Date: 31/03/2021 08:27:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_WorkplaceReaderSwipe
*	Description: This view fetches swipe data from new readers used in the workplace
*
*	Date:			Author:		Rev.#:		Comments:
*	24/09/2020		Ervin		1.0			Created
*	29/11/2020		Ervin		1.1			Added the following readers: Annealing 123, Annealing 456, and Roll Grinder 1
*	10/12/2020		Ervin		1.2			Removed reader #13 in the where filter clause
**********************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_WorkplaceReaderSwipe]
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
		CASE WHEN a.L_TID IN (3, 11) THEN 'IN'
			WHEN a.L_TID IN (2, 12) THEN 'OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter a WITH (NOLOCK)
	WHERE (a.L_TID BETWEEN 41 AND 70)

GO


