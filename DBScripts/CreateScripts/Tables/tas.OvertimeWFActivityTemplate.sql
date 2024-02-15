/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeWFActivityTemplate
*	Description: This table stores the workflow activity template for the "Overtime Online Approval System"
*
*	Date			Author		Rev.#		Comments
*	26/07/2017		Ervin		1.0			Created
*****************************************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeWFActivityTemplate') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeWFActivityTemplate
END

	CREATE TABLE tas.OvertimeWFActivityTemplate
	(
		WorkflowTemplateID int IDENTITY(1,1) NOT NULL,
		WFModuleCode varchar(10) NOT NULL,
		ActivityCode varchar(20) NOT NULL,
		NextActivityCode varchar(20) NULL,
		ActivityDesc1 varchar(200) NOT NULL,
		ActivityDesc2 varchar(500) NULL,
		WFActivityTypeCode varchar(10) NOT NULL,
		SequenceNo int NOT NULL,
		SequenceType int NULL,						--(Note: 1 = Series; 2 = Parallel)
		ApprovalType int NULL,						--(Note: 1 = Primary Only; 2 = At Least One; 3 = All Approvers)
		ActionRole int NULL,						--(Note: 1 = Service Provider; 2 = Approver; 3 = Validator)
		IsActive bit NULL,
		IsFinalAct bit NULL,						--(Note: This is a flag that determines if activity is the last one)
		ActionMemberCode varchar(10) NULL,			--(Note: Refers to the Workflow Distribution List Code)
		ActionMemberType TINYINT NULL,				--(Note: 1 = Builtin Group; 2 = Individual Employee; 3 = Distribuition List)
		ServiceProviderTypeCode VARCHAR(10) NULL,	--(Note: 'SPAPPROVER' = Approver; 'SPVALIDTOR' = Validator; 'SPTECHSUP' = Service Provider; 'SPCONTRIBR' = Contributor)
		ParameterSourceTable varchar(50) NULL,
		ParameterName varchar(50) NULL,
		ParameterDataType varchar(50) NULL,
		ConditionCheckValue varchar(50) NULL,
		ConditionCheckDataType varchar(50) NULL,
		EmailSourceName varchar(30) NULL,
		EmailCCRecipient varchar(200) NULL,
		EmailCCRecipientType int NULL,				--(Note: 1 = Builtin Group; 2 = Individual Employee; 3 = Distribuition List)
		BypassIfAlreadyApproved BIT NULL,
		CreatedByUser varchar(50) NULL,
		CreatedDate datetime DEFAULT getdate(),		
		CreatedByUserEmpNo int NULL,
		CreatedByUserEmpName varchar(50) NULL,
		LastUpdateUser varchar(50) NULL,		
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateEmpName varchar(50) NULL
		
		CONSTRAINT [PK_OvertimeWFActivityTemplate] PRIMARY KEY CLUSTERED 
		(
			WFModuleCode,
			ActivityCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO

