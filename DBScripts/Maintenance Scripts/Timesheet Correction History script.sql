	--select DT,empno, * from tran_timesheet where autoid = 4541433

	EXEC Show_AUDIT_Tran_Timesheet 4541433

	--SELECT a.Processed, * FROM tas.Tran_Timesheet a
	--WHERE a.AutoID = 4541433

	select  XID_AutoID , 
			 Autoid , 
			 EmpNo , 
			 EffectiveDate EffectiveDate  , 
			 EndingDate EndingDate  , 
			 ShiftPatCode  , 
			 ShiftPointer , 
			 ChangeType , 
			 action_type , 
			 action_machine ,  
			 DB_ActionTime 
	  from tas.vAUDIT_Tran_ShiftPatternChanges  
	  where EmpNo='10003493' 
	  and EffectiveDate >= '2016-jan-16'  
	  and (EndingDate    <= '2016-jan-16' or EndingDate is null) 
	  and changetype='T' 
	  union  
	  select top 1 
			 XID_AutoID , 
			 Autoid , 
			 EmpNo , 
			 EffectiveDate EffectiveDate  , 
			 EndingDate EndingDate  , 
			 ShiftPatCode  , 
			 ShiftPointer , 
			 ChangeType , 
			 action_type , 
			 action_machine ,  
			 DB_ActionTime 
	  from tas.vAUDIT_Tran_ShiftPatternChanges  
	  where EmpNo='10003493' 
	  and EffectiveDate >= '2016-jan-16'  
	  and (EndingDate    <= '2016-jan-16' or EndingDate is null) 
	  and changeType='D' 
	 order by XID_AutoID  desc 

	select * 
	from Tran_Absence 
	where '02/28/2016' >= EffectiveDate 
	and  '02/28/2016' <= EndingDate
	and empNo=10003493 

	 select * 
	  from Tran_Absence 
	  where '16-FEB-16'>= EffectiveDate 
	  and  '16-FEB-16'<= EndingDate
	  and empNo=10003617

	select * from  tran_Leave_JDE
	 where EmpNo=10003493 
	 and 160216 >= FromDate and 160216  <= ToDate

	 select * from  tran_Leave_JDE
	 where EmpNo=10003493 
	 and 160116 >= FromDate and 150216  <= ToDate


