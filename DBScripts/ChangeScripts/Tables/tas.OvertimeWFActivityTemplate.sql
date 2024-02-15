/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.OvertimeWFActivityTemplate
*	Description: Modify the schema of "tas.OvertimeWFActivityTemplate" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	27/08/2017		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'OvertimeWFActivityTemplate' AND COLUMN_NAME = 'BypassIfAlreadyApproved')
	BEGIN

		ALTER TABLE tas.OvertimeWFActivityTemplate 
		ADD BypassIfAlreadyApproved BIT NULL
	END

	

	

	

	

	
	

