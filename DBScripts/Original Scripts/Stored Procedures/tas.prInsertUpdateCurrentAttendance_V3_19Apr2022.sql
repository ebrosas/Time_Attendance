USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[prInsertUpdateCurrentAttendance_V3]    Script Date: 19/04/2022 12:33:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.prInsertUpdateCurrentAttendance_V3
*	Description: Used to populate data in "Master_EmployeeAttendance" temporary table
*
*	Date			Author		Revision No.	Comments:
*	18/04/2022		Ervin		1.0				Created
*
****************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[prInsertUpdateCurrentAttendance_V3]  
(
	@action			VARCHAR(10),
	@processDate	DATETIME = NULL,
	@costCenter		VARCHAR(12) = NULL       	
)
AS                

	DECLARE @FromDAte		DATETIME, 
			@ToDAte			DATETIME,
			@isBackDated	BIT = 0

	IF UPPER(RTRIM(@action)) = 'INSERT'
	BEGIN 

		--Validate parameters
		IF @processDate IS NULL
			SET @processDate = GETDATE()

		IF ISNULL(@costCenter, '') = ''
			SET @costCenter = NULL 

		SELECT @FromDAte = tas.fmtDate_MinTimeOfDay_V2(@processDate)
		SELECT @ToDate = tas.fmtDate_MaxTimeOfDay_V2(@processDate)

		--Check if process date is less than today's date
		IF @processDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
			SET @isBackDated = 1
		
		/****************************************** Start of Rev. #1.1  **************************************************************************/
		--Build the workplace swipe table whose data comes from the plant and Admin Building readers
		--/* Comment code block below to enable single pair of workplace swipes
		SELECT a.EmpNo, a.SwipeDate, a.SwipeDateTime AS DT, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode AS ShiftCode
		INTO #AttendaceTable
		FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DateX
			INNER JOIN tas.Master_Employee_JDE c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
		WHERE a.SwipeDate = @processDate
			AND (RTRIM(c.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.EmpNo, a.SwipeDate, a.SwipeDateTime, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode

		--Start of Rev. #1.2
		UNION
        
		SELECT a.EmpNo, a.SwipeDate, a.SwipeDateTime AS DT, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode AS ShiftCode
		FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DateX
			INNER JOIN tas.Master_Employee_JDE c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
		WHERE a.SwipeDate = @processDate
			AND (RTRIM(c.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.EmpNo, a.SwipeDate, a.SwipeDateTime, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode
		--End of Rev. #1.2

		IF @isBackDated = 1
		BEGIN
        
			SELECT a.EmpNo, a.SwipeDate, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, 'A' AS 'Source', 
				CASE WHEN a.ID%2 = 0 THEN 'OUT' 
					ELSE 
						CASE WHEN a.SwipeCount = 1 AND RTRIM(a.ShiftCode) = 'N' THEN 'OUT' 
						ELSE --'IN' 
							CASE WHEN a.ID = a.SwipeCount AND a.SwipeCount > 1 THEN 'OUT' ELSE 'IN' END 
						END 
				END AS SwipeType,
				a.ShiftCode, a.ID, a.SwipeCount
			INTO #AttendaceTableBackDated
			FROM
			(
				SELECT ROW_NUMBER() OVER (PARTITION BY EmpNo, SwipeDate ORDER BY DT) AS ID, 	
					COUNT(*) OVER (PARTITION BY EmpNo, SwipeDate) AS SwipeCount, 	
					a.* 
				FROM #AttendaceTable a WITH (NOLOCK)
				GROUP BY a.EmpNo, a.SwipeDate, a.DT,  a.LocationCode, a.ReaderNo, a.EventCode, a.ShiftCode
			) a

			SELECT a.EmpNo, a.SwipeDate, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, a.[Source], a.SwipeType, a.ShiftCode, a.ID, a.SwipeCount   
			INTO #WorkplaceTableBackDated
			FROM #AttendaceTableBackDated a

			--SELECT a.EmpNo, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, a.[Source] AS 'Source', CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) AS SwipeDate 
			--INTO #AttendanceTable1
			--FROM tas.Tran_SwipeData_dsx1 a WITH (NOLOCK)
			--WHERE DT BETWEEN @FromDAte AND @ToDate

			INSERT INTO tas.Master_EmployeeAttendance 
			(
				EmployeeNo, 
				AttendanceDate, 
				LocationCode, 
				ReaderNo, 
				EventCode, 
				[Source],
				SwipeType
			)
			SELECT * FROM
			(
				SELECT	DISTINCT 
						a.EmpNo, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= e.EffectiveDate  THEN c.DT ELSE a.DT END AS DT, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= e.EffectiveDate  THEN c.LocationCode ELSE a.LocationCode END AS LocationCode, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= e.EffectiveDate  THEN c.ReaderNo ELSE a.ReaderNo END AS ReaderNo, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= e.EffectiveDate  THEN c.EventCode ELSE a.EventCode END AS EventCode, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= e.EffectiveDate  THEN c.[Source] ELSE a.[Source] END AS 'Source',
						c.SwipeType
				FROM tas.Tran_SwipeData_dsx1 a
					INNER JOIN tas.Master_Employee_JDE d WITH (NOLOCK) ON a.EmpNo = d.EmpNo
					CROSS APPLY 
					(
						SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)
						WHERE RTRIM(CostCenter) = RTRIM(d.BusinessUnit) 
							AND IsActive = 1
					) e
					CROSS APPLY
					(
						SELECT IsWorkplaceEnabled FROM tas.fnCheckWorkplaceEnabled(a.EmpNo)
					) b
					LEFT JOIN #WorkplaceTable c ON a.EmpNo = c.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = c.SwipeDate
				WHERE a.DT BETWEEN @FromDAte AND @ToDate
			) a
			WHERE a.DT IS NOT NULL
			/*************************************************** End of Rev. #1.1 ******************************************************/
			
			UNION	 

			SELECT	EmpNo, 
					DATEADD(MINUTE, CONVERT(INT, SUBSTRING(timeIN, 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING(timeIN, 1, 2)), dtIN)) AS 'DT', 
					-1 AS 'LocationCode', 
					-1 AS 'ReaderNo', 
					'' AS 'EventCode', 
					'' AS 'Source',
					'' AS SwipeType 
			FROM tas.Tran_ManualAttendance
			WHERE CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MINUTE, CONVERT(INT, SUBSTRING(timeIN, 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING(timeIN, 1, 2)), dtIN)), 12)) BETWEEN @FromDAte AND @ToDate
			
			UNION

			SELECT	EmpNo, 
					DATEADD(MINUTE, CONVERT(INT, SUBSTRING([timeOUT], 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING([timeOUT], 1, 2)), dtOut)) AS 'DT', 
					-1 AS 'LocationCode', 
					-2 AS 'ReaderNo', 
					'' AS 'EventCode', 
					'' AS 'Source',
					'' AS SwipeType 
			FROM tas.Tran_ManualAttendance
			WHERE CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MINUTE, CONVERT(INT, SUBSTRING([timeOUT], 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING([timeOUT], 1, 2)), dtOut)), 12)) BETWEEN @FromDAte AND @ToDate
			

			-- Dropping temporary tables			
			DROP TABLE #AttendaceTableBackDated
			DROP TABLE #WorkplaceTableBackDated
		END 

		ELSE 
		BEGIN

			SELECT a.EmpNo, a.SwipeDate, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, 'A' AS 'Source', 
				CASE WHEN a.ID%2 = 0 THEN --'OUT'	--(Notes: Even number will display OUT)
					CASE WHEN b.IsSwipedOut = 0 AND a.ID = a.SwipeCount THEN 'IN' ELSE 'OUT' END 
					ELSE 
						CASE WHEN a.SwipeCount = 1 AND RTRIM(a.ShiftCode) = 'N' THEN 'OUT' 
						ELSE --'IN' 
							CASE WHEN a.ID = a.SwipeCount AND a.SwipeCount > 1 AND b.IsSwipedOut = 1 THEN 'OUT' ELSE 'IN' END 
						END 
				END AS SwipeType,
				a.ShiftCode, a.ID, a.SwipeCount
			INTO #AttendaceTable2
			FROM
			(
				SELECT ROW_NUMBER() OVER (PARTITION BY EmpNo, SwipeDate ORDER BY DT) AS ID, 	
					COUNT(*) OVER (PARTITION BY EmpNo, SwipeDate) AS SwipeCount, 	
					a.* 
				FROM #AttendaceTable a WITH (NOLOCK)
				GROUP BY a.EmpNo, a.SwipeDate, a.DT,  a.LocationCode, a.ReaderNo, a.EventCode, a.ShiftCode
			) a
			OUTER APPLY
            (
				SELECT tas.fnCheckIfSwipedOut(a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, SwipeDate, 12)))  AS IsSwipedOut
			) b

			SELECT a.EmpNo, a.SwipeDate, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, a.[Source], a.SwipeType, a.ShiftCode, a.ID, a.SwipeCount   
			INTO #WorkplaceTable
			FROM #AttendaceTable2 a

			INSERT INTO tas.Master_EmployeeAttendance 
			(
				EmployeeNo, 
				AttendanceDate, 
				LocationCode, 
				ReaderNo, 
				EventCode, 
				[Source],
				SwipeType
			)
			SELECT * FROM
			(
				SELECT	DISTINCT 
						a.EmpNo, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= c.EffectiveDate  THEN c.DT ELSE a.DT END AS DT, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= c.EffectiveDate  THEN c.LocationCode ELSE a.LocationCode END AS LocationCode, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= c.EffectiveDate  THEN c.ReaderNo ELSE a.ReaderNo END AS ReaderNo, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= c.EffectiveDate  THEN c.EventCode ELSE a.EventCode END AS EventCode, 
						CASE WHEN b.IsWorkplaceEnabled = 1 AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) <= c.EffectiveDate  THEN c.[Source] ELSE a.[Source] END AS 'Source',
						c.SwipeType
				FROM tas.Tran_SwipeData_dsx1 a
					CROSS APPLY
					(
						SELECT IsWorkplaceEnabled FROM tas.fnCheckWorkplaceEnabled(a.EmpNo)
					) b
					LEFT JOIN
					(
						SELECT y.EffectiveDate, x.* 
						FROM #WorkplaceTable x WITH (NOLOCK)
							INNER JOIN tas.WorkplaceReaderSetting y WITH (NOLOCK) ON x.ReaderNo = y.ReaderNo
					) c ON a.EmpNo = c.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = c.SwipeDate
				WHERE a.DT BETWEEN @FromDAte AND @ToDate
			) a
			WHERE a.DT IS NOT NULL
			/*************************************************** End of Rev. #1.1 ******************************************************/
			
			UNION	 

			SELECT	EmpNo, 
					DATEADD(MINUTE, CONVERT(INT, SUBSTRING(timeIN, 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING(timeIN, 1, 2)), dtIN)) AS 'DT', 
					-1 AS 'LocationCode', 
					-1 AS 'ReaderNo', 
					'' AS 'EventCode', 
					'' AS 'Source',
					'' AS SwipeType 
			FROM tas.Tran_ManualAttendance
			WHERE CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MINUTE, CONVERT(INT, SUBSTRING(timeIN, 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING(timeIN, 1, 2)), dtIN)), 12)) BETWEEN @FromDAte AND @ToDate
			
			UNION

			SELECT	EmpNo, 
					DATEADD(MINUTE, CONVERT(INT, SUBSTRING([timeOUT], 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING([timeOUT], 1, 2)), dtOut)) AS 'DT', 
					-1 AS 'LocationCode', 
					-2 AS 'ReaderNo', 
					'' AS 'EventCode', 
					'' AS 'Source',
					'' AS SwipeType 
			FROM tas.Tran_ManualAttendance
			WHERE CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MINUTE, CONVERT(INT, SUBSTRING([timeOUT], 3, 2)), DATEADD(hh, CONVERT(INT, SUBSTRING([timeOUT], 1, 2)), dtOut)), 12)) BETWEEN @FromDAte AND @ToDate
			

			-- Dropping temporary tables
			DROP TABLE #AttendaceTable2
			DROP TABLE #WorkplaceTable
        END 

		DROP TABLE #AttendaceTable
	END
	
	ELSE
	BEGIN
    
		DELETE FROM tas.Master_EmployeeAttendance
	END 


/*	Debugging:

	SELECT * FROM tas.Master_EmployeeAttendance a
	WHERE a.EmployeeNo = 10003632
	ORDER BY AttendanceDate

	exec tas.prInsertUpdateCurrentAttendance_V3 'INSERT', '04/18/2022', '7600' 
	exec tas.prInsertUpdateCurrentAttendance_V3 'delete'

*/
	