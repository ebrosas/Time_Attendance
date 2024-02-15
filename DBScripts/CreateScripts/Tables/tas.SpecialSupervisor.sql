/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.SpecialSupervisor
*	Description: This table stores all special supervisors wherein their job title does not contain the word "Supervisor"
*
*	Date			Author		Rev.#		Comments
*	06/11/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.SpecialSupervisor') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.SpecialSupervisor
--END

	CREATE TABLE tas.SpecialSupervisor
	(
		SettingID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		IsEnabled BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_SpecialSupervisor] PRIMARY KEY CLUSTERED 
		(
			EmpNo
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
GO
