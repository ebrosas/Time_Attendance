/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeShiftSpan
*	Description: This table stores information about the Timesheet record that has been corrected due to incorrect calculation of overtime that involve shift span days
*
*	Date			Author		Rev.#		Comments
*	10/08/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.OvertimeShiftSpan') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.OvertimeShiftSpan
--END

	CREATE TABLE tas.OvertimeShiftSpan
	(
		TransactionID INT IDENTITY(1,1) NOT NULL,	
		LogID BIGINT NOT NULL,
		TableNameToUpdate VARCHAR(100) NOT NULL,
		TSAutoIDSource INT NOT NULL,
		TSAutoIDTarget INT NOT NULL,
		OTStartTime DATETIME NULL,
		OTEndTime DATETIME NULL,
		OTType VARCHAR(10) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT PK_OvertimeShiftSpan PRIMARY KEY CLUSTERED 
		(
			LogID,
			TableNameToUpdate,
			TSAutoIDSource,
			TSAutoIDTarget
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
