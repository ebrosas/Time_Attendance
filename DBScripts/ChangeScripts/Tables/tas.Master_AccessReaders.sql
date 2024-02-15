/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.Master_AccessReaders
*	Description: Modified the schema of "tas.Master_AccessReaders" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	11/11/2020		Ervin		1.0			Added new field called ""
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Master_AccessReaders' AND COLUMN_NAME = 'SourceID')
	BEGIN

		ALTER TABLE tas.Master_AccessReaders 
		ADD SourceID TINYINT NULL
	END

	
	
	

