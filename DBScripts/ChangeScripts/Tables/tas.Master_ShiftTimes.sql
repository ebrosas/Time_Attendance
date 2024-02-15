/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.Master_ShiftTimes
*	Description: Modified the schema of "tas.Master_ShiftTimes" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	10/06/2018		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'CreatedByEmpNo')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD CreatedByEmpNo INT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'CreatedByEmpName')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD CreatedByEmpName VARCHAR(50) NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'CreatedByUser')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD CreatedByUser VARCHAR(50) NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'CreatedDate')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD CreatedDate DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'LastUpdateEmpNo')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD LastUpdateEmpNo INT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_ShiftTimes' AND COLUMN_NAME = 'LastUpdateEmpName')
	BEGIN

		ALTER TABLE tas.Master_ShiftTimes 
		ADD LastUpdateEmpName VARCHAR(50) NULL
	END

	
	

