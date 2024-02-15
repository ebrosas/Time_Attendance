/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.WorkplaceReaderSetting
*	Description: Modified the schema of "tas.WorkplaceReaderSetting" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	08/04/2022		Ervin		1.0			Added new field called "IsSyncTimesheet"
*	17/04/2022		Ervin		1.1			Added new field called "EffectiveDate"
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'WorkplaceReaderSetting' AND COLUMN_NAME = 'IsSyncTimesheet')
	BEGIN

		ALTER TABLE tas.WorkplaceReaderSetting 
		ADD IsSyncTimesheet BIT DEFAULT 1
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'WorkplaceReaderSetting' AND COLUMN_NAME = 'EffectiveDate')
	BEGIN

		ALTER TABLE tas.WorkplaceReaderSetting 
		ADD EffectiveDate DATETIME DEFAULT GETDATE() 
		--DROP COLUMN EffectiveDate 
	END

/*	Set "IsSyncTimesheet" to 1 for all active workplace cost center 

	BEGIN TRAN T1

	UPDATE tas.WorkplaceReaderSetting 
	SET IsSyncTimesheet = 1
	WHERE IsActive = 1	

	ROLLBACK TRAN T1
	COMMIT TRAN T1
*/

/*	Set "EffectiveDate" equal to CreatedDate for all active workplace cost center 

	SELECT * FROM tas.WorkplaceReaderSetting a WITH (NOLOCK) 
	WHERE a.IsActive = 1	
	ORDER BY a.CostCenter

	BEGIN TRAN T1

	UPDATE tas.WorkplaceReaderSetting 
	SET EffectiveDate = CONVERT(DATETIME, CONVERT(VARCHAR, CreatedDate, 12))
	WHERE IsActive = 1	

	--Set Effective Date of ICT
	UPDATE tas.WorkplaceReaderSetting 
	SET EffectiveDate = '04/18/2022'
	WHERE IsActive = 1	
		AND RTRIM(CostCenter) = '7600'

	ROLLBACK TRAN T1
	COMMIT TRAN T1
*/

	
	
	

