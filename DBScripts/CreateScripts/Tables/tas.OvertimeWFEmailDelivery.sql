/********************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeWFEmailDelivery
*	Description: This table stores email communication records for the "Overtime Online Approval System"
*
*	Date			Author		Rev.#		Comments
*	26/07/2017		Ervin		1.0			Created
*********************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeWFEmailDelivery') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeWFEmailDelivery
END
	
	CREATE TABLE tas.OvertimeWFEmailDelivery
	(
		DeliveryID int IDENTITY(1,1) NOT NULL,	
		OTRequestNo BIGINT NOT NULL,	
		TS_AutoID int NOT NULL,
		RequestSubmissionDate datetime NOT NULL,
		CurrentlyAssignedEmpNo int NOT NULL,
		CurrentlyAssignedEmpName varchar(50) NULL,
		CurrentlyAssignedEmpEmail varchar(50) NULL,
		ActivityCode varchar(20) NOT NULL,
		ActionMemberCode varchar(10) NULL,
		EmailSourceName varchar(30) NULL,
		EmailCCRecipient varchar(200) NULL,
		EmailCCRecipientType int NULL,
		IsDelivered bit NOT NULL,
		CreatedByEmpNo int NOT NULL,
		CreatedByEmpName varchar(50) NULL,
		CreatedDate datetime NULL,		
		LastUpdateEmpNo int NULL,
		LastUpdateEmpName varchar(50) NULL,
		LastUpdateTime datetime NULL
		
		CONSTRAINT [PK_OvertimeWFEmailDelivery] PRIMARY KEY CLUSTERED 
		(			
			OTRequestNo,
			TS_AutoID,
			ActivityCode,
			RequestSubmissionDate,
			CurrentlyAssignedEmpNo
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
