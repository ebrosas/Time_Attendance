USE [tas2]
GO

/****** Object:  View [tas].[Vw_EmployeeSwipeData_Workplace]    Script Date: 13/09/2022 14:35:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmployeeSwipeData_Workplace
*	Description: This view retrieves employee swipe information filtered by workplace cost centers
*
*	Date:			Author:		Rev.#:		Comments:
*	08/07/2015		Ervin		1.0			Created
*	12/08/2015		Ervin		1.1			Revised the filter conditions
*	31/08/2015		Ervin		1.2			Fetch the workplace cost centers from "WorkplaceReaderSetting" table
*	29/12/2015		Ervin		1.3			Added 3 hours in the workplace swipe in/out time due to the delay in the reader device's clock
*	29/01/2016		Ervin		1.4			Added filter condition that checks if an employee swipes in the correct reader at the workplace station
*	08/02/2016		Ervin		1.5			Check for excluded employees in the "WorkplaceSwipeExclusion" table
*	25/04/2016		Ervin		1.6			Added condition to fetch the working cost center if it is defined in the "Master_EmployeeAdditional" table
*	15/11/2016		Ervin		1.7			Added condition to exclude swipes at reader nos. 8 and 9 which are used as test readers
*	21/12/2016		Ervin		1.8			Refactored the code in fetching the employee's cost center
*	11/11/2018		Ervin		1.9			Added entry for Sayed Israk (Contractor ID #59960). Refactored code to enhance data retrieval performance
*	19/03/2020		Ervin		2.0			Modified the logic in fetching the employee number espcially for those numbers between 10010000 to 10019999
*	24/09/2020		Ervin		2.1			Implemented the new workplace reader from "unis_tenter" database
*	29/11/2020		Ervin		2.2			Added "UsedForTS" filter to return only readers that are used in Timesheet Processing
*	16/01/2022		Ervin		2.3			Added join to the views that fetch the main gate swipe from the new readers in UNIS
*	22/02/2022		Ervin		2.4			Commented all links to the old Access system database
*	13/04/2022		Ervin		2.5			Added a union query to "Vw_AdminBldgReaderSwipe" to fetch the Admin Bldg. reader swipe data
****************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_EmployeeSwipeData_Workplace]
AS

	SELECT * FROM
	(
		SELECT 
			a.SwipeDate, 
			a.EmpNo, 
			CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 
				THEN 
					CASE WHEN ISNULL(c.ContractorEmpName, '') <> '' THEN RTRIM(c.ContractorEmpName)
					ELSE RTRIM(a.LName) END
				ELSE RTRIM(b.YAALPH) 
				END AS EmpName, 
			CASE WHEN ISNUMERIC(b.YAPGRD) = 1 
				THEN tas.lpad(CAST(b.YAPGRD AS INT), 2 , '0')  
				ELSE 0 
				END AS GradeCode,
			CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END AS EmpStatus,

			CASE WHEN a.EmpNo IN (56186, 56836, 59960) THEN '7600' 
				WHEN ((a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000) THEN RTRIM(d.Effective_BusinessUnit)			
				ELSE 
					CASE WHEN ISNULL(g.WorkingBusinessUnit, '') <> '' THEN RTRIM(g.WorkingBusinessUnit) 
					ELSE 
						CASE WHEN LTRIM(RTRIM(h.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))	--Rev. #1.8
							WHEN LTRIM(RTRIM(h.ABAT1)) = 'UG' THEN LTRIM(RTRIM(h.ABMCU)) 
						END
					END	
			END AS BusinessUnit, 
			
			a.SwipeTime, 
			a.SwipeLocation, 
			a.LocationName,
			a.ReaderName,
			a.SwipeType, 
			a.LocationCode, 
			a.ReaderNo, 
			a.SwipeCode,
			CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor,
			b.YAJBCD,
			d.Effective_ShiftPatCode AS ShiftPatCode,
			d.Effective_ShiftPointer AS ShiftPointer,
			d.Effective_ShiftCode AS ShiftCode,
			CONVERT(VARCHAR(8), e.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), e.DepartFrom, 108) AS ShiftTiming,
			e.ArrivalFrom,
			e.ArrivalTo,
			e.DepartFrom,
			e.DepartTo,
			e.RArrivalFrom,
			e.RArrivalTo,
			e.RDepartFrom,
			e.RDepartTo,

			CASE WHEN EXISTS(SELECT HOHDT FROM tas.syJDE_F55HOLID WITH (NOLOCK) WHERE a.SwipeDate = tas.ConvertFromJulian(HOHDT)) AND LTRIM(RTRIM(b.YAEEOM)) = 'M'
			THEN 
				CASE WHEN e.RArrivalTo > e.RDepartFrom
				THEN (DATEDIFF(n, e.RDepartFrom, e.RArrivalTo)) / 2
				ELSE DATEDIFF(n, e.RArrivalTo, e.RDepartFrom) 
				END
			ELSE
				CASE WHEN e.ArrivalTo > e.DepartFrom
				THEN (DATEDIFF(n, e.DepartFrom, e.ArrivalTo)) / 2
				ELSE DATEDIFF(n, e.ArrivalTo, e.DepartFrom) 
				END
			END AS DurationRequired,
			CASE WHEN RTRIM(d.Effective_ShiftCode) = 'O' THEN 'Day-off' ELSE '' END AS Remarks,
			f.IsDayShift
		FROM
		(
			--Start of Rev. #2.3
			--Get Main Gate swipes from UNIS system
			SELECT	a.EmpNo,
					a.SwipeDate,
					a.SwipeDateTime AS SwipeTime,
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
					RTRIM(a.EmpName) AS LName,
					b.SourceID
			FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #2.2
				CROSS APPLY
				(
					SELECT IsWorkplaceEnabled FROM tas.fnCheckWorkplaceEnabled(a.EmpNo) 
				) c
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)       
				AND c.IsWorkplaceEnabled = 1	--Rev. #2.5

			UNION 

			--Get Main Gate swipes from UNIS system (Var Park #5)
			SELECT	a.EmpNo,
					a.SwipeDate,
					a.SwipeDateTime AS SwipeTime,
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
					RTRIM(a.EmpName) AS LName,
					b.SourceID
			FROM tas.Vw_CarParkSwipeData a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #2.2
				CROSS APPLY
				(
					SELECT IsWorkplaceEnabled FROM tas.fnCheckWorkplaceEnabled(a.EmpNo) 
				) c
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)     
				AND c.IsWorkplaceEnabled = 1	--Rev. #2.5

			--End of Rev. #2.3

			UNION

			--Get Workplace swipes (Note: New reader where data is fetch from "unis_tenter" database)	Rev. #2.1
			SELECT	a.EmpNo,
					a.SwipeDate,
					a.SwipeDateTime AS SwipeTime,
					RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType,
					b.LocationCode,
					b.ReaderNo,
					'WORKPLACE' AS SwipeCode,
					b.LocationName,
					b.ReaderName,
					RTRIM(a.EmpName) AS LName,
					b.SourceID
			FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	--Rev. #2.2
				INNER JOIN tas.Master_EmployeeAdditional c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
				INNER JOIN tas.Master_ShiftPatternTitles d WITH (NOLOCK) ON RTRIM(c.ShiftPatCode) = RTRIM(d.ShiftPatCode) 	
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)
				AND ISNULL(d.IsDayShift, 0) = 0	

			UNION
            
			--Start of Rev. #2.5
			--Get Admin Bldg. reader swipes 
			SELECT	a.EmpNo,
					a.SwipeDate,
					a.SwipeDateTime AS SwipeTime,
					RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType,
					b.LocationCode,
					b.ReaderNo,
					'WORKPLACE' AS SwipeCode,
					b.LocationName,
					b.ReaderName,
					RTRIM(a.EmpName) AS LName,
					b.SourceID
			FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo AND UPPER(RTRIM(b.UsedForTS)) = 'Y'	AND b.SourceID = 2
				CROSS APPLY
				(
					SELECT IsWorkplaceEnabled FROM tas.fnCheckWorkplaceEnabled(a.EmpNo) 
				) c
			WHERE a.EventCode = 8	--(Note: 8 means successful swipe)
				AND c.IsWorkplaceEnabled = 1
			--End of Rev. #2.5
		) AS a
		LEFT JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = b.YAAN8
		LEFT JOIN tas.Master_ContractEmployee c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND ((a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000)
		LEFT JOIN tas.Tran_ShiftPatternUpdates d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.SwipeDate = d.DateX
		LEFT JOIN tas.Master_ShiftTimes e WITH (NOLOCK) ON RTRIM(d.Effective_ShiftPatCode) = RTRIM(e.ShiftPatCode) AND RTRIM(d.Effective_ShiftCode) = RTRIM(e.ShiftCode)
		LEFT JOIN tas.Master_ShiftPatternTitles f WITH (NOLOCK) ON RTRIM(d.Effective_ShiftPatCode) = RTRIM(f.ShiftPatCode) 	
		LEFT JOIN tas.Master_EmployeeAdditional g WITH (NOLOCK) ON a.EmpNo = g.EmpNo	--Rev. #1.6
		LEFT JOIN tas.syJDE_F0101 h WITH (NOLOCK) ON a.EmpNo = CAST(h.ABAN8 AS INT)
		WHERE 
			--(Note: Exclude the contractors
			a.EmpNo > 10000000

			--(Note: Filter records where cost center exists in the list of workplace cost centers)
			AND							
			(
				CASE WHEN a.EmpNo IN (56186, 56836, 59960) THEN '7600' 
					WHEN ((a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000) THEN RTRIM(d.Effective_BusinessUnit)			
					ELSE 
						CASE WHEN ISNULL(g.WorkingBusinessUnit, '') <> '' THEN RTRIM(g.WorkingBusinessUnit) 
						ELSE 
							CASE WHEN LTRIM(RTRIM(h.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))	--Rev. #1.8
								WHEN LTRIM(RTRIM(h.ABAT1)) = 'UG' THEN LTRIM(RTRIM(h.ABMCU)) 
							END
						END	
				END 
			) IN 
			(
				SELECT DISTINCT CostCenter 
				FROM tas.WorkplaceReaderSetting WITH (NOLOCK) 
				WHERE IsActive = 1
			)
	) A
	WHERE 
		(
			(
				A.ReaderNo IN
				(
					SELECT ReaderNo 
					FROM tas.WorkplaceReaderSetting WITH (NOLOCK) 
					WHERE RTRIM(CostCenter) = RTRIM(A.BusinessUnit)

					UNION
            
					SELECT ReaderNo 
					FROM tas.fnGetAlternateReaderNos(RTRIM(A.BusinessUnit))
				)
				AND
				RTRIM(A.SwipeCode) = 'WORKPLACE'
			)
			OR RTRIM(A.SwipeCode) = 'MAINGATE'
		)

GO


