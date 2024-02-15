/**************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetMainGateSwipe
*	Description: Retrieve the the swipe time in/out based on specific date
*
*	Date			Author		Revision No.	Comments:
*	30/01/2011		Zaharan		1.0				Created
*	02/07/2020		Ervin		1.1				Refactored the code 
*	06/04/2022		Ervin		1.2				Fetch the swipe data from either Main Gate or Workplace readers based on whether workplace reader is activated
**************************************************************************************************************************************************************************************************/

ALTER VIEW tas.vuEmployeeAttendance 
AS 

	--SELECT a.EmpNo, a.TimeDate AS DT, a.LocationCode, a.ReaderNo, a.EventCode, 'A' AS 'Source' 
	--FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)

	--Start of Rev. #1.2
	SELECT a.EmpNo, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, a.Source AS 'Source', a.SwipeType
	FROM
    (
		SELECT	DISTINCT 
				a.EmpNo,
				CASE WHEN ISNULL(b.CostCenter, '') = '' THEN c.SwipeTime ELSE d.SwipeTime END AS DT,
				CASE WHEN ISNULL(b.CostCenter, '') = '' THEN c.LocationCode ELSE d.LocationCode END AS LocationCode,
				CASE WHEN ISNULL(b.CostCenter, '') = '' THEN c.ReaderNo ELSE d.ReaderNo END AS ReaderNo,
				CASE WHEN ISNULL(b.CostCenter, '') = '' THEN c.EventCode ELSE d.EventCode END AS EventCode,
				'A' AS 'Source',
				d.SwipeType  
		FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			OUTER APPLY	
			( 
				SELECT DISTINCT CostCenter 
				FROM tas.WorkplaceReaderSetting WITH (NOLOCK) 
				WHERE IsActive = 1	
					AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
			) b
			OUTER APPLY
			(
				SELECT SwipeTime, LocationCode, ReaderNo, EventCode
				FROM tas.Vw_MainGateSwipeRawData WITH (NOLOCK)
				WHERE EmpNo = a.EmpNo 
			) c
			OUTER APPLY
			(
				SELECT x.SwipeDateTime AS SwipeTime, x.LocationCode, x.ReaderNo, x.EventCode,
					 CASE WHEN x.SwipeDateTime = y.InTime THEN 'IN'
						WHEN x.SwipeDateTime = z.OutTime THEN 'OUT'
						ELSE '' 
					 END AS SwipeType
				FROM tas.Vw_WorkplaceReaderSwipe x WITH (NOLOCK)
					CROSS APPLY 
					(
						SELECT TOP 1 SwipeDateTime AS InTime 
						FROM tas.Vw_WorkplaceReaderSwipe WITH (NOLOCK)
						WHERE EmpNo = x.EmpNo
							AND SwipeDate = x.SwipeDate
						ORDER BY SwipeDateTime ASC
					) y
					CROSS APPLY 
					(
						SELECT TOP 1 SwipeDateTime AS OutTime 
						FROM tas.Vw_WorkplaceReaderSwipe WITH (NOLOCK)
						WHERE EmpNo = x.EmpNo
							AND SwipeDate = x.SwipeDate
						ORDER BY SwipeDateTime DESC
					) z
				WHERE x.EmpNo = a.EmpNo 
			) d
		WHERE ISNUMERIC(a.EmpNo) = 1
	) a
	WHERE a.DT IS NOT NULL 
	--End of Rev. #1.2

	UNION

	SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING(timeIN,3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING(timeIN,1,2)), dtIN)) AS 'DT', 
		-1 AS 'LocationCode', -1 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source', '' AS SwipeType
	FROM tas.Tran_ManualAttendance WITH (NOLOCK)

	UNION

	SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING([timeOUT],3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING([timeOUT],1,2)), dtOut)) AS 'DT', 
		-1 AS 'LocationCode', -2 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source', '' AS SwipeType
	FROM tas.Tran_ManualAttendance WITH (NOLOCK)

GO

/*	Debug:

	SELECT * FROM tas.vuEmployeeAttendance a
	WHERE a.EmpNo IN (10003631, 10003632)
		AND CONVERT(VARCHAR, a.DT, 101) = '04/05/2022'
	ORDER BY a.EmpNo

*/