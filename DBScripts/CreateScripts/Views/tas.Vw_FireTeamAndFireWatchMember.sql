/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_FireTeamAndFireWatchMember
*	Description: Get the list of all Fire Team nenbers
*
*	Date:			Author:		Rev. #:		Comments:
*	05/02/2018		Ervin		1.0			Created
*	16/08/2018		Ervin		1.1			Added WITH (NOLOCK) clause in all joint table to enhance data retrieval performance
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_FireTeamAndFireWatchMember
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
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) as SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) as SwipeTime,
			RTRIM(d.LocationName)  + ' - ' + RTRIM(d.ReaderName) AS SwipeLocation,
			(
				CASE	WHEN UPPER(RTRIM(d.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(d.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType, 		
			LTRIM(RTRIM(CONVERT(VARCHAR(500), b.Notes))) AS Notes,
			CASE WHEN c.UdfNum = 11 THEN 'Fire Team' ELSE 'Fire Watch' END AS GroupType
	FROM tas.sy_EvnLog a WITH (NOLOCK) 
		INNER JOIN tas.sy_NAMES b WITH (NOLOCK) ON LTRIM(RTRIM(a.FName)) = LTRIM(RTRIM(b.FName)) 
		INNER JOIN tas.sy_UDF c WITH (NOLOCK) ON b.ID = c.NameID
		INNER JOIN tas.Master_AccessReaders d WITH (NOLOCK) ON a.Loc = d.LocationCode AND a.Dev = d.ReaderNo
	WHERE 
		a.[Event] = 8	--(Note: 8 means successful swipe)
		AND c.UdfNum IN (11, 14)
		AND UPPER(LTRIM(RTRIM(c.UdfText))) = 'Y' 
		AND 
		(
			(a.Loc = 1 AND a.Dev IN (4, 6, 5, 7))
			OR (a.Loc = 2 AND a.Dev IN (0, 1, 2, 3)) 
		) 

GO 

/* Testing:

	SELECT * FROM tas.Vw_FireTeamAndFireWatchMember a
	
*/