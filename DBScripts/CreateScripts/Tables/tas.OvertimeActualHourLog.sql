/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeActualHourLog
*	Description: This table will store information about the employee's attendance punctuality
*
*	Date			Author		Rev.#		Comments
*	14/03/2018		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.OvertimeActualHourLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.OvertimeActualHourLog
--END

	CREATE TABLE tas.OvertimeActualHourLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		FiscalYear INT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		TotalActualOTHour FLOAT NULL, 
		LastUpdateTime DATETIME NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_OvertimeActualHourLog] PRIMARY KEY CLUSTERED 
		(
			FiscalYear,
			CostCenter
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
