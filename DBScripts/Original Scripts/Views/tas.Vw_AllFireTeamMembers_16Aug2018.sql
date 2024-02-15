USE [tas2]
GO

/****** Object:  View [tas].[Vw_AllFireTeamMembers]    Script Date: 16/08/2018 15:29:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_AllFireTeamMembers
*	Description: Get the list of all Fire Team nenbers
*
*	Date:			Author:		Rev. #:		Comments:
*	06/02/2018		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_AllFireTeamMembers]
AS		

	SELECT	CASE WHEN ISNUMERIC(a.FName) = 1 
					THEN 
						CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
						THEN 
							CONVERT(INT, a.FName)
						ELSE 
							CONVERT(INT, a.FName) + 10000000 
						END
					ELSE 0 
					END AS EmpNo,
			CONVERT(DATETIME, CONVERT(VARCHAR, c.TimeDate, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, c.TimeDate, 126)) AS SwipeTime,
			RTRIM(d.LocationName)  + ' - ' + RTRIM(d.ReaderName) AS SwipeLocation,
			(
				CASE	WHEN UPPER(RTRIM(d.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(d.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType,
			LTRIM(RTRIM(CONVERT(VARCHAR(500), a.Notes))) AS Notes
	FROM tas.sy_NAMES a 
		INNER JOIN tas.sy_UDF b ON a.ID = b.NameID	
		LEFT JOIN tas.sy_EvnLog c ON LTRIM(RTRIM(a.FName)) = LTRIM(RTRIM(c.FName)) 
			AND 
			(
				(c.Loc = 1 AND c.Dev IN (4, 6, 5, 7))
				OR (c.Loc = 2 AND c.Dev IN (0, 1, 2, 3)) 
			) 
			AND c.[Event] = 8
		LEFT JOIN tas.Master_AccessReaders d ON c.Loc = d.LocationCode AND c.Dev = d.ReaderNo
	WHERE b.UdfNum = 11 
		AND UPPER(LTRIM(RTRIM(b.UdfText))) = 'Y' 

GO


