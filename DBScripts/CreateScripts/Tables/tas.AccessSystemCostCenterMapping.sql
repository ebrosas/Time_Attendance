/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.AccessSystemCostCenterMapping
*	Description: This table stores information about the total man-hours of all employees and contractors within specific period
*
*	Date			Author		Rev.#		Comments
*	15/06/2016		Ervin		1.0			Created
*****************************************************************************************************************************************/

IF OBJECT_ID ('tas.AccessSystemCostCenterMapping') IS NOT NULL
BEGIN	

	DROP TABLE tas.AccessSystemCostCenterMapping
END

	CREATE TABLE tas.AccessSystemCostCenterMapping
	(
		AutoID INT IDENTITY(1,1) NOT NULL,
		CompanyID SMALLINT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		CreatedDate datetime DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUser varchar(50) NULL,
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateUser varchar(50) NULL
		
		CONSTRAINT [PK_AccessSystemCostCenterMapping] PRIMARY KEY CLUSTERED 
		(
			CompanyID, 
			CostCenter
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
