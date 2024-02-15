/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ShiftPatternRestriction
*	Description: This table stores the Shift Pattern security restriction settings
*
*	Date			Author		Rev.#		Comments
*	22/10/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.ShiftPatternRestriction') IS NOT NULL
BEGIN	

	DROP TABLE tas.ShiftPatternRestriction
END

	CREATE TABLE tas.ShiftPatternRestriction
	(
		SettingID INT IDENTITY(1,1) NOT NULL,	
		ShiftPatCode VARCHAR(2) NOT NULL,
		RestrictionType TINYINT NOT NULL,	--(Note: 0 => No restriction; 1 => Access restricted to specific employee; 2 => Access restricted to specific cost center; 3 => Access restricted to specific employee and cost center) 
		RestrictedEmpNoArray VARCHAR(200) NULL,
		RestrictedCostCenterArray VARCHAR(200) NULL,
		ErrorMessage VARCHAR(200) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_ShiftPatternRestriction] PRIMARY KEY CLUSTERED 
		(
			ShiftPatCode,
			RestrictionType
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
