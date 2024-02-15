/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.ManhourHistory
*	Description: Modified the schema of "tas.ManhourHistory" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	02/08/2016		Ervin		1.0			Created
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'ManhourHistory' AND COLUMN_NAME = 'IsLTI')
	BEGIN

		ALTER TABLE tas.ManhourHistory 
		ADD IsLTI BIT NULL
	END

	
	

