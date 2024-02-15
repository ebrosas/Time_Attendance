/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeRemovalLogRecovery
*	Description: This table stores information about the incorrect overtime records that were removed
*
*	Date			Author		Rev.#		Comments
*	27/07/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.OvertimeRemovalLogRecovery') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.OvertimeRemovalLogRecovery
--END

	CREATE TABLE tas.OvertimeRemovalLogRecovery
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		SourceTableName VARCHAR(50) NOT NULL,
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		TS_AutoID INT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		OTStartTime DATETIME NULL,
		OTEndTime DATETIME NULL,
		OTType VARCHAR(10) NULL,
		IsProcessed BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT PK_OvertimeRemovalLogRecovery PRIMARY KEY CLUSTERED 
		(
			SourceTableName,
			EmpNo,
			DT,
			TS_AutoID
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
