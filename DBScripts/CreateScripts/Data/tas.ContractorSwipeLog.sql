
	BEGIN TRAN T1


	INSERT INTO tas.ContractorSwipeLog
           ([EmpNo]
           ,[SwipeDate]
           ,[SwipeTime]
           ,[LocationName]
           ,[ReaderName]
           ,[SwipeType]
           ,[LocationCode]
           ,[ReaderNo]
           ,[ContractorName]
           ,[EventCode]
           ,[CreatedDate]
           ,[CreatedByEmpNo]
           ,[CreatedByUserID])
     SELECT	DISTINCT
			a.EmpNo,
			a.SwipeDate,
			a.SwipeTime,
			a.LocationName,
			a.ReaderName,
			a.SwipeType,
			a.LocationCode,
			a.ReaderNo,
			RTRIM(a.LName) AS ContractorName,
			a.Event,
			GETDATE(),
			0,
			'System Admin' 
	 FROM tas.Vw_ContractorSwipe a
	 ORDER BY a.SwipeDate DESC, a.SwipeTime

	 ROLLBACK TRAN T1
	 COMMIT TRAN T1
