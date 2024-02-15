/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.UnisPostCostCenterMapping
*	Description: This table stores the cost center mapping of the data in "dbo.cOffice" table in UNIS system
*
*	Date			Author		Rev.#		Comments
*	09/12/2021		Ervin		1.0			Created
*****************************************************************************************************************************************/

IF OBJECT_ID ('tas.UnisPostCostCenterMapping') IS NOT NULL
BEGIN	

	DROP TABLE tas.UnisPostCostCenterMapping
END

	CREATE TABLE tas.UnisPostCostCenterMapping
	(
		AutoID INT IDENTITY(1,1) NOT NULL,
		CompanyCode VARCHAR(5) NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		PostCode VARCHAR(30) NOT NULL,
		CreatedDate datetime DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUser varchar(50) NULL,
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateUser varchar(50) NULL
		
		CONSTRAINT [PK_UnisPostCostCenterMapping] PRIMARY KEY CLUSTERED 
		(
			CompanyCode, 
			CostCenter,
			PostCode
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
