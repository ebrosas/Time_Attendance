/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.DeathReasonOfAbsence
*	Description: This table stores data about the death-related timesheet correction
*
*	Date			Author		Rev.#		Comments
*	01/07/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.DeathReasonOfAbsence') IS NOT NULL
BEGIN	

	DROP TABLE tas.DeathReasonOfAbsence
END

	CREATE TABLE tas.DeathReasonOfAbsence
	(
		ReasonAbsenceID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		CorrectionCode VARCHAR(10) NOT NULL,
		ShiftPatCode VARCHAR(2) NULL,
		ShiftCode VARCHAR(10) NULL,		
		RelativeTypeCode VARCHAR(15) NULL,
		OtherRelativeType VARCHAR(200) NULL,
		Remarks VARCHAR(200) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByEmpName VARCHAR(150) NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateDate DATETIME DEFAULT GETDATE(),		
		LastUpdateEmpNo INT NULL,
		LastUpdateEmpName VARCHAR(150) NULL,
		LastUpdateUserID VARCHAR(50) NULL

		CONSTRAINT [PK_DeathReasonOfAbsence] PRIMARY KEY CLUSTERED 
		(
			EmpNo ASC,
			DT,
			CostCenter,
			CorrectionCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
