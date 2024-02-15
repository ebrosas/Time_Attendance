/***************************************************************************************************************************************************************
*	Revision History
*	Name: tas.WorkplaceSwipeExclusion
*	Description: Modified the schema of "tas.WorkplaceSwipeExclusion" table
*
*	Date:	  		Author:		Rev.#		Comments:
*	06/08/2022		Ervin		1.0			Added new field called "ReaderNoList"
**********************************************************************************************************************************************************/

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'WorkplaceSwipeExclusion' AND COLUMN_NAME = 'ReaderNoListList')
	BEGIN

		ALTER TABLE tas.WorkplaceSwipeExclusion 
		ADD ReaderNoList VARCHAR(100) NULL 
	END

	

	
	
	

