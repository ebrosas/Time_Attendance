/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.LongStayNotificationLog
*	Description: This table stores information about the employees who stayed working in the company longer than 16 hours
*
*	Date			Author		Rev.#		Comments
*	08/04/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.LongStayNotificationLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.LongStayNotificationLog
--END

	CREATE TABLE tas.LongStayNotificationLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(2) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_LongStayNotificationLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
