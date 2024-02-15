/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.FamilyRelativeSetting
*	Description: This table stores the various type of family relatives
*
*	Date			Author		Rev.#		Comments
*	30/06/2019		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.FamilyRelativeSetting') IS NOT NULL
BEGIN	

	DROP TABLE tas.FamilyRelativeSetting
END

	CREATE TABLE tas.FamilyRelativeSetting
	(
		SettingID INT IDENTITY(1,1) NOT NULL,	
		DegreeLevel TINYINT NOT NULL,
		RelativeTypeCode VARCHAR(15) NOT NULL,
		RelativeTypeName VARCHAR(300) NOT NULL,
		SequenceNo TINYINT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateDate DATETIME DEFAULT GETDATE(),		
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL

		CONSTRAINT [PK_FamilyRelativeSetting] PRIMARY KEY CLUSTERED 
		(
			DegreeLevel,
			RelativeTypeCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO
