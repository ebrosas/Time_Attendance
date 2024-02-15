/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.LicenseRegistry
*	Description: Modified the schema of "tas.LicenseRegistry" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	20/12/2022		Ervin		1.0			Added new field called "LicenseCategory"
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'LicenseRegistry' AND COLUMN_NAME = 'LicenseCategory')
	BEGIN

		ALTER TABLE tas.LicenseRegistry 
		ADD LicenseCategory VARCHAR(10) NULL DEFAULT 'LCATPERMNT' 
	END

	

	
	
	

