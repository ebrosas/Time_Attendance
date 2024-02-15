/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.Master_EmployeeAdditional
*	Description: Modify the schema of "tas.Master_EmployeeAdditional" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	28/12/2017		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_EmployeeAdditional' AND COLUMN_NAME = 'CatgEffectiveDate')
	BEGIN

		ALTER TABLE tas.Master_EmployeeAdditional 
		ADD CatgEffectiveDate DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_EmployeeAdditional' AND COLUMN_NAME = 'CatgEndingDate')
	BEGIN

		ALTER TABLE tas.Master_EmployeeAdditional 
		ADD CatgEndingDate DATETIME NULL
	END

	
	

	

	

	
	

