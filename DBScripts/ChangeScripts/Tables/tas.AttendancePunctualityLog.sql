/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.AttendancePunctualityLog
*	Description: Modified the schema of "tas.AttendancePunctualityLog" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	08/07/2019		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'AttendancePunctualityLog' AND COLUMN_NAME = 'PayGrade')
	BEGIN

		ALTER TABLE tas.AttendancePunctualityLog 
		ADD PayGrade INT NULL
	END

	
	
	

