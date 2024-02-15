/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.EmployeeContractorMapping
*	Description: This table stores information about the Employee No. and Contractor No. mapping
*
*	Date			Author		Rev.#		Comments
*	18/11/2016		Ervin		1.0			Created
*****************************************************************************************************************************************/

--IF OBJECT_ID ('tas.EmployeeContractorMapping') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.EmployeeContractorMapping
--END

	CREATE TABLE tas.EmployeeContractorMapping
	(
		MappingID INT IDENTITY(1,1) NOT NULL,
		EmpNo INT NOT NULL,
		ContractorNo INT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		PrimaryIDNoType TINYINT NOT NULL DEFAULT 0,	--(Note: 0 = Use Employee No; 1 = Use Contractor No.)
		CreatedDate datetime DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUser varchar(50) NULL,
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateUser varchar(50) NULL
		
		CONSTRAINT [PK_EmployeeContractorMapping] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			ContractorNo
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
