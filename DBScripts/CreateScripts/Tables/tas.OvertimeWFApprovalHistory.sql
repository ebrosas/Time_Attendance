/********************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeWFApprovalHistory
*	Description: This table stores approval history for the "Overtime Online Approval System"
*
*	Date			Author		Rev.#		Comments
*	26/07/2017		Ervin		1.0			Created
*********************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeWFApprovalHistory') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeWFApprovalHistory
END
	
	CREATE TABLE tas.OvertimeWFApprovalHistory
	(
		AutoID int IDENTITY(1,1) NOT NULL,		
		OTRequestNo BIGINT NOT NULL,
		TS_AutoID int NOT NULL,
		RequestSubmissionDate datetime NOT NULL,
		AppApproved bit NOT NULL,
		AppRemarks varchar(300) NULL,
		AppRoutineSeq int NULL,
		AppCreatedBy int NOT NULL,
		AppCreatedName varchar(50) NULL,
		AppCreatedDate datetime NULL,
		AppModifiedBy int NULL,
		AppModifiedName varchar(50) NULL,
		AppModifiedDate datetime NULL,
		ApprovalRole VARCHAR(500) NULL,
		ActionRole INT NULL 
		
		CONSTRAINT PK_OvertimeWFApprovalHistory PRIMARY KEY CLUSTERED 
		(
			AutoID,
			OTRequestNo,
			TS_AutoID,
			RequestSubmissionDate 
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
