/********************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeWFRoutineHistory
*	Description: This table stores workflow transaction history of the "Overtime Online Approval System"
*
*	Date			Author		Rev.#		Comments
*	26/07/2017		Ervin		1.0			Created
*********************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeWFRoutineHistory') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeWFRoutineHistory
END
	
	CREATE TABLE tas.OvertimeWFRoutineHistory
	(
		AutoID int IDENTITY(1,1) NOT NULL,	
		OTRequestNo BIGINT NOT NULL,	
		TS_AutoID int NOT NULL,
		RequestSubmissionDate datetime NOT NULL,
		HistDesc varchar(300) NULL,
		HistCreatedBy int NOT NULL,
		HistCreatedName varchar(50) NULL,
		HistCreatedDate datetime NULL
		
		CONSTRAINT [PK_OvertimeWFRoutineHistory] PRIMARY KEY CLUSTERED 
		(
			AutoID,		
			OTRequestNo,	
			TS_AutoID,
			RequestSubmissionDate 
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
