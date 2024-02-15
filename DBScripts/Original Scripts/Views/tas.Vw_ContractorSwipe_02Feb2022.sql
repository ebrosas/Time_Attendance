USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorSwipe]    Script Date: 02/02/2022 10:57:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*****************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ContractorSwipe
*	Description: Get the Contractors swipe data starting from the date the new ID badge has been implemented
*
*	Date:			Author:		Rev. #:		Comments:
*	14/10/2016		Ervin		1.0			Created
*	17/10/2016		Ervin		1.1			Added filter condition for the LocationCode and ReaderNo		
*	26/08/2018		Ervin		1.2			Commented the filter condition "a.[Event] = 8"
*	26/09/2019		Ervin		1.3			Added "Event" field
*	16/06/2021		Ervin		1.4			Return the top 10000 records to enhance data retrieval performance
*	24/11/2021		Ervin		1.5			Refactored the logic in fethcing the contractor's swipe data from the old Access system and Unis
*	25/11/2021		Ervin		1.6			Added join query to fetch contractor's swipe registered through the Contractor Management System
******************************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_ContractorSwipe]
AS

	/* Old logic commented
	SELECT 
		CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
		END AS EmpNo,
		CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
		CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,
		RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
		(
			CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
					WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
					ELSE '' END
		) AS SwipeType,
		b.LocationCode,
		b.ReaderNo,
		'MAINGATE' AS SwipeCode,
		b.LocationName,
		b.ReaderName,
		a.LName,
		a.[Event]
	FROM tas.sy_EvnLog a WITH (NOLOCK)
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE 
		--a.[Event] = 8	--(Note: 8 means successful swipe)
		--AND 
		(
			(b.LocationCode = 1 AND b.ReaderNo IN (4, 5, 6, 7))
			OR
            (b.LocationCode = 2 AND b.ReaderNo IN (0, 1, 2, 3))
		)
		AND 
		(
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN 
						CONVERT(INT, a.FName)
					ELSE 
						CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END
		) < 10000000
		AND 
		(
			CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
					THEN 
						CONVERT(INT, a.FName)
					ELSE 
						CONVERT(INT, a.FName) + 10000000 
					END
				ELSE 0 
			END
		) > 50000
		AND CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) >= '10/10/2016'	--Refers to the date the new Contractor ID badge was implemented
	*/

	--Get the Contractor's swipe from the old Access system
	
	SELECT	a.EmpNo,			
			a.SwipeDate,
			a.SwipeTime,
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			CASE WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
				WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
				ELSE '' 
			END AS SwipeType,
			b.LocationCode,
			a.ReaderNo,
			'MAINGATE' AS SwipeCode,
			b.LocationName,
			b.ReaderName,
			a.LName,
			a.[Event]
	FROM
    (
		SELECT	CASE WHEN ISNUMERIC(FName) = 1 THEN CAST(FName AS INT) ELSE 0 END AS EmpNo, 
				CONVERT(DATETIME, CONVERT(VARCHAR, TimeDate, 12)) AS SwipeDate,
				CONVERT(DATETIME, CONVERT(VARCHAR, TimeDate, 126)) AS SwipeTime,
				Dev AS ReaderNo,
				LName,
				[Event]
		FROM tas.sy_EvnLog WITH (NOLOCK)
		WHERE CASE WHEN ISNUMERIC(FName) = 1 THEN CAST(FName AS INT) ELSE 0 END BETWEEN 50000 AND 69999
			AND CONVERT(DATETIME, CONVERT(VARCHAR, TimeDate, 12)) >= '10/10/2016'	--Refers to the date the new Contractor ID badge was implemented
	) a
	INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode IN (1, 2) 

	UNION
    
	--Get the Contractor's swipe from the new Unis system
	SELECT	a.EmpNo,
			b.SwipeDate,
			b.SwipeTime,
			RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
			CASE WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
				WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
				ELSE '' 
			END AS SwipeType,
			c.LocationCode,
			b.ReaderNo,
			'MAINGATE' AS SwipeCode,
			c.LocationName,
			c.ReaderName,
			b.LName,
			b.EventCode AS [Event]
	FROM 
	(
		SELECT CASE WHEN ISNUMERIC(EmpNo) = 1 THEN CAST(EmpNo AS INT) ELSE 0 END AS EmpNo, 
			RTRIM(FName) + ' - ' + RTRIM(LName) AS EmpName 
		FROM tas.sy_PrintedCards WITH (NOLOCK)
		WHERE ISNUMERIC(EmpNo) = 1 
			AND 
			(
				CASE WHEN ISNUMERIC(EmpNo) = 1 THEN CAST(EmpNo AS INT) ELSE 0 END BETWEEN 50000 AND 69999
			)
	) a
	CROSS APPLY
	(
		SELECT	CAST(C_Unique AS INT) AS EmpNo,
				CAST(C_date AS DATETIME) AS SwipeDate,
				CAST(C_date AS DATETIME) +
					CONVERT
					(
						TIME, 
						CONVERT(VARCHAR(2), C_Time / 10000) + ':' +
						CONVERT(VARCHAR(2), (C_Time % 10000) / 100) + ':' +
						CONVERT(VARCHAR(2), C_Time % 100)          
					)  
				AS SwipeTime,
				L_TID AS ReaderNo,
				RTRIM(C_Name) AS LName,
				CASE WHEN L_Result = 0 THEN 8 ELSE L_Result END AS EventCode
		FROM tas.unis_tenter WITH (NOLOCK)
		WHERE CAST(C_Unique AS INT) = a.EmpNo
	) b 
	INNER JOIN tas.Master_AccessReaders c WITH (NOLOCK) ON b.ReaderNo = c.ReaderNo AND c.LocationCode IN (1, 2) AND c.SourceID = 1 
	WHERE b.ReaderNo IN
		(
			13,		--Main Gate Turnstile (In)
			14,		--Main Gate Turnstile (Out)
			15,		--Main Gate Barrier (In)
			16,		--Main Gate Barrier (Out)
			31,		--Foil Mill Turstile (In)
			32,		--Foil Mill Turstile (Out)
			33,		--Foil Mill Barrier (In)
			34		--Foil Mill Barrier (Out)
		)

	UNION
    
	--Start of Rev. #1.6
	--Get Contractors swipe registered through the Contractor Management System
	SELECT 
		CAST(b.C_Unique AS INT) AS EmpNo,
		CAST(b.C_date AS DATETIME) AS SwipeDate,
		CAST(b.C_date AS DATETIME) +
			CONVERT
			(
				TIME, 
				CONVERT(VARCHAR(2), b.C_Time / 10000) + ':' +
				CONVERT(VARCHAR(2), (b.C_Time % 10000) / 100) + ':' +
				CONVERT(VARCHAR(2), b.C_Time % 100)          
			)  
		AS SwipeTime,
		RTRIM(c.LocationName) + ' ' + RTRIM(c.ReaderName) AS SwipeLocation,
		CASE WHEN RTRIM(c.Direction) = 'I' THEN 'IN'
			WHEN RTRIM(c.Direction) = 'O' THEN 'OUT'
			WHEN RTRIM(c.Direction) = 'IO' THEN 'IN/OUT'
			ELSE ''
		END AS SwipeType,
		c.LocationCode,
		b.L_TID AS ReaderNo,
		UPPER(c.LocationName) AS SwipeCode,
		c.LocationName,
		c.ReaderName,
		RTRIM(b.C_Name) AS LName,
		CASE WHEN b.L_Result = 0 THEN 8 ELSE b.L_Result END AS [Event]
	FROM tas.ContractorRegistry a WITH (NOLOCK) 
		INNER JOIN tas.unis_tenter b WITH (NOLOCK) ON a.ContractorNo = CAST(b.C_Unique AS INT)
		INNER JOIN tas.Master_AccessReaders c WITH (NOLOCK) ON b.L_TID = c.ReaderNo 
	WHERE c.SourceID = 1				--(Notes: 1 means setup us new reader that uses "UNIS_TENTER" database)
		AND c.LocationCode IN (1, 2)	--(Notes: 1 = Main Gate location; 2 = Foil Mill location)	
	--End of Rev. #1.6
		
GO


