/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.InvalidTimesheet
*	Description: This table stores all incorrect attendance records due to issue in the swipe system
*
*	Date			Author		Rev.#		Comments
*	22/05/2018		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.InvalidTimesheet') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.InvalidTimesheet
--END

	CREATE TABLE tas.InvalidTimesheet
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		AutoID INT NOT NULL,
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(2) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		IsCorrected BIT NULL,
		CorrectionDate DATETIME NULL 

		CONSTRAINT [PK_InvalidTimesheet] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
