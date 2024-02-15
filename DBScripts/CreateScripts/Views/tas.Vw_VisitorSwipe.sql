/*******************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_VisitorSwipe
*	Description: Get the swipe records of specific Visitor
*
*	Date:			Author:		Rev. #:		Comments:
*	08/08/2016		Ervin		1.0			Created
*	24/09/2020		Ervin		1.1			Implemented the new workplace reader from "unis_tenter" database
*	29/11/2020		Ervin		1.2			Added "UsedForTS" filter to return only readers that are used in Timesheet Processing
*	22/12/2022		Ervin		1.3			Commented code that fetch data from the old Access System
*	08/08/2023		Ervin		1.4			Fetch swipe data from "Vw_NewReaderSwipeData"
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_VisitorSwipe
AS		

	--Get Workplace swipes (Note: New reader where data is fetch from "unis_tenter" database)	Rev. #1.1
	SELECT	0 AS SwipeID,
			a.EmpNo,
			a.EmpName,
			a.SwipeDate,
			a.SwipeDateTime AS SwipeTime,
			RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
			NULL AS SwipeTypeCode,
			(
				CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
						WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
						ELSE '' END
			) AS SwipeType,
			b.LocationCode,
			b.LocationName,
			b.ReaderNo,
			b.ReaderName,
			'WORKPLACE' AS SwipeCode,
			NULL AS LogID
	FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)		--Rev. #1.4
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #1.2
	WHERE a.EventCode = 8	--(Note: 8 means successful swipe)

	UNION 

	--Get Manual Swipe recorded at the main gate entrance 
	SELECT	b.SwipeID,
			a.VisitorCardNo AS EmpNo,
			a.VisitorName AS EmpName,
			b.SwipeDate,
			b.SwipeTime,
			'Manual Swipe' AS SwipeLocation,
			b.SwipeTypeCode,
			CASE WHEN RTRIM(b.SwipeTypeCode) = 'valIN' THEN 'IN'
				WHEN RTRIM(b.SwipeTypeCode) = 'valOUT' THEN 'OUT'
				ELSE ''
			END AS SwipeType,
			NULL AS LocationCode,
			'' AS LocationName,
			NULL AS ReaderNo,
			'' AS ReaderName,
			'MANUAL' AS SwipeCode,
			b.LogID
	FROM tas.VisitorPassLog a
		INNER JOIN tas.VisitorSwipeLog b ON a.LogID = b.LogID

GO

/*	Debug:

	SELECT * FROM tas.Vw_VisitorSwipe a
	WHERE a.EmpNo = 11105

	SELECT * FROM tas.Vw_VisitorSwipe a
	WHERE a.SwipeDate = '09/24/2020'
		AND a.ReaderNo = 13
	ORDER BY a.SwipeTime DESC

	SELECT * FROM tas.Vw_VisitorSwipe a
	WHERE a.EmpNo = 10003011
		AND a.SwipeDate BETWEEN '27/01/2016' AND '27/01/2016'
	ORDER BY a.SwipeTime ASC

*/



