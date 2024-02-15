/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.DayOffAbsentLog
*	Description: This table stores all incorrect attendance records wherein the dayoff were flagged as absent
*
*	Date			Author		Rev.#		Comments
*	25/03/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.DayOffAbsentLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.DayOffAbsentLog
--END

	CREATE TABLE tas.DayOffAbsentLog
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
		Processed BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_DayOffAbsentLog] PRIMARY KEY CLUSTERED 
		(
			TSAutoID,
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
