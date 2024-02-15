/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.AttendanceCorrection
*	Description: This table will store information any type of attendance corrections
*
*	Date			Author		Rev.#		Comments
*	11/02/2023		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.AttendanceCorrection') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.AttendanceCorrection
--END

	CREATE TABLE tas.AttendanceCorrection
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		AutoID INT NOT NULL,
		UpdateType TINYINT NOT NULL,			--(Notes: 1 = Correct NPH and OT removal not flag for payment)
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_AttendanceCorrection] PRIMARY KEY CLUSTERED 
		(
			EmpNo, DT, AutoID, UpdateType
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
