/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.OvertimeRequest
*	Description: Modify the schema of "tas.OvertimeRequest" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	15/08/2017		Ervin		1.0			Created
*	21/01/2018		Ervin		1.1			Added "IsHold" field
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'OTApproved')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD OTApproved VARCHAR(1) NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'OTReason')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD OTReason VARCHAR(10) NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'OTComment')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD OTComment VARCHAR(1000) NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'IsModifiedByHR')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD IsModifiedByHR BIT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'IsOTCommentModified')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD IsOTCommentModified BIT NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeRequest' AND COLUMN_NAME = 'IsHold')
	BEGIN

		ALTER TABLE tas.OvertimeRequest 
		ADD IsHold BIT NULL
	END

	

	

	

	
	

