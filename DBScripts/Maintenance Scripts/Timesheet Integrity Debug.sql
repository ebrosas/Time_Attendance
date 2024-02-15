	--Add OT, but there is no OT
	SELECT AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC]
	from vuEditTimesheet 
	where correctionCode is not  null 
	and correctionCode like 'AO%'  
	and tas.fmtMIN(OtstartTime) = 0 
	and tas.fmtMIN(OtEndTime) = 0 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Add NoPayHours, but there are no NoPayHours
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC]
	from vuEditTimesheet 
	where correctionCode is not  null 
	and correctionCode like 'AN%'  
	and (NoPayhours =0 or NoPayhours  is null)   
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Add Shift Allowance, but there is Shift Allowance
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC]
	from vuEditTimesheet 
	where correctionCode is not  null 
	and correctionCode like 'AS%'  
	and shiftAllowance=0 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Mark Absent NoPayHours
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC], a.RemarkCode
	from vuEditTimesheet a
	where correctionCode is not  null 
	and correctionCode like 'MA%'  
	and (remarkCode <> 'A' or remarkCode is null)  
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Mark DIL, but there is DIL
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC]
	from vuEditTimesheet 
	where correctionCode is not  null 
	and correctionCode like 'MD%'  
	and DIL_Entitlement is null 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Remove OT, but still there is OT
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC], a.OTStartTime, a.OTEndTime
	from vuEditTimesheet a
	where correctionCode is not  null 
	and correctionCode like 'RO%'  
	and tas.fmtMIN(OtstartTime) <> 0 
	and tas.fmtMIN(OtEndTime) <> 0 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Remove NoPayHour, but still there is NoPayHour
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC], a.NoPayHours
	from vuEditTimesheet a
	where correctionCode is not  null 
	and correctionCode like 'RN%'  
	and nopayhours is not null 
	and nopayhours <>0 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Remove Shift Allowances, but it is not removed
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC]
	from vuEditTimesheet 
	where correctionCode is not  null 
	and correctionCode like 'RS%'  
	and shiftAllowance=1 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Remove Absence, but still there is Absence
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC], a.RemarkCode
	from vuEditTimesheet a 
	where correctionCode is not  null 
	and correctionCode like 'RA%'  
	and remarkCode = 'A' 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  

	--Remove DIL, but still there is DIL
	select AutoID,CorrectionCode [C.Code],tas.fmtdate(dt) [Dt],tas.fmtTime(dtIn) [IN],tas.fmtTime(dtIn) [Out],empno [EmpNO],businessunit [CC], a.DIL_Entitlement
	from vuEditTimesheet a
	where correctionCode is not  null 
	and correctionCode like 'RD%'  
	and DIL_Entitlement is not null 
	and '01-Jan-2004'<=Dt and '05-Jul-2016' >= Dt 
	order by dt  



	SELECT TOP 100 a.LastUpdateUser, a.ShiftAllowance, 
	* FROM tas.Tran_Timesheet a
	WHERE a.IsLastRow = 1
		AND ISNULL(a.LastUpdateUser, '') <> ''
		AND RTRIM(a.LastUpdateUser) <> 'System Admin'
	ORDER BY dt DESC 

	SELECT tas.fmtMIN(a.OTStartTime), tas.fmtMIN(a.OTEndTime),
		a.OTStartTime, a.OTEndTime, a.CorrectionCode,
	* FROM tas.Tran_Timesheet a
	WHERE a.IsLastRow = 1
		AND RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'AO%'
		AND a.OTStartTime IS NOT NULL	
		AND a.OTEndTime IS NOT NULL
	ORDER BY dt DESC 

	SELECT tas.fmtMIN(a.OTStartTime), tas.fmtMIN(a.OTEndTime),
		a.OTStartTime, a.OTEndTime, a.CorrectionCode,
	* FROM tas.Tran_Timesheet a
	WHERE a.IsLastRow = 1
		AND RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'AO%'
		AND (a.OTStartTime IS NULL OR a.OTEndTime IS NULL)
	ORDER BY dt DESC 

		--Retrieve all Timesheet Correction Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-T0'
	ORDER BY LTRIM(RTRIM(DRDL01))