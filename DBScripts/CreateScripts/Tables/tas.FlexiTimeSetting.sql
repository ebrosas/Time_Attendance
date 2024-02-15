/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.FlexiTimeSetting
*	Description: This table stores information about the Flexi-timing for each Shift Pattern Code   
*
*	Date			Author		Rev.#		Comments
*	03/04/2016		Ervin		1.0			Created
*****************************************************************************************************************************************/

IF OBJECT_ID ('tas.FlexiTimeSetting') IS NOT NULL
BEGIN	

	DROP TABLE tas.FlexiTimeSetting
END

	CREATE TABLE tas.FlexiTimeSetting
	(
		SettingID bigint IDENTITY(1,1) NOT NULL,
		ShiftPatCode varchar(2) NOT NULL,
		ShiftCode varchar(10) NOT NULL,
		NormalArrivalFrom time NOT NULL,
		NormalArrivalTo time NOT NULL,
		RamadanArrivalFrom time NULL,
		RamadanArrivalTo time NULL,
		IsActive bit NULL,
		CreatedDate datetime DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUser varchar(50) NULL,
		CreatedByEmpName varchar(100) NULL,
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateUser varchar(50) NULL,
		LastUpdateEmpName varchar(100) NULL
		
		CONSTRAINT [PK_FlexiTimeSetting] PRIMARY KEY CLUSTERED 
		(
			ShiftPatCode, 
			ShiftCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
