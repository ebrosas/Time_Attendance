/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.FlexiTimeSetting
*	Description: Modified the schema of "tas.FlexiTimeSetting" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	19/04/2016		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'ArrivalFrom_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD ArrivalFrom_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'ArrivalTo_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD ArrivalTo_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'DepartFrom_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD DepartFrom_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'DepartTo_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD DepartTo_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'RArrivalFrom_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD RArrivalFrom_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'RArrivalTo_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD RArrivalTo_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'RDepartFrom_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD RDepartFrom_Old DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'FlexiTimeSetting' AND COLUMN_NAME = 'RDepartTo_Old')
	BEGIN

		ALTER TABLE tas.FlexiTimeSetting 
		ADD RDepartTo_Old DATETIME NULL
	END

	
	

