/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDutyROTAOnHoliday
*	Description: Get employees which will be given DIL for coming to work during weekends which happened to be a public holiday at the same time
*
*	Date			Author		Rev. #		Comments:
*	05/01/2017		Ervin		1.0			Created
*	01/08/2020		Ervin		1.1			Refactored the code to enhance data retrieval performance
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDutyROTAOnHoliday
(
	@LoadType	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME
)
AS
	SET NOCOUNT ON 
	
	DECLARE @HolidayFrom datetime, @HolidayTo datetime
	SELECT	@HolidayFrom = CONVERT(datetime, '01/01/' + CONVERT(varchar(4), DATEPART(year,@StartDate))),
			@HolidayTo = CONVERT(datetime, '12/31/' + CONVERT(varchar(4), DATEPART(year,@StartDate)))

	IF @LoadType = 0 --Based on @StartDate and @EndDate
	BEGIN 	

		SELECT * FROM
		(
			SELECT 
				a.AutoID,
				a.EmpNo,
				b.EmpName,
				f.[JobTitle],
				g.YAAN8 as SupervisorEmpNo,
				h.EmpName as SupervisorEmpName,
				a.businessunit as CostCenter,
				c.businessunitname as CostCenterName,
				DT,
				dtin,
				dtout,
				shiftpatcode,
				shiftcode,
				dil_entitlement,
				d.Effective_ShiftPatCode,
				d.Effective_ShiftPointer,
				d.Effective_ShiftCode,
				a.IsOnDutyRota,
				a.IsPublicHoliday,
				rtrim(e.HODESC) as HolidayDesc,
				a.Duration_Worked_Cumulative as DurationWorked,
				@HolidayFrom as HolidayFrom,
				@HolidayTo as HolidayTo,
				'DOW' = (select datename(dw, a.DT)),
				'DayOffType' = (select tas.fnCheckDayOff(datename(dw, a.DT), a.EmpNo, a.DT)),
				IsWorked6Hours = CASE WHEN DATEDIFF(mi, dtIn, dtOut) > 360 THEN 1 ELSE 0 END 
			FROM tas.tran_timesheet a WITH (NOLOCK)
				INNER JOIN tas.master_Employee_JDE_VIew b WITH (NOLOCK) on a.EmpNo = b.EmpNo 
				INNER JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) on rtrim(a.BusinessUnit) = rtrim(c.BusinessUnit)
				INNER JOIN tas.tran_shiftpatternupdates d WITH (NOLOCK) on a.EmpNo = d.EmpNo
				INNER JOIN 
				(
					select * from tas.syJDE_F55HOLID WITH (NOLOCK)   
					where tas.ConvertFromJulian(HOHDT) between @HolidayFrom and @HolidayTo
					and rtrim(HOHLCD) in ('D','H')
				) e on a.dt = tas.ConvertFromJulian(e.HOHDT) 
				LEFT JOIN tas.Master_JobTitles_JDE f WITH (NOLOCK) on a.EmpNo = f.EmpNo
				INNER JOIN [tas].[syJDE_F060116] g WITH (NOLOCK) on a.EmpNo = g.YAAN8
				LEFT JOIN tas.master_Employee_JDE_VIew h WITH (NOLOCK) on g.YAANPA = h.EmpNo 
			WHERE
				a.ShiftCode = 'O'
				and isnull(a.IsSalStaff,0) = 1
				and isnull(a.isdayworker_or_shifter,0) = 1
				and isnull(a.IsPublicHoliday,0) = 1
				and isnull(a.IsLastRow,0) = 1
				and isnull(a.IsOnDutyRota,0) = 1
				and a.DT = d.DateX
				and a.Duration_Worked_Cumulative >= 360
				and (isnull(a.DIL_Entitlement,'') = '' or isnull(a.DIL_Entitlement,'') = 'ES')
				and a.dt between @StartDate and @EndDate
		) tblMain
		WHERE DayOffType in (1,2) --(Note: 1 => Friday/Saturday dayoff; 2 => Other dayoffs which is not Friday or Saturday)
		ORDER BY CostCenter, EmpNo, DT
	END

	ELSE 
	BEGIN --Based on specific date

		SELECT * FROM
		(
			SELECT 
				a.AutoID,
				a.EmpNo,
				b.EmpName,
				f.[JobTitle],
				g.YAAN8 as SupervisorEmpNo,
				h.EmpName as SupervisorEmpName,
				a.businessunit as CostCenter,
				c.businessunitname as CostCenterName,
				dt,
				dtin,
				dtout,
				shiftpatcode,
				shiftcode,
				dil_entitlement,
				d.Effective_ShiftPatCode,
				d.Effective_ShiftPointer,
				d.Effective_ShiftCode,
				a.IsOnDutyRota,
				a.IsPublicHoliday,
				rtrim(e.HODESC) as HolidayDesc,
				DATEDIFF(mi,dtIn,dtOut) as DurationWorked,
				@HolidayFrom as HolidayFrom,
				@HolidayTo as HolidayTo,
				'DOW' = (select datename(dw, a.DT)),
				'DayOffType' = (select tas.fnCheckDayOff(datename(dw, a.DT), a.EmpNo, a.DT)),
				IsWorked6Hours = CASE WHEN DATEDIFF(mi, dtIn, dtOut) > 360 THEN 1 ELSE 0 END 
			FROM tas.tran_timesheet a WITH (NOLOCK)
				INNER JOIN tas.master_Employee_JDE_VIew b WITH (NOLOCK) on a.EmpNo = b.EmpNo 
				INNER JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) on rtrim(a.BusinessUnit) = rtrim(c.BusinessUnit)
				INNER JOIN tas.tran_shiftpatternupdates d WITH (NOLOCK) on a.EmpNo = d.EmpNo
				INNER JOIN 
				(
					select * from tas.syJDE_F55HOLID WITH (NOLOCK)   
					where tas.ConvertFromJulian(HOHDT) between @HolidayFrom and @HolidayTo
					and rtrim(HOHLCD) in ('D','H')
				) e on a.dt = tas.ConvertFromJulian(e.HOHDT) 
				LEFT JOIN tas.Master_JobTitles_JDE f WITH (NOLOCK) on a.EmpNo = f.EmpNo
				INNER JOIN [tas].[syJDE_F060116] g WITH (NOLOCK) on a.EmpNo = g.YAAN8
				LEFT JOIN tas.master_Employee_JDE_VIew h WITH (NOLOCK) on g.YAANPA = h.EmpNo 
			WHERE
				a.ShiftCode = 'O'
				and isnull(a.IsSalStaff,0) = 1
				and isnull(a.isdayworker_or_shifter,0) = 1
				and isnull(a.IsPublicHoliday,0) = 1
				and isnull(a.IsLastRow,0) = 1
				and isnull(a.IsOnDutyRota,0) = 1
				and a.DT = d.DateX
				and a.Duration_Worked_Cumulative >= 360
				and (isnull(a.DIL_Entitlement,'') = '' or isnull(a.DIL_Entitlement,'') = 'ES')
				and a.dt = @StartDate
		) tblMain
		WHERE DayOffType in (1,2)	--(Note: 1 => Friday/Saturday dayoff; 2 => Other dayoffs which is not Friday or Saturday)
		ORDER BY CostCenter, EmpNo, DT
	END

GO 

/*	Debug:

	EXEC tas.Pr_GetDutyROTAOnHoliday 1, '08/01/2020', null 

PARAMETERS:
	@LoadType	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME

*/
