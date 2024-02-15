/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ROAAbsentLog
*	Description: This table stores the attendance records which were marked as absent by removing the ROA
*
*	Date			Author		Rev.#		Comments
*	13/05/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.ROAAbsentLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.ROAAbsentLog
--END

	CREATE TABLE tas.ROAAbsentLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		TSAutoID INT NOT NULL,
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(2) NULL,
		CorrectionCode VARCHAR(10) NULL,
		RemarkCode VARCHAR(10) NULL,
		LeaveType VARCHAR(10) NULL,
		AbsenceReasonCode VARCHAR(10) NULL,
		AbsenceReasonColumn VARCHAR(10) NULL,
		NoPayHours INT NULL,
		Processed BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_ROAAbsentLog] PRIMARY KEY CLUSTERED 
		(
			TSAutoID,
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
