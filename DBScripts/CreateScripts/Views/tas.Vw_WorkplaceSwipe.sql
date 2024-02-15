	/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_WorkplaceSwipe
*	Description: Get the employee's swipes at the plant readers
*
*	Date:			Author:		Rev. #:		Comments:
*	16/02/2017		Ervin		1.0			Created
*	28/02/2017		Ervin		1.1			Added condition that will show the value of "TimeInWP" and "TimeOutWP" fields only when correction is approved and workflow is closed
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_WorkplaceSwipe
AS
	
	SELECT	a.EmpNo AS EmployeeNo,
			a.SwipeDate,

			a.TimeInMG,
			--CASE WHEN a.CorrectionType IN (1, 3)	--(Note: 1 = Workplace Time In; 3 = Both)
			--	THEN
			--		CASE WHEN a.IsCorrected = 1 AND a.IsClosed = 1 AND RTRIM(ISNULL(a.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN a.TimeInMG ELSE NULL END 
			--	ELSE
			--		a.TimeInMG
			--END AS TimeInMG,
			CASE WHEN a.CorrectionType IN (1, 3)	--(Note: 1 = Workplace Time In; 3 = Both)
				THEN
					CASE WHEN a.IsCorrected = 1 AND a.IsClosed = 1 AND RTRIM(ISNULL(a.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN a.TimeInWP ELSE NULL END 
				ELSE
					a.TimeInWP
			END AS TimeInWP,
			CASE WHEN a.CorrectionType IN (2, 3)	--(Note: 2 = Workplace Time Out; 3 = Both)
				THEN
					CASE WHEN a.IsCorrected = 1 AND a.IsClosed = 1 AND RTRIM(ISNULL(a.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN a.TimeOutWP ELSE NULL END 
				ELSE
					a.TimeOutWP
			END AS TimeOutWP,

			--CASE WHEN a.CorrectionType IN (2, 3)	--(Note: 2 = Workplace Time Out; 3 = Both)
			--	THEN
			--		CASE WHEN a.IsCorrected = 1 AND a.IsClosed = 1 AND RTRIM(ISNULL(a.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN a.TimeOutMG ELSE NULL END 
			--	ELSE
			--		a.TimeOutMG
			--END AS TimeOutMG,
			a.TimeOutMG,
			
			a.CorrectionType,
			a.IsCorrected,
			CASE WHEN a.IsClosed = 1 AND RTRIM(ISNULL(a.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected')
				THEN 1
				ELSE 0
			END AS IsClosed,			
			CASE WHEN a.IsCorrected = 1
				THEN a.Remarks
				ELSE
					CASE WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe in and out at the workplace'
						WHEN ISNULL(a.TimeInWP, '') = '' AND ISNULL(a.TimeOutWP, '') <> ''
						THEN 'Missing swipe in at the workplace'
						WHEN ISNULL(a.TimeInWP, '') <> '' AND ISNULL(a.TimeOutWP, '') = ''
						THEN 'Missing swipe out at the workplace'
						ELSE ''
					END
			END AS Remarks	
	FROM tas.Tran_WorkplaceSwipe a

GO 

/*	Debug:

	SELECT * FROM tas.Vw_WorkplaceSwipe a
	WHERE a.EmployeeNo = 10003730
		AND a.SwipeDate = '02/08/2017'

*/