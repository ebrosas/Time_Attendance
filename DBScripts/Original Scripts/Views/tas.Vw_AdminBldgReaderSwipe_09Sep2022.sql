USE [tas2]
GO

/****** Object:  View [tas].[Vw_AdminBldgReaderSwipe]    Script Date: 09/09/2022 21:38:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_AdminBldgReaderSwipe
*	Description: This view returns the swipe data from the Admin Bldg. readers which uses facial recognition technology
*
*	Date:			Author:		Rev.#:		Comments:
*	08/04/2022		Ervin		1.0			Created
*	14/08/2022		Ervin		1.1			Added UNION clause to fetch the swipe data of the reader devices which were migrated from UNIS system into ALPETA system
**********************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_AdminBldgReaderSwipe]
AS
	
	SELECT 
		CASE WHEN ISNULL(a.C_Unique, '') <> '' THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,
		RTRIM(a.C_Name) COLLATE SQL_Latin1_General_CP1_CI_AS AS EmpName ,
		CAST(a.C_date AS DATETIME) AS SwipeDate,
		CAST(a.C_Time AS TIME) AS SwipeTime,
		CAST(a.C_date AS DATETIME) + CAST(a.C_Time AS TIME) AS SwipeDateTime,
		8 AS LocationCode,
		b.LocationName,
		a.L_TID AS ReaderNo,
		b.ReaderName,
		CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
		'A' AS Source,
		CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'	
			WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
			WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter_alpeta a WITH (NOLOCK) 
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE (a.L_TID BETWEEN 71 AND 100)

	--Start of Rev. #1.1
	UNION
        
	--Get swipe data from several reader devices which were migrated from UNIS system into ALPETA system
	SELECT 
		CASE WHEN ISNULL(a.C_Unique, '') <> '' THEN CAST(a.C_Unique AS INT) + 10000000 ELSE 0 END AS EmpNo,
		RTRIM(a.C_Name) COLLATE SQL_Latin1_General_CP1_CI_AS AS EmpName ,
		CAST(a.C_date AS DATETIME) AS SwipeDate,
		CAST(a.C_Time AS TIME) AS SwipeTime,
		CAST(a.C_date AS DATETIME) + CAST(a.C_Time AS TIME) AS SwipeDateTime,
		8 AS LocationCode,
		b.LocationName,
		a.L_TID AS ReaderNo,
		b.ReaderName,
		CASE WHEN a.L_Result = 0 THEN 8 ELSE a.L_Result END AS EventCode,
		'A' AS Source,
		CASE WHEN RTRIM(b.Direction) = 'I' THEN 'IN'	
			WHEN RTRIM(b.Direction) = 'O' THEN 'OUT'
			WHEN RTRIM(b.Direction) = 'IO' THEN 'IN/OUT'
			ELSE ''
		END AS Direction,
		RTRIM(a.C_Card) AS CardNo,
		a.L_UID AS UserID		
	FROM tas.unis_tenter_alpeta a WITH (NOLOCK) 
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.L_TID = b.ReaderNo 
	WHERE a.L_TID IN 
		(
			46		--Remelt Control Room2
		)
	--End of Rev. #1.1

GO


