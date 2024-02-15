/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Master_ShiftTimes_Setting
*	Description: This table stores the default timing for each shift code
*
*	Date			Author		Rev.#		Comments
*	12/06/2018		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.Master_ShiftTimes_Setting') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.Master_ShiftTimes_Setting
--END

	CREATE TABLE tas.Master_ShiftTimes_Setting
	(
		SettingID INT IDENTITY(1,1) NOT NULL,	
		ShiftCode VARCHAR(10) NOT NULL,
		ArrivalFrom DATETIME NULL,
		ArrivalTo DATETIME NULL,
		DepartFrom DATETIME NULL,
		DepartTo DATETIME NULL,
		RArrivalFrom DATETIME NULL,
		RArrivalTo DATETIME NULL,
		RDepartFrom DATETIME NULL,
		RDepartTo DATETIME NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByEmpName VARCHAR(50) NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateDate DATETIME DEFAULT GETDATE(),		
		LastUpdateEmpNo INT NULL,
		LastUpdateEmpName VARCHAR(50) NULL,
		LastUpdateUserID VARCHAR(50) NULL

		CONSTRAINT [PK_Master_ShiftTimes_Setting] PRIMARY KEY CLUSTERED 
		(
			ShiftCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
