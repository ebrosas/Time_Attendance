DECLARE	@startDate	DATETIME,
		@endDate	DATETIME

SELECT	@startDate	= '05/17/2018',
		@endDate	= '06/16/2018'

	SELECT b.DT, b.EmpNo, b.dtIN, b.dtOUT, b.OTStartTime AS OTStartTime_TS, b.OTEndTime AS OTEndTime_TS, b.OTType AS OTType_TS, 
		b.CorrectionCode,
		a.* 
	FROM tas.Tran_Timesheet_Extra a
		INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
	WHERE 
		(b.isRamadan = 1 OR b.IsPublicHoliday = 1)
		AND 
		(
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND 
			(b.OTStartTime IS NOT NULL AND b.OTEndTime IS NOT NULL)	
		)
		AND ISNULL(b.CorrectionCode, '') = ''
		AND a.OTApproved = '0'
		AND b.DT BETWEEN @startDate AND @endDate
	ORDER BY b.DT DESC
