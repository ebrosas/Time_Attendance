DECLARE	@actionType			TINYINT,	--(Note: 0 = Search records for undo operation; 1 = Insert new records; 2 = Delete existing records)
		@isCommitTrans		BIT,
		@empNo				INT,
		@startDate			DATETIME,
		@endDate			DATETIME 	 	

SELECT	@actionType			= 0,
		@isCommitTrans		= 0,
		@empNo				= 0,	
		@startDate			= NULL,
		@endDate			= NULL 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@startDate, '') = '' OR CONVERT(DATETIME, '') = @startDate
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = '' OR CONVERT(DATETIME, '') = @endDate
		SET @endDate = NULL

	IF @actionType = 0	--Find records to recover back the overtime
	BEGIN

		SELECT DISTINCT 
			b.Duration_Required,
			b.AutoID, b.ShiftSpan, b.ShiftSpanDate, b.ShiftSpan_XID, b.ShiftSpan_HoursDay1, b.ShiftSpan_HoursDay2, b.ShiftSpan_AwardOT, b.ShiftSpan_2ndDayFullOT, 		
			a.LogID,
			a.SourceTableName,
			a.EmpNo,
			a.DT,
			a.TS_AutoID,
			a.CostCenter,
			a.OTStartTime,
			a.OTEndTime,
			a.OTType,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByUserID,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateUserID
		FROM tas.OvertimeRemovalLog a
			CROSS APPLY
			(
				SELECT * FROM tas.Tran_Timesheet
				WHERE EmpNo = a.EmpNo
					AND DT = a.DT
					AND ISNULL(ShiftSpan_XID, 0) > 0	--(Note: If "ShiftSpan_XID" and "ShiftSpanDate" are not null, then the "ShiftSpan" value on the previous day is equal to 1 and "ShiftSpan_XID" are the same in both records.)
					AND ShiftSpanDate IS NOT NULL	
					AND (ShiftSpan_HoursDay1 > 0 AND ShiftSpan_HoursDay1 > Duration_Required)
			) b
		WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
			AND 
			(
				(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
				OR
				(a.DT = @startDate AND @startDate = @endDate)
				OR 
				(@startDate IS NULL AND @endDate IS NULL)
			)
		ORDER BY a.DT DESC, a.EmpNo

		--SELECT * FROM tas.OvertimeRemovalLogRecovery a
		--WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		--	AND 
		--	(
		--		(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
		--		OR
		--		(a.DT = @startDate AND @startDate = @endDate)
		--		OR 
		--		(@startDate IS NULL AND @endDate IS NULL)
		--	)
		--ORDER BY a.DT DESC 
	END 

	ELSE IF @actionType = 1		--Insert new records
	BEGIN

		--Start transaction
		BEGIN TRAN T1

		--Find records that should be undone
		INSERT INTO tas.OvertimeRemovalLogRecovery
		( 
			SourceTableName,
			EmpNo,
			DT,
			TS_AutoID,
			CostCenter,
			OTStartTime,
			OTEndTime,
			OTType,
			IsProcessed,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID,
			LastUpdateTime,
			LastUpdateEmpNo,
			LastUpdateUserID
		)
		SELECT DISTINCT 
			a.SourceTableName,
			a.EmpNo,
			a.DT,
			a.TS_AutoID,
			a.CostCenter,
			a.OTStartTime,
			a.OTEndTime,
			a.OTType,
			0 AS IsProcessed,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByUserID,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateUserID
		FROM tas.OvertimeRemovalLog a
			CROSS APPLY
			(
				SELECT * FROM tas.Tran_Timesheet
				WHERE EmpNo = a.EmpNo
					AND DT = a.DT
					AND ISNULL(ShiftSpan_XID, 0) > 0
					AND ShiftSpanDate IS NOT NULL	
			) b
		ORDER BY a.DT DESC, a.EmpNo

		--Get the inserted records
		SELECT * FROM tas.OvertimeRemovalLogRecovery

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE 
			ROLLBACK TRAN T1
	END 

	ELSE IF @actionType = 2		--Delete existing records
	BEGIN

		BEGIN TRAN T1

		DELETE FROM tas.OvertimeRemovalLogRecovery

		--Get the inserted records
		SELECT * FROM tas.OvertimeRemovalLogRecovery

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE 
			ROLLBACK TRAN T1
    END 

/*	Debugging:

	SELECT * FROM tas.OvertimeRemovalLog a
	ORDER BY a.DT DESC
	
	SELECT * FROM tas.OvertimeRemovalLogRecovery a
	ORDER BY a.DT DESC

	TRUNCATE TABLE tas.OvertimeRemovalLogRecovery

	--SELECT * INTO tas.System_Values_Temp FROM tas.System_Values

*/	

/*	Data change:

	BEGIN TRAN T1

	DELETE FROM tas.OvertimeRemovalLog
	WHERE LogID = 75

	COMMIT TRAN T1

*/