/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.AttendancePunctualityLog
*	Description: This table will store information about the employee's attendance punctuality
*
*	Date			Author		Rev.#		Comments
*	19/07/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.AttendancePunctualityLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.AttendancePunctualityLog
--END

	CREATE TABLE tas.AttendancePunctualityLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		PunctualityTypeID TINYINT NOT NULL,	--(Note: 1 = Late; 2 = Left Early; 3 = Late and Left Early; 4 = Work Extra Hours)
		CostCenter VARCHAR(12) NULL,
		TotalDuration INT NULL, 
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(10) NULL,
		Actual_ShiftCode VARCHAR(10) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_AttendancePunctualityLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			DT,
			PunctualityTypeID
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
