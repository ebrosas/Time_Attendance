/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.DayOffUnpaidLeaveLog
*	Description: This table stores the attendance records wherein the dayoff is removed and marked as unpaid leave
*
*	Date			Author		Rev.#		Comments
*	29/04/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.DayOffUnpaidLeaveLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.DayOffUnpaidLeaveLog
--END

	CREATE TABLE tas.DayOffUnpaidLeaveLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		TSAutoID INT NOT NULL,
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(2) NULL,
		LeaveType VARCHAR(10) NULL,
		CorrectionCode VARCHAR(10) NULL,
		Processed BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_DayOffUnpaidLeaveLog] PRIMARY KEY CLUSTERED 
		(
			TSAutoID,
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
