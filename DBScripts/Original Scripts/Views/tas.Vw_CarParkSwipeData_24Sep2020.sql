USE [tas2]
GO

/****** Object:  View [tas].[Vw_CarParkSwipeData]    Script Date: 24/09/2020 11:35:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_CarParkSwipeData
*	Description: This view fetches the swipe data from the Car Park #5 readers
*
*	Date:			Author:		Rev.#:		Comments:
*	08/08/2020		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_CarParkSwipeData]
AS

	--Get the swipe date from the Car Park #5
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
		1 AS LocationCode,
		a.L_TID AS ReaderNo,
		a.L_Result AS EventCode,
		'A' AS Source,
		CASE WHEN a.L_TID = 11 THEN 'IN'
			WHEN a.L_TID = 12 THEN 'OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter a WITH (NOLOCK)
	--WHERE CAST(a.C_Unique AS INT) + 10000000 > 10000000

GO


