/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.Master_EmployeeAttendance
*	Description: Modified the schema of "tas.Master_EmployeeAttendance" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	07/04/2022		Ervin		1.0			Added new field called "SwipeType"
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_EmployeeAttendance' AND COLUMN_NAME = 'SwipeType')
	BEGIN

		ALTER TABLE tas.Master_EmployeeAttendance 
		ADD SwipeType VARCHAR(10) NULL
	END

	
	
	

