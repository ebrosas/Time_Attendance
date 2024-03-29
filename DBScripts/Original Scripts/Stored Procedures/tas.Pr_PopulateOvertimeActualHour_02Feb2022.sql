USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_PopulateOvertimeActualHour]    Script Date: 02/02/2022 10:56:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_PopulateOvertimeActualHour
*	Description: This stored procedure is used to populate the overtime actual hours based on fiscal year
*
*	Date:			Author:		Rev.#:		Comments:
*	14/03/2018		Ervin		1.0			Created
*	27/06/2021		Ervin		1.1			Added code to populate contractor's swipe data
**************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_PopulateOvertimeActualHour]
(
	@fiscalYear		INT,
	@costCenter		VARCHAR(12) = ''
)
AS	
BEGIN
	
	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT		

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0
		
	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	BEGIN TRY

		IF NOT EXISTS
		(
			SELECT * FROM tas.OvertimeActualHourLog a WITH (NOLOCK)
			WHERE a.FiscalYear = @fiscalYear
				AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		)
		BEGIN

			INSERT INTO tas.OvertimeActualHourLog
			SELECT	(a.GBCTRY * 100) + a.GBFY AS FiscalYear,
					LTRIM(RTRIM(a.GBMCU)) AS CostCenter,
					b.TotalActualOTHour,
					GETDATE() AS LastUpdateTime,
					'System Admin' AS LastUpdateUserID
			FROM tas.sy_F0902 a WITH (NOLOCK)
				CROSS APPLY tas.fnGetOTActualHourByCostCenter(@fiscalYear, LTRIM(RTRIM(a.GBMCU))) b
			WHERE 
				LTRIM(RTRIM(a.GBOBJ)) = '533150'		--Object Account
				AND LTRIM(RTRIM(a.GBSUB)) = '1100'		--Subledger
				AND LTRIM(RTRIM(a.GBLT)) = 'BU'			--Ledger Type
				AND (a.GBCTRY * 100) + a.GBFY = @fiscalYear
				AND (LTRIM(RTRIM(a.GBMCU)) = @costCenter OR @costCenter IS NULL)

			--Get the number of affected records 
			SELECT @rowsAffected = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END
		END 

		ELSE
		BEGIN

			UPDATE tas.OvertimeActualHourLog
			SET TotalActualOTHour = tas.fnCalculateOTActualHourByCostCenter(FiscalYear, RTRIM(CostCenter)),
				LastUpdateTime = GETDATE()
			WHERE FiscalYear = @fiscalYear
				AND (RTRIM(CostCenter) = @costCenter OR @costCenter IS NULL)

			--Get the number of affected records 
			SELECT @rowsAffected = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END
		END 

		/************************************************************************************************************************************************************************
			Rev. #1.1 - Populate contractor swipe data from yesterday until current day
		*************************************************************************************************************************************************************************/
		IF EXISTS
        (
			SELECT 1 FROM tas.ContractorSwipeLog a WITH (NOLOCK)
			WHERE a.SwipeDate BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR(8), DATEADD(DAY, -1, GETDATE()), 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR(8), GETDATE(), 12))
		)
		BEGIN
        
			--Delete existing records
			DELETE FROM tas.ContractorSwipeLog 
			WHERE SwipeDate BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR(8), DATEADD(DAY, -1, GETDATE()), 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR(8), GETDATE(), 12))
		END

		INSERT INTO tas.ContractorSwipeLog
        (
			EmpNo,
			SwipeDate,
			SwipeTime,
			LocationName,
			ReaderName,
			SwipeType,
			LocationCode,
			ReaderNo,
			ContractorName,
			EventCode,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
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
				a.[Event],
				GETDATE() AS CreatedDate,
				0 AS CreatedByEmpNo,
				'System Admin' AS CreatedByUserID
		 FROM tas.Vw_ContractorSwipe a WITH (NOLOCK)
		 WHERE a.SwipeDate BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR(8), DATEADD(DAY, -1, GETDATE()), 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR(8), GETDATE(), 12))
		 /********************************************************************************* END ***************************************************************************************/

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected

END

/*	Debug:

PARAMETERS:
	@fiscalYear		INT,
	@costCenter		VARCHAR(12)

	EXEC tas.Pr_PopulateOvertimeActualHour 2021, ''

	SELECT * FROM tas.OvertimeActualHourLog a
	
	SELECT * FROM tas.ContractorSwipeLog a WITH (NOLOCK)
	ORDER BY a.SwipeDate DESC, a.EmpNo, a.SwipeTime 

*/
