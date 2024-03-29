USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_ManhourHistory_CRUD]    Script Date: 11/04/2017 14:48:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Tran_WorkplaceSwipe_CRUD
*	Description: Performs insert, update, and delete operations in "tas.ManhourHistory" table
*
*	Date:			Author:		Rev.#:		Comments:
*	16/06/2016		Ervin		1.0			Created
*	02/08/2016		Ervin		1.1			Refactored the code to check for LTI record when calculating the total man-hour
*	05/03/2017		Ervin		1.2			Added join to "ServiceRequest" table to check if requisition is not rejected or cancelled
************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_ManhourHistory_CRUD]
(
	@actionType				TINYINT,	
	@startDate				DATETIME,
	@endDate				DATETIME,	
	@processDate			DATETIME,	
	@userEmpNo				INT,
	@userID					VARCHAR(50)
)
AS	
	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Define variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@newID					INT,
			@rowsAffected			INT,
			@currentTotalHour		FLOAT,
			@previousTotalHour		FLOAT,
			@mirReportDate			DATETIME

	--Initialize variables
	SELECT	@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= '',
			@newID				= 0,
			@rowsAffected		= 0,
			@currentTotalHour	= 0,
			@previousTotalHour	= 0,
			@mirReportDate		= NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF @actionType = 1
	BEGIN

		--Check for LTI records 
		IF EXISTS
        (
			SELECT a.LogID FROM tas.ManhourHistory a
			WHERE a.StartDate >= @startDate
				AND a.EndDate <= @endDate
				AND a.IsLTI = 1
		)
		BEGIN

			--Change action type from Insert to Update operation
			SET @actionType = 2
		END 
    END 

	IF @actionType = 0			--Check existing record
	BEGIN

		SELECT * FROM tas.ManhourHistory a
		WHERE 
		(
			(a.StartDate = @startDate AND a.EndDate = @endDate)
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
		ORDER BY a.LogID DESC 
	END

	ELSE IF @actionType = 1		--Insert new record
	BEGIN
		
		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

			--Check for LTI record
			IF EXISTS
            (
				SELECT a.MIRRequestNo 
				FROM tas.sy_MIRRequest a
					INNER JOIN tas.sy_ServiceRequest b ON a.ServiceRequestNo = b.ServiceRequestNo
				WHERE 
					a.IsLTI = 1
					AND a.DateReported = @processDate
					AND RTRIM(b.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')
			)
			BEGIN

				--Set the current and previous count to zero
				SELECT	@currentTotalHour = 0,
						@previousTotalHour = 0

				--Get the MIR Report Date
				SELECT @mirReportDate = a.DateReported
				FROM tas.sy_MIRRequest a
					INNER JOIN tas.sy_ServiceRequest b ON a.ServiceRequestNo = b.ServiceRequestNo
				WHERE 
					a.IsLTI = 1
					AND a.DateReported = @processDate
					AND RTRIM(b.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')

				--Insert Man-hour record with LTI
				INSERT INTO tas.ManhourHistory
				(
					StartDate,
					EndDate,
					CurrentTotalHour,
					PreviousTotalHour,
					IsProcessed,
					CreatedDate,
					CreatedByEmpNo,
					CreatedByUser,
					IsLTI
				)
				VALUES
				(
					@mirReportDate,
					@mirReportDate,
					@currentTotalHour,
					@previousTotalHour,
					0,
					GETDATE(),
					@userEmpNo,
					@userID,
					1
				)

				--Get the new identity ID
				SELECT	@newID = @@identity

				--Set the End Date of the current pay period to the MIR Report Date minus 1 day
				--Recalculate the value of CurrentTotalHour based on the new date range
				UPDATE tas.ManhourHistory
				SET EndDate = DATEADD(DAY, -1, @mirReportDate),
					CurrentTotalHour =  tas.fnGetTotalManhour(StartDate, DATEADD(DAY, -1, @mirReportDate))
				WHERE StartDate = @startDate
					AND EndDate = @endDate
		
				--Get the number of affected rows
				SELECT @rowsAffected = @@rowcount
            END
            
			ELSE
            BEGIN
            
				--Get the total man-hour by period
				SELECT @currentTotalHour = tas.fnGetTotalManhour(@startDate, @endDate)

				--Calculate @previousTotalHour
				IF (SELECT COUNT(*) FROM tas.ManhourHistory) > 0
				BEGIN
    
					SELECT TOP 1 @previousTotalHour = CurrentTotalHour + PreviousTotalHour
					FROM tas.ManhourHistory a
					ORDER BY LogID DESC
				END 

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
				VALUES
				(
					@startDate,
					@endDate,
					@currentTotalHour,
					@previousTotalHour,
					0,
					GETDATE(),
					@userEmpNo,
					@userID
				)
		
				--Get the new identity ID
				SELECT	@newID = @@identity,
						@rowsAffected = 1
			END 
		END TRY

		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
			COMMIT TRANSACTION T1		
		ELSE
			ROLLBACK TRANSACTION T1

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@newID AS NewIdentityID,
				1 AS RowsAffected,
				@currentTotalHour AS TotalHourByPeriod,
				@currentTotalHour + @previousTotalHour AS GrandTotalHour
	END

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

			--Check for LTI record
			IF EXISTS
            (
				SELECT a.MIRRequestNo 
				FROM tas.sy_MIRRequest a
					INNER JOIN tas.sy_ServiceRequest b ON a.ServiceRequestNo = b.ServiceRequestNo
				WHERE 
					a.IsLTI = 1
					AND a.DateReported = @processDate
					AND RTRIM(b.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')
			)
			BEGIN

				IF NOT EXISTS
                (
					SELECT a.LogID FROM tas.ManhourHistory a
					WHERE a.StartDate = @processDate
						AND a.EndDate = @processDate
						AND a.IsLTI = 1
				)
				BEGIN
                
					--Set the current and previous count to zero
					SELECT	@currentTotalHour = 0,
							@previousTotalHour = 0

					--Get the MIR Report Date
					SELECT @mirReportDate = a.DateReported
					FROM tas.sy_MIRRequest a
						INNER JOIN tas.sy_ServiceRequest b ON a.ServiceRequestNo = b.ServiceRequestNo
					WHERE 
						a.IsLTI = 1
						AND a.DateReported = @processDate
						AND RTRIM(b.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')

					--Insert Man-hour record with LTI
					INSERT INTO tas.ManhourHistory
					(
						StartDate,
						EndDate,
						CurrentTotalHour,
						PreviousTotalHour,
						IsProcessed,
						CreatedDate,
						CreatedByEmpNo,
						CreatedByUser,
						IsLTI
					)
					VALUES
					(
						@mirReportDate,
						@mirReportDate,
						@currentTotalHour,
						@previousTotalHour,
						0,
						GETDATE(),
						@userEmpNo,
						@userID,
						1
					)

					--Get the new identity ID
					SELECT	@newID = @@identity

					--Set the End Date of the current pay period to the MIR Report Date minus 1 day
					--Recalculate the value of CurrentTotalHour based on the new date range
					UPDATE tas.ManhourHistory
					SET EndDate = DATEADD(DAY, -1, @mirReportDate),
						CurrentTotalHour =  tas.fnGetTotalManhour(StartDate, DATEADD(DAY, -1, @mirReportDate))
					WHERE StartDate = @startDate
						AND EndDate = @endDate
		
					--Get the number of affected rows
					SELECT @rowsAffected = @@rowcount
				END 
            END

			ELSE 
			BEGIN

				--Check if the last Man-hour record is an LTI
				DECLARE @isLTI BIT
				SELECT TOP 1  @isLTI = a.IsLTI
				FROM tas.ManhourHistory a
				ORDER BY a.LogID DESC	 

				IF @isLTI = 1
				BEGIN

					--Get the MIR Report Date
					SELECT TOP 1 @mirReportDate = a.StartDate
					FROM tas.ManhourHistory a
					ORDER BY a.LogID DESC	 

					--Check if current process date is greater than MIR Report Date
					IF @processDate > @mirReportDate
					BEGIN

						--Check if no record exist after the LTI
						IF NOT EXISTS
						(
							SELECT a.LogID FROM tas.ManhourHistory a
							WHERE a.StartDate = DATEADD(DAY, 1, @mirReportDate)
								AND a.EndDate = @endDate
						)
						BEGIN

							--Insert Man-hour record - continuation of count after LTI
							INSERT INTO tas.ManhourHistory
							(
								StartDate,
								EndDate,
								CurrentTotalHour,
								PreviousTotalHour,
								IsProcessed,
								CreatedDate,
								CreatedByEmpNo,
								CreatedByUser,
								IsLTI
							)
							VALUES
							(
								DATEADD(DAY, 1, @mirReportDate),	--Note: MIR report date plus 1 day
								@endDate,
								tas.fnGetTotalManhour(DATEADD(DAY, 1, @mirReportDate), @endDate),
								0,
								0,
								GETDATE(),
								@userEmpNo,
								@userID,
								NULL 
							)
		
							--Get the new identity ID
							SELECT	@newID = @@identity,
									@rowsAffected = 1
                        END 

						ELSE
                        BEGIN

							--Calculate @previousTotalHour
							SELECT TOP 1 @previousTotalHour = CurrentTotalHour + PreviousTotalHour
							FROM tas.ManhourHistory a
							WHERE a.IsLTI = 1
							ORDER BY a.LogID DESC	 

							UPDATE tas.ManhourHistory
							SET CurrentTotalHour = tas.fnGetTotalManhour(DATEADD(DAY, 1, @mirReportDate), @endDate),
								PreviousTotalHour = @previousTotalHour,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @userEmpNo,
								LastUpdateUser = @userID
							WHERE StartDate = DATEADD(DAY, 1, @mirReportDate)
								AND EndDate = @endDate

							SELECT @rowsAffected = @@rowcount 
                        END 
                    END 
                END 

				ELSE
                BEGIN
                
					--Check for LTI records 
					IF EXISTS
					(
						SELECT TOP 1 a.LogID 
						FROM tas.ManhourHistory a
						WHERE a.StartDate >= @startDate
							AND a.EndDate <= @endDate
							AND a.IsLTI = 1
						ORDER BY LogID DESC
					)
					BEGIN
                    
						DECLARE	@sDate	DATETIME,
								@eDate	DATETIME

						SELECT TOP 1	
							@sDate = a.StartDate,
							@eDate = a.EndDate
						FROM tas.ManhourHistory a
						WHERE a.LogID > 
						(
							SELECT TOP 1 LogID 
							FROM tas.ManhourHistory 
							WHERE StartDate >= @startDate
								AND EndDate <= @endDate
								AND IsLTI = 1
							ORDER BY LogID DESC
						)
						ORDER BY LogID DESC

						IF @sDate IS NOT NULL AND @eDate IS NOT NULL
						BEGIN
                       
							--Get the total man-hour by period
							SELECT @currentTotalHour = tas.fnGetTotalManhour(@sDate, @eDate)

							--Calculate @previousTotalHour
							SELECT TOP 1 @previousTotalHour = CurrentTotalHour + PreviousTotalHour
							FROM tas.ManhourHistory a
							WHERE a.LogID < (SELECT LogID FROM tas.ManhourHistory WHERE StartDate = @startDate AND EndDate = @endDate)
							ORDER BY a.LogID DESC	 

							UPDATE tas.ManhourHistory
							SET CurrentTotalHour = @currentTotalHour,
								PreviousTotalHour = @previousTotalHour,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @userEmpNo,
								LastUpdateUser = @userID
							WHERE StartDate = @sDate
								AND EndDate = @eDate

							SELECT @rowsAffected = @@rowcount 
						END 
					END 

					ELSE
                    BEGIN

						--Get the total man-hour by period
						SELECT @currentTotalHour = tas.fnGetTotalManhour(@startDate, @endDate)

						--Calculate @previousTotalHour
						SELECT TOP 1 @previousTotalHour = CurrentTotalHour + PreviousTotalHour
						FROM tas.ManhourHistory a
						WHERE a.LogID < (SELECT LogID FROM tas.ManhourHistory WHERE StartDate = @startDate AND EndDate = @endDate)
						ORDER BY a.LogID DESC	 

						UPDATE tas.ManhourHistory
						SET CurrentTotalHour = @currentTotalHour,
							PreviousTotalHour = @previousTotalHour,
							LastUpdateTime = GETDATE(),
							LastUpdateEmpNo = @userEmpNo,
							LastUpdateUser = @userID
						WHERE StartDate = @startDate
							AND EndDate = @endDate

						SELECT @rowsAffected = @@rowcount 
                    END 
				END 
			END 
		END TRY

		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
			COMMIT TRANSACTION T1		
		ELSE
			ROLLBACK TRANSACTION T1

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription, 
				@newID AS NewIdentityID,
				@rowsAffected AS RowsAffected,
				@currentTotalHour AS TotalHourByPeriod,
				@currentTotalHour + @previousTotalHour AS GrandTotalHour
	END

	ELSE IF (@actionType = 3)  --Delete existing record 
	BEGIN

		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

			DELETE FROM tas.ManhourHistory
			WHERE StartDate = @startDate
				AND EndDate = @endDate

			SELECT @rowsAffected = @@rowcount

		END TRY

		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
			COMMIT TRANSACTION T1		
		ELSE
			ROLLBACK TRANSACTION T1

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription, 
				0 AS NewIdentityID,
				@rowsAffected AS RowsAffected,
				0 AS TotalHourByPeriod,
				0 AS GrandTotalHour
	END

