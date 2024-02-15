/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeWFCostCenterMapping
*	Description: This table stores the mapping information between specific cost center and the workflow template to use
*
*	Date			Author		Rev.#		Comments
*	31/07/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeWFCostCenterMapping') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeWFCostCenterMapping
END

	CREATE TABLE tas.OvertimeWFCostCenterMapping
	(
		MappingID INT IDENTITY(1,1) NOT NULL,	
		GroupCode VARCHAR(3) NOT NULL,
		CostCenter VARCHAR(12) NULL,
		WFModuleCode VARCHAR(10) NOT NULL,
		IsWFByCostCenter BIT NULL,
		IsActive BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT PK_OvertimeWFCostCenterMapping PRIMARY KEY CLUSTERED 
		(
			WFModuleCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
