/********************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeDistributionMember
*	Description: This table stores the action members of a particular distribution group
*
*	Date			Author		Rev.#		Comments
*	15/08/2017		Ervin		1.0			Created
*********************************************************************************************************************/

--IF OBJECT_ID ('tas.OvertimeDistributionMember') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.OvertimeDistributionMember
--END
	
	CREATE TABLE tas.OvertimeDistributionMember
	(
		AutoID int IDENTITY(1,1) NOT NULL,		
		OTRequestNo BIGINT NOT NULL,
		WorkflowTransactionID BIGINT NOT NULL,	
		EmpNo int NOT NULL,
		EmpName VARCHAR(100) NULL,
		EmpEmail VARCHAR(50) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT PK_OvertimeDistributionMember PRIMARY KEY CLUSTERED 
		(
			OTRequestNo,
			WorkflowTransactionID,
			EmpNo 
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
