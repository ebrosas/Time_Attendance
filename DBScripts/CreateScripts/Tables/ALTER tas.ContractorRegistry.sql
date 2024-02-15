/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.ContractorRegistry
*	Description: Modified the schema of "tas.ContractorRegistry" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	18/11/2021		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'ContractorRegistry' AND COLUMN_NAME = 'WorkDurationHours')
	BEGIN

		ALTER TABLE tas.ContractorRegistry 
		ADD WorkDurationHours INT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'ContractorRegistry' AND COLUMN_NAME = 'WorkDurationMins')
	BEGIN

		ALTER TABLE tas.ContractorRegistry 
		ADD WorkDurationMins INT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'ContractorRegistry' AND COLUMN_NAME = 'CompanyContactNo')
	BEGIN

		ALTER TABLE tas.ContractorRegistry 
		ADD CompanyContactNo VARCHAR(30) NULL
	END

	
	
	

