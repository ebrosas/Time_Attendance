/**************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_PunctualityMonthlyReport
*	Description: This stored procedure is used to fetch the data for the Employee Monthly Punctuality Report
*
*	Date:			Author:		Rev.#:		Comments:
*	14/07/2017		Ervin		1.0			Created
*	26/11/2017		Ervin		1.1			Added filter condition that exclude employees with medical condition
*	15/05/2018		Ervin		1.2			Implemented Ramadan timing logic in the code
*	31/03/2019		Ervin		1.3			Added filter condition that checks if employee is not on duty ROTA
*	30/06/2019		Ervin		1.4			Commented filter condition that checks if employee is on duty ROTA
*	11/07/2019		Ervin		1.5			Modified the filter condition to return only Grade 12 Managers
*	25/06/2020		Ervin		1.6			Modified the filter for managers to include all grade 12 and above
*	09/07/2020		Ervin		1.7			Modified the filter condition to fetch all unpunctual employees regardless of pay grade
*	27/09/2021		Ervin		1.8			Added filter condition to exclude employee with RA correction codes
*	02/02/2022		Ervin		1.9			Added filter to exclude employees with 10005xxx series as per Helpdesk No. 174168 
**************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_PunctualityMonthlyReport
(
	@loadType					TINYINT,
	@startDate					DATETIME,
	@endDate					DATETIME,
	@costCenter					VARCHAR(12) = '',
	@occurenceLimit				INT = 0,
	@lateAttendanceThreshold	INT = 0,
	@earlyLeavingThreshold		INT = 0
)
AS	
BEGIN

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@occurenceLimit, 0) = 0			--(Note: The default occurence limit is 8)
		SET @occurenceLimit = 8		

	IF ISNULL(@lateAttendanceThreshold, 0) = 0	--(Note: Default value is 5 minutes)
		SET @lateAttendanceThreshold = 5	

	IF ISNULL(@earlyLeavingThreshold, 0) = 0	--(Note: Default value is 5 minutes)
		SET @earlyLeavingThreshold = 5	

	IF @loadType = 0			--Get the list of cost centers
	BEGIN

		SELECT	A.CostCenter,
				A.CostCenterName,
				B.CostCenterManager AS ManagerEmpNo,
				RTRIM(E.EmpName) AS ManagerEmpName,
				LTRIM(RTRIM(ISNULL(C.EAEMAL, ''))) AS ManagerEmail,
				B.Superintendent AS SuperintendentEmpNo,
				RTRIM(F.EmpName) AS SuperintendentEmpName,
				LTRIM(RTRIM(ISNULL(D.EAEMAL, ''))) AS SuperintendentEmail
		FROM
        (
			SELECT DISTINCT 
				RTRIM(c.BusinessUnit) AS CostCenter, 
				RTRIM(d.BUname) AS CostCenterName
			FROM tas.Tran_Timesheet a
				OUTER APPLY
				(
					SELECT TOP 1 dtIN, dtOUT 
					FROM tas.Tran_Timesheet 
					WHERE DT = a.DT
						AND dtIN IS NOT NULL 
						AND dtOUT IS NOT NULL 
						AND EmpNo = a.EmpNo
				) b
				INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE_view d ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
				LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
				LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
				LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
				LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
					AND 
					(
						CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
							THEN a.ShiftCode
							ELSE 
								CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
									THEN RTRIM(a.Actual_ShiftCode)
									ELSE RTRIM(a.ShiftCode)
								END
						END
					) = RTRIM(h.ShiftCode)
			WHERE 
				a.IsLastRow = 1
				--AND ISNULL(a.IsOnDutyRota, 0) = 0	--Rev. #1.3
				AND ISNULL(a.IsDriver, 0) = 0
				AND ISNULL(a.IsLiasonOfficer, 0) = 0
				AND ISNULL(a.IsHedger, 0) = 0
				AND a.DT BETWEEN @startDate AND @endDate
				AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND a.EmpNo IN (SELECT EmpNo FROM tas.fnGetNotPuctualEmployeesAll(@startDate, @endDate, @costCenter, @occurenceLimit, @lateAttendanceThreshold, @earlyLeavingThreshold))
				--AND c.GradeCode <= 11	
				AND tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo)  = 0	--Rev. #1.1
				AND 
				(
					CASE 
						WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
							THEN a.ShiftCode
							ELSE 
								CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
									THEN RTRIM(a.Actual_ShiftCode)
									ELSE RTRIM(a.ShiftCode)
								END
						END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 
						WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
								THEN a.ShiftCode
								ELSE 
									CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
										THEN RTRIM(a.Actual_ShiftCode)
										ELSE RTRIM(a.ShiftCode)
									END
							END = 'O' THEN 'Dayoff'							
						WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
						WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
						WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
						WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'

						--'Late and Left Early'
						WHEN 
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							) > @lateAttendanceThreshold 	
							AND DATEDIFF
								(
									MINUTE,			
									CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
														THEN CONVERT(DATETIME, '')
														ELSE a.dtOUT
													END),
									CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
													THEN DATEADD
														(
															MINUTE, 
															CASE WHEN a.isRamadan = 1
																THEN 
																	CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																		THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																		ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																	END
																ELSE
																	CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																		THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																		ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																	END
															END, 
															b.dtIN
														)
													ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
												END)
								) > @earlyLeavingThreshold
							THEN 'LateAndLeftEarly'

						--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
						WHEN 
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							) > @lateAttendanceThreshold THEN 
							CASE 
								WHEN ISNULL
									(
										DATEDIFF
										(
											MINUTE, 
											CAST
											(
												CASE WHEN e.SettingID IS NOT NULL 
													THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
													ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
												END AS TIME
											), 
											CAST(b.dtIN AS TIME)
										)
									, 0) 
									>= 
									(
										CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										END
									)
									THEN 'Ontime'
								ELSE 'Late'
							END 

						--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @lateAttendanceThreshold 
						WHEN DATEDIFF
								(
									MINUTE,			
									CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
														THEN CONVERT(DATETIME, '')
														ELSE a.dtOUT
													END),
									CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
													THEN DATEADD
														(
															MINUTE, 
															CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
																THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
																ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
															END, 
															b.dtIN
														)
													ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
												END)
								) > @lateAttendanceThreshold
								THEN 'LeftEarly'
							ELSE 'Ontime'
						END
					IN ('Late', 'LeftEarly', 'LateAndLeftEarly')
				)
				AND		--Rev. #1.8 
				(
					ISNULL(a.CorrectionCode, '') = '' 
					OR (ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(a.CorrectionCode, 1, 2) NOT IN ('RA'))
				)
				AND NOT a.EmpNo BETWEEN 10005000 AND 10005999		--Rev. #1.9
		) A
			LEFT JOIN tas.Master_BusinessUnit_JDE B ON RTRIM(A.CostCenter) = RTRIM(B.BusinessUnit)
			LEFT JOIN tas.syJDE_F01151 C ON B.CostCenterManager = C.EAAN8 AND C.EAIDLN = 0 AND C.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(C.EAETP))) = 'E' 
			LEFT JOIN tas.syJDE_F01151 D ON B.Superintendent = D.EAAN8 AND D.EAIDLN = 0 AND D.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(D.EAETP))) = 'E' 
			LEFT JOIN tas.Master_Employee_JDE_View_V2 E ON b.CostCenterManager = E.EmpNo
			LEFT JOIN tas.Master_Employee_JDE_View_V2 F ON b.Superintendent = F.EmpNo
		--WHERE A.CostCenter = '7600'		--(Note: This line of code if for testing purpose only. Comment it upon deployment to production)
		ORDER BY A.CostCenter
    END
    
	ELSE IF @loadType = 1		--Get the attendance records of unpunctual employees 
	BEGIN
    
		SELECT DISTINCT 
			a.EmpNo, 
			c.EmpName,
			c.SupervisorNo,		--Rev. #1.3
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.BusinessUnit AS CostCenter, 
			RTRIM(d.BUname) AS CostCenterName,
			a.DT,
			CASE WHEN b.dtIN IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE b.dtIN
			END AS dtIN,
		
			CASE WHEN a.dtOUT IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE a.dtOUT
			END AS dtOUT,

			CASE WHEN e.SettingID IS NOT NULL 
				THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
			END AS MaxArrivalTime,

			CASE WHEN e.SettingID IS NOT NULL
				THEN DATEADD
					(
						MINUTE, 
						CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END AS TIME)
							THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
							ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
						END, 
						b.dtIN
					)
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
			END AS RequiredTimeOut,

			ISNULL
			(
				DATEDIFF
				(
					MINUTE, 
					CAST
					(
						CASE WHEN e.SettingID IS NOT NULL 
							THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
							ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
						END AS TIME
					), 
					CAST(b.dtIN AS TIME)
				)
			, 0) AS ArrivalTimeDiff,

			DATEDIFF
			(
				MINUTE,			
				CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
									THEN CONVERT(DATETIME, '')
									ELSE a.dtOUT
								END),
				CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
								THEN DATEADD
									(
										MINUTE, 
										CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										END, 
										b.dtIN
									)
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
							END)
			) AS DepartureTimeDiff,			--(Formula used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) 

			CASE 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END = 'O' THEN 'Dayoff'				
				WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
				WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
				WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
				WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'
				WHEN RTRIM(a.AbsenceReasonCode) = 'DD' THEN 'DIL'
				WHEN ISNULL(a.AbsenceReasonCode, '') <> '' AND RTRIM(a.AbsenceReasonCode) <> 'DD' THEN i.UDCDescription

				--'Late and Left Early'
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold 	
					AND DATEDIFF
						(
							MINUTE,			
							CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
												THEN CONVERT(DATETIME, '')
												ELSE a.dtOUT
											END),
							CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
											THEN DATEADD
												(
													MINUTE, 
													CASE WHEN a.isRamadan = 1
														THEN 
															CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
															END
														ELSE
															CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
															END
													END, 
													b.dtIN
												)
											ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
										END)
						) > @earlyLeavingThreshold
					THEN 'LateAndLeftEarly'

				--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold THEN 
					CASE 
						WHEN ISNULL
						(
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END  
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							)
						, 0) 
							>= 
							(
								CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
									THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
									ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
								END
							)
							THEN 'Ontime'
						ELSE 'Late'
					END 

				--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @earlyLeavingThreshold
				WHEN DATEDIFF
					(
						MINUTE,			
						CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
											THEN CONVERT(DATETIME, '')
											ELSE a.dtOUT
										END),
						CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
										THEN DATEADD
											(
												MINUTE, 
												CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
													THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
													ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
												END, 
												b.dtIN
											)
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
									END)
					) > @earlyLeavingThreshold
					THEN 'LeftEarly'
				ELSE 'Ontime'
			END AS Remarks,
			a.LeaveType,
			a.GradeCode			
		FROM tas.Tran_Timesheet a
			OUTER APPLY
			(
				SELECT TOP 1 dtIN, dtOUT 
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo				
			) b
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view d ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
				AND 
				(
					CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.fnGetUDCList('55', 'RA') i ON RTRIM(a.AbsenceReasonCode) = i.UDCCOde	
		WHERE 
			a.IsLastRow = 1
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND a.EmpNo IN (SELECT EmpNo FROM tas.fnGetNotPuctualEmployeesAll(@startDate, @endDate, @costCenter, @occurenceLimit, @lateAttendanceThreshold, @earlyLeavingThreshold))
			--AND c.GradeCode <= 11	
			AND tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo)  = 0	--Rev. #1.1
			AND 
			(
				CASE 
					WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 
					WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END = 'O' THEN 'Dayoff'						
					WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
					WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
					WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
					WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'

					--'Late and Left Early'
					WHEN 
						DATEDIFF
						(
							MINUTE, 
							CAST
							(
								CASE WHEN e.SettingID IS NOT NULL 
									THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
									ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
								END AS TIME
							), 
							CAST(b.dtIN AS TIME)
						) > @lateAttendanceThreshold 	
						AND DATEDIFF
							(
								MINUTE,			
								CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
													THEN CONVERT(DATETIME, '')
													ELSE a.dtOUT
												END),
								CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
												THEN DATEADD
													(
														MINUTE, 
														CASE WHEN a.isRamadan = 1
															THEN 
																CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																	THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																	ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																END
															ELSE
																CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																	THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																	ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																END
														END, 
														b.dtIN
													)
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
											END)
							) > @earlyLeavingThreshold
						THEN 'LateAndLeftEarly'

					--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
					WHEN 
						DATEDIFF
						(
							MINUTE, 
							CAST
							(
								CASE WHEN e.SettingID IS NOT NULL 
									THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
									ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
								END AS TIME
							), 
							CAST(b.dtIN AS TIME)
						) > @lateAttendanceThreshold THEN 
						CASE 
							WHEN ISNULL
								(
									DATEDIFF
									(
										MINUTE, 
										CAST
										(
											CASE WHEN e.SettingID IS NOT NULL 
												THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
											END AS TIME
										), 
										CAST(b.dtIN AS TIME)
									)
								, 0) 
								>= 
								(
									CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
										THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
									END
								)
								THEN 'Ontime'
							ELSE 'Late'
						END 

					--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @lateAttendanceThreshold 
					WHEN DATEDIFF
							(
								MINUTE,			
								CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
													THEN CONVERT(DATETIME, '')
													ELSE a.dtOUT
												END),
								CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
												THEN DATEADD
													(
														MINUTE, 
														CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
															THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
															ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
														END, 
														b.dtIN
													)
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
											END)
							) > @lateAttendanceThreshold
							THEN 'LeftEarly'
						ELSE 'Ontime'
					END
				IN ('Late', 'LeftEarly', 'LateAndLeftEarly')
			)
			AND		--Rev. #1.8 
			(
				ISNULL(a.CorrectionCode, '') = '' 
				OR (ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(a.CorrectionCode, 1, 2) NOT IN ('RA'))
			)
			AND NOT a.EmpNo BETWEEN 10005000 AND 10005999		--Rev. #1.9
		ORDER BY CostCenter, EmpNo, DT 
	END 

	ELSE IF @loadType = 2		--Get the attendance records to be saved into "tas.AttendancePunctualityLog" table
	BEGIN
    
		SELECT DISTINCT 
			a.EmpNo, 
			c.EmpName,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			RTRIM(c.BusinessUnit) AS CostCenter, 
			RTRIM(d.BUname) AS CostCenterName,
			a.DT,
			CASE WHEN b.dtIN IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE b.dtIN
			END AS dtIN,
		
			CASE WHEN a.dtOUT IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE a.dtOUT
			END AS dtOUT,

			CASE WHEN e.SettingID IS NOT NULL 
				THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
			END AS MaxArrivalTime,

			CASE WHEN e.SettingID IS NOT NULL
				THEN DATEADD
					(
						MINUTE, 
						CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END AS TIME)
							THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
							ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
						END, 
						b.dtIN
					)
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
			END AS RequiredTimeOut,

			ISNULL
			(
				DATEDIFF
				(
					MINUTE, 
					CAST
					(
						CASE WHEN e.SettingID IS NOT NULL 
							THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
							ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
						END AS TIME
					), 
					CAST(b.dtIN AS TIME)
				)
			, 0) AS ArrivalTimeDiff,

			DATEDIFF
			(
				MINUTE,			
				CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
									THEN CONVERT(DATETIME, '')
									ELSE a.dtOUT
								END),
				CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
								THEN DATEADD
									(
										MINUTE, 
										CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										END, 
										b.dtIN
									)
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
							END)
			) AS DepartureTimeDiff,			--(Formula used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) 

			0 AS ExtraTimeDiff,

			CASE 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END = 'O' THEN 'Dayoff'				
				WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
				WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
				WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
				WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'
				WHEN RTRIM(a.AbsenceReasonCode) = 'DD' THEN 'DIL'
				WHEN ISNULL(a.AbsenceReasonCode, '') <> '' AND RTRIM(a.AbsenceReasonCode) <> 'DD' THEN i.UDCDescription

				--'Late and Left Early'
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold 	
					AND DATEDIFF
						(
							MINUTE,			
							CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
												THEN CONVERT(DATETIME, '')
												ELSE a.dtOUT
											END),
							CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
											THEN DATEADD
												(
													MINUTE, 
													CASE WHEN a.isRamadan = 1
														THEN 
															CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
															END
														ELSE
															CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
															END
													END, 
													b.dtIN
												)
											ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
										END)
						) > @earlyLeavingThreshold
					THEN 'LateAndLeftEarly'

				--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold THEN 
					CASE 
						WHEN ISNULL
						(
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END  
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							)
						, 0) 
							>= 
							(
								CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
									THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
									ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
								END
							)
							THEN 'Ontime'
						ELSE 'Late'
					END 

				--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @earlyLeavingThreshold
				WHEN DATEDIFF
					(
						MINUTE,			
						CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
											THEN CONVERT(DATETIME, '')
											ELSE a.dtOUT
										END),
						CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
										THEN DATEADD
											(
												MINUTE, 
												CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
													THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
													ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
												END, 
												b.dtIN
											)
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
									END)
					) > @earlyLeavingThreshold
					THEN 'LeftEarly'
				ELSE 'Ontime'
			END AS Remarks,
			a.LeaveType
		FROM tas.Tran_Timesheet a
			OUTER APPLY
			(
				SELECT TOP 1 dtIN, dtOUT 
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo				
			) b
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view d ON RTRIM(c.BusinessUnit) = RTRIM(d.BU)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
				AND 
				(
					CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, g.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.fnGetUDCList('55', 'RA') i ON RTRIM(a.AbsenceReasonCode) = i.UDCCOde	--Rev. #1.6
		WHERE 
			a.IsLastRow = 1
			--AND ISNULL(a.IsOnDutyRota, 0) = 0	--Rev. #1.3
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND a.EmpNo IN (SELECT EmpNo FROM tas.fnGetNotPuctualEmployeesAll(@startDate, @endDate, @costCenter, @occurenceLimit, @lateAttendanceThreshold, @earlyLeavingThreshold))
			--AND c.GradeCode <= 11	
			AND tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo)  = 0	--Rev. #1.1
			AND 
			(
				(
					CASE 
						WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
							THEN a.ShiftCode
							ELSE 
								CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
									THEN RTRIM(a.Actual_ShiftCode)
									ELSE RTRIM(a.ShiftCode)
								END
						END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 

						WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
							THEN a.ShiftCode
							ELSE 
								CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
									THEN RTRIM(a.Actual_ShiftCode)
									ELSE RTRIM(a.ShiftCode)
								END
						END = 'O' THEN 'Dayoff'	

						WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
						WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
						WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
						WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'

						--'Late and Left Early'
						WHEN 
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							) > @lateAttendanceThreshold 	
							AND DATEDIFF
								(
									MINUTE,			
									CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
														THEN CONVERT(DATETIME, '')
														ELSE a.dtOUT
													END),
									CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
													THEN DATEADD
														(
															MINUTE, 
															CASE WHEN a.isRamadan = 1
																THEN 
																	CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																		THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																		ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																	END
																ELSE
																	CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																		THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																		ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																	END
															END, 
															b.dtIN
														)
													ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
												END)
								) > @earlyLeavingThreshold
							THEN 'LateAndLeftEarly'

						--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
						WHEN 
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							) > @lateAttendanceThreshold THEN 
							CASE 
								WHEN ISNULL
									(
										DATEDIFF
										(
											MINUTE, 
											CAST
											(
												CASE WHEN e.SettingID IS NOT NULL 
													THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
													ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
												END AS TIME
											), 
											CAST(b.dtIN AS TIME)
										)
									, 0) 
									>= 
									(
										CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										END
									)
									THEN 'Ontime'
								ELSE 'Late'
							END 

						--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @lateAttendanceThreshold 
						WHEN DATEDIFF
							(
								MINUTE,			
								CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
													THEN CONVERT(DATETIME, '')
													ELSE a.dtOUT
												END),
								CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
												THEN DATEADD
													(
														MINUTE, 
														CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
															THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
															ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
														END, 
														b.dtIN
													)
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
											END)
							) > @lateAttendanceThreshold
							THEN 'LeftEarly'
							ELSE 'Ontime'
						END
					IN ('Late', 'LeftEarly', 'LateAndLeftEarly')
				)
			)
			AND		--Rev. #1.8 
			(
				ISNULL(a.CorrectionCode, '') = '' 
				OR (ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(a.CorrectionCode, 1, 2) NOT IN ('RA'))
			)
			AND NOT a.EmpNo BETWEEN 10005000 AND 10005999		--Rev. #1.9
		ORDER BY CostCenter, EmpNo, DT 
	END 

	ELSE IF @loadType = 3		--Get the attendance records of unpunctual managers (grade 12 and above)
	BEGIN
    
		SELECT DISTINCT 
			a.EmpNo, 
			c.EmpName,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			RTRIM(c.BusinessUnit) AS CostCenter, 
			RTRIM(d.BusinessUnitName) AS CostCenterName,
			d.GroupCode,
			a.DT,
			CASE WHEN b.dtIN IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE b.dtIN
			END AS dtIN,
		
			CASE WHEN a.dtOUT IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE a.dtOUT
			END AS dtOUT,

			CASE WHEN e.SettingID IS NOT NULL 
				THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
			END AS MaxArrivalTime,

			CASE WHEN e.SettingID IS NOT NULL
				THEN DATEADD
					(
						MINUTE, 
						CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END AS TIME)
							THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
							ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN g.RDepartFrom ELSE g.DepartFrom END)
						END, 
						b.dtIN
					)
				ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
			END AS RequiredTimeOut,

			ISNULL
			(
				DATEDIFF
				(
					MINUTE, 
					CAST
					(
						CASE WHEN e.SettingID IS NOT NULL 
							THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
							ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
						END AS TIME
					), 
					CAST(b.dtIN AS TIME)
				)
			, 0) AS ArrivalTimeDiff,

			DATEDIFF
			(
				MINUTE,			
				CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
									THEN CONVERT(DATETIME, '')
									ELSE a.dtOUT
								END),
				CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
								THEN DATEADD
									(
										MINUTE, 
										CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										END, 
										b.dtIN
									)
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
							END)
			) AS DepartureTimeDiff,			--(Formula used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) 

			CASE 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 
				WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END = 'O' THEN 'Dayoff'				
				WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
				WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
				WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
				WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'
				WHEN RTRIM(a.AbsenceReasonCode) = 'DD' THEN 'DIL'
				WHEN ISNULL(a.AbsenceReasonCode, '') <> '' AND RTRIM(a.AbsenceReasonCode) <> 'DD' THEN i.UDCDescription

				--'Late and Left Early'
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold 	
					AND DATEDIFF
						(
							MINUTE,			
							CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
												THEN CONVERT(DATETIME, '')
												ELSE a.dtOUT
											END),
							CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
											THEN DATEADD
												(
													MINUTE, 
													CASE WHEN a.isRamadan = 1
														THEN 
															CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
															END
														ELSE
															CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
															END
													END, 
													b.dtIN
												)
											ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
										END)
						) > @earlyLeavingThreshold
					THEN 'LateAndLeftEarly'

				--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
				WHEN 
					DATEDIFF
					(
						MINUTE, 
						CAST
						(
							CASE WHEN e.SettingID IS NOT NULL 
								THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
								ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
							END AS TIME
						), 
						CAST(b.dtIN AS TIME)
					) > @lateAttendanceThreshold THEN 
					CASE 
						WHEN ISNULL
						(
							DATEDIFF
							(
								MINUTE, 
								CAST
								(
									CASE WHEN e.SettingID IS NOT NULL 
										THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END  
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END
									END AS TIME
								), 
								CAST(b.dtIN AS TIME)
							)
						, 0) 
							>= 
							(
								CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
									THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
									ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
								END
							)
							THEN 'Ontime'
						ELSE 'Late'
					END 

				--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @earlyLeavingThreshold
				WHEN DATEDIFF
					(
						MINUTE,			
						CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
											THEN CONVERT(DATETIME, '')
											ELSE a.dtOUT
										END),
						CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
										THEN DATEADD
											(
												MINUTE, 
												CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
													THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
													ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
												END, 
												b.dtIN
											)
										ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
									END)
					) > @earlyLeavingThreshold
					THEN 'LeftEarly'
				ELSE 'Ontime'
			END AS Remarks,
			a.LeaveType
		FROM tas.Tran_Timesheet a
			OUTER APPLY
			(
				SELECT TOP 1 dtIN, dtOUT 
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo				
			) b
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_V2 d ON RTRIM(c.BusinessUnit) = RTRIM(d.BusinessUnit)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
				AND 
				(
					CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN g.RArrivalTo ELSE g.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.fnGetUDCList('55', 'RA') i ON RTRIM(a.AbsenceReasonCode) = i.UDCCOde	--Rev. #1.6
		WHERE 
			a.IsLastRow = 1
			--AND ISNULL(a.IsOnDutyRota, 0) = 0	--Rev. #1.3
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			--AND a.EmpNo IN (SELECT EmpNo FROM tas.fnGetNotPuctualManagers(@startDate, @endDate, @costCenter, @occurenceLimit, @lateAttendanceThreshold, @earlyLeavingThreshold))
			AND a.EmpNo IN (SELECT EmpNo FROM tas.fnGetNotPuctualManagersAll(@startDate, @endDate, @costCenter, @occurenceLimit, @lateAttendanceThreshold, @earlyLeavingThreshold))		--Rev. #1.6	
			AND c.GradeCode >= 12
			AND tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo)  = 0	--Rev. #1.1
			AND 
			(
				CASE 
					WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END <> 'O' AND a.ShiftSpan = 1 THEN 'Ontime' 

					WHEN CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END = 'O' THEN 'Dayoff'	
											
					WHEN a.IsPublicHoliday = 1 OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1) THEN 'Holiday'
					WHEN RTRIM(a.RemarkCode) = 'A' THEN 'Absent'
					WHEN RTRIM(ISNULL(a.RemarkCode, '')) <> 'A' AND ISNULL(a.LeaveType, '') = '' AND ISNULL(a.DIL_Entitlement, '') = '' AND ISNULL(a.AbsenceReasonCode, '') = '' AND (a.dtIN IS NULL OR a.dtOUT IS NULL AND a.IsLastRow = 1) THEN 'MissingSwipe'
					WHEN ISNULL(a.LeaveType, '') <> '' THEN 'OnLeave'

					--'Late and Left Early'
					WHEN 
						DATEDIFF
						(
							MINUTE, 
							CAST
							(
								CASE WHEN e.SettingID IS NOT NULL 
									THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END 
									ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
								END AS TIME
							), 
							CAST(b.dtIN AS TIME)
						) > @lateAttendanceThreshold 	
						AND DATEDIFF
							(
								MINUTE,			
								CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
													THEN CONVERT(DATETIME, '')
													ELSE a.dtOUT
												END),
								CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
												THEN DATEADD
													(
														MINUTE, 
														CASE WHEN a.isRamadan = 1
															THEN 
																CASE WHEN CAST(h.RDepartFrom AS TIME) > CAST(h.RArrivalTo AS TIME)
																	THEN DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																	ELSE 1440 + DATEDIFF(MINUTE, h.RArrivalTo, h.RDepartFrom)
																END
															ELSE
																CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
																	THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																	ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
																END
														END, 
														b.dtIN
													)
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END 
											END)
							) > @earlyLeavingThreshold
						THEN 'LateAndLeftEarly'

					--("Late" Formula Used: DATEDIFF(MINUTE, MaxArrivalTime, dtIN) > @@lateAttendanceThreshold
					WHEN 
						DATEDIFF
						(
							MINUTE, 
							CAST
							(
								CASE WHEN e.SettingID IS NOT NULL 
									THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
									ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
								END AS TIME
							), 
							CAST(b.dtIN AS TIME)
						) > @lateAttendanceThreshold THEN 
						CASE 
							WHEN ISNULL
								(
									DATEDIFF
									(
										MINUTE, 
										CAST
										(
											CASE WHEN e.SettingID IS NOT NULL 
												THEN CASE WHEN a.isRamadan = 1 THEN e.RamadanArrivalTo ELSE e.NormalArrivalTo END
												ELSE CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END 
											END AS TIME
										), 
										CAST(b.dtIN AS TIME)
									)
								, 0) 
								>= 
								(
									CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
										THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
										ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
									END
								)
								THEN 'Ontime'
							ELSE 'Late'
						END 

					--("Left Early" Formula Used: DATEDIFF(MINUTE, dtOUT, RequiredTimeOut) > @lateAttendanceThreshold 
					WHEN DATEDIFF
						(
							MINUTE,			
							CONVERT(TIME, CASE WHEN a.dtOUT IS NULL 
												THEN CONVERT(DATETIME, '')
												ELSE a.dtOUT
											END),
							CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
											THEN DATEADD
												(
													MINUTE, 
													CASE WHEN CAST(CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END AS TIME) > CAST(CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END AS TIME)
														THEN DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
														ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN a.isRamadan = 1 THEN h.RArrivalTo ELSE h.ArrivalTo END, CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END)
													END, 
													b.dtIN
												)
											ELSE CASE WHEN a.isRamadan = 1 THEN h.RDepartFrom ELSE h.DepartFrom END
										END)
						) > @lateAttendanceThreshold
						THEN 'LeftEarly'
						ELSE 'Ontime'
					END
				IN ('Late', 'LeftEarly', 'LateAndLeftEarly')
			)
			AND		--Rev. #1.8 
			(
				ISNULL(a.CorrectionCode, '') = '' 
				OR (ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(a.CorrectionCode, 1, 2) NOT IN ('RA'))
			)
			AND NOT a.EmpNo BETWEEN 10005000 AND 10005999		--Rev. #1.9
		ORDER BY CostCenter, EmpNo, DT 
	END 

END

/*	Debug:

	EXEC tas.Pr_PunctualityMonthlyReport 1, '06/01/2020', '06/30/2020'		--Get  all employees
	EXEC tas.Pr_PunctualityMonthlyReport 3, '06/14/2020', '06/20/2020'		--Grade 12 and above

PARAMETERS:	
	@loadType					TINYINT,
	@startDate					DATETIME,
	@endDate					DATETIME,
	@costCenter					VARCHAR(12) = '',
	@occurenceLimit				INT = 0,
	@lateAttendanceThreshold	INT = 0,
	@earlyLeavingThreshold		INT = 0

*/

