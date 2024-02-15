	--Get all Aspire overtime
	SELECT * FROM tas.Tran_Timesheet_OTRemoved a
	WHERE a.CostCenter = '7920'
		AND a.OTDate BETWEEN '06/06/2016' AND '07/05/2016'
	ORDER BY a.OTDate DESC 

	--Get overtime less than 30 minutes
	SELECT	b.isRamadan, 
			DATEDIFF(n, b.OTStartTime, b.OTEndTime) AS OTDuration,
			b.* 
	FROM tas.Tran_Timesheet_OTRemoved a
		INNER JOIN tas.Tran_Timesheet b ON a.TS_AutoID = b.AutoID
	WHERE a.CostCenter = '7920'
		AND a.OTDate BETWEEN '06/06/2016' AND '07/05/2016'
		AND DATEDIFF(n, b.OTStartTime, b.OTEndTime) < 30
	ORDER BY a.OTDate DESC 

/*

	EXEC tas.Pr_GetRemovedOT '06/06/2016'

	SELECT	Minutes_MinOT_NSS AS FLAG_Minutes_MinOT_NSS,
			Minutes_MinOT_SS AS FLAG_Minutes_MinOT_SS,
			Minutes_MinOT_SS_Ramadan AS FLAG_Minutes_MinOT_SS_Ramadan,
			Minutes_MinShiftAllowance AS FLAG_Minutes_MinShiftAllowance
	FROM tas.System_Values

*/