	DECLARE	@actionType				TINYINT,
			@isCommitTrans			BIT,
			@startDate				DATETIME,
			@endDate				DATETIME,	
			@currentTotalHour		FLOAT,
			@previousTotalHour		FLOAT,
			@isProcessed			BIT 

	SELECT	@actionType			= 0,	--(Note: 0 = View record; 1 = Insert record; 2 = Update record; 3 = Delete record)
			@isCommitTrans		= 0,
			@startDate			= '06/16/2016',
			@endDate			= '07/15/2016',	
			@currentTotalHour	= 8910,
			@previousTotalHour	= 0,
			@isProcessed		= 0

	--Calculate @previousTotalHour
	IF (SELECT COUNT(*) FROM tas.ManhourHistory) > 0
	BEGIN
    
		SELECT TOP 1 @previousTotalHour = CurrentTotalHour + PreviousTotalHour
		FROM tas.ManhourHistory a
		ORDER BY LogID DESC
	END 

	IF @actionType = 0			--Check existing record
	BEGIN

		SELECT * FROM tas.ManhourHistory a
		ORDER BY a.LogID DESC 
	END
    
	ELSE IF @actionType = 1		--Insert new record
	BEGIN

		IF NOT EXISTS
        (
			SELECT LogID FROM tas.ManhourHistory a
			WHERE a.StartDate = @startDate
			AND a.EndDate = @endDate
		)
		BEGIN
        
			--Start a transaction
			BEGIN TRAN T1

			INSERT INTO tas.ManhourHistory
			(
				StartDate,
				EndDate,
				CurrentTotalHour,
				PreviousTotalHour,
				IsProcessed,
				CreatedDate,
				CreatedByEmpNo,
				CreatedByUser
			)
			SELECT	
				@startDate,
				@endDate,
				@currentTotalHour,
				@previousTotalHour,
				@isProcessed,
				GETDATE(),
				10003632,
				'GARMCO\ervin'

			--Check inserted record
			SELECT * FROM tas.ManhourHistory a
			WHERE a.StartDate = @startDate
				AND a.EndDate = @endDate

			IF @isCommitTrans = 1
				COMMIT TRAN T1
			ELSE
				ROLLBACK TRAN T1
		END
        ELSE
			SELECT 'Error: Record already exist!'
	END 

	ELSE IF @actionType = 2		--Update existing record
	BEGIN
    
		--Start a transaction
		BEGIN TRAN T1

		UPDATE tas.ManhourHistory
		SET CurrentTotalHour = @currentTotalHour,
			PreviousTotalHour = @previousTotalHour,
			IsProcessed = @isProcessed,
			LastUpdateTime = GETDATE(),
			LastUpdateEmpNo = 10003632,
			LastUpdateUser = 'GARMCO\ervin'
		WHERE StartDate = @startDate
			AND EndDate = @endDate

		--Check updated record
		SELECT * FROM tas.ManhourHistory a
		WHERE a.StartDate = @startDate
			AND a.EndDate = @endDate

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

	ELSE IF @actionType = 3		--Delete existing record
	BEGIN
    
		--Start a transaction
		BEGIN TRAN T1

		DELETE FROM tas.ManhourHistory
		WHERE StartDate = @startDate
			AND EndDate = @endDate

		--Check updated record
		SELECT * FROM tas.ManhourHistory a
		WHERE a.StartDate = @startDate
			AND a.EndDate = @endDate

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 



