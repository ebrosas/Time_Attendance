DECLARE	@actionType			TINYINT = 0,	--(Notes: 0 = Check records, 1 = Insert records)
		@isCommitTrans		BIT = 0

	IF @actionType = 0
	BEGIN
    
		SELECT * FROM tas.UnisPostCostCenterMapping a WITH (NOLOCK)
		ORDER BY a.CostCenter
	END 
	
	ELSE IF @actionType = 1
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.UnisPostCostCenterMapping
		(
			CompanyCode,
			CostCenter,
			PostCode,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUser
		)

		SELECT DISTINCT 
			a.CompanyCode AS CompanyID,
			a.BusinessUnit AS CostCenter,
			CASE WHEN RTRIM(a.BusinessUnit) IN ('2110', '2111', '2112') THEN '0031'	--Remelt Melting & Casting
				WHEN RTRIM(a.BusinessUnit) IN ('3240') THEN '0028'		--Hot Mill
				WHEN RTRIM(a.BusinessUnit) IN ('3250') THEN '0032'		--Roll Grinding
				WHEN RTRIM(a.BusinessUnit) IN ('3220', '3410', '3412', '3415', '3420', '3422', '3430') THEN '0035'	--Packing
				WHEN RTRIM(a.BusinessUnit) IN ('3470') THEN '0038'		--Packing (Carpentry)
				WHEN RTRIM(a.BusinessUnit) IN ('3320') THEN '0023'		--Cold Mill
				WHEN RTRIM(a.BusinessUnit) IN ('3322') THEN '0024'		--Cold Mill 2
				WHEN RTRIM(a.BusinessUnit) IN ('3880', '3881', '3882', '3883', '3884', '3885') THEN '0006'	--Foil Mill
				WHEN RTRIM(a.BusinessUnit) IN ('3800', '3890') THEN '0022'		--General Factory
				WHEN RTRIM(a.BusinessUnit) IN ('3900') THEN '0010'		--PPC
				WHEN RTRIM(a.BusinessUnit) IN ('3950') THEN '0040'		--Shipping
				WHEN RTRIM(a.BusinessUnit) IN ('4100') THEN '0015'		--Technical
				WHEN RTRIM(a.BusinessUnit) IN ('4200') THEN '0029'		--Laboratory
				WHEN RTRIM(a.BusinessUnit) IN ('5200') THEN '0018'		--Mechanical
				WHEN RTRIM(a.BusinessUnit) IN ('5300') THEN '0017'		--Electrical
				WHEN RTRIM(a.BusinessUnit) IN ('5400') THEN '0020'		--Central Engineering
				WHEN RTRIM(a.BusinessUnit) IN ('6100', '6200') THEN '0008'		--Marketing
				WHEN RTRIM(a.BusinessUnit) IN ('7100') THEN '0005'		--Corporate Communications
				WHEN RTRIM(a.BusinessUnit) IN ('7150') THEN '0036'		--Administration Services
				WHEN RTRIM(a.BusinessUnit) IN ('7200') THEN '0013'		--Safety
				WHEN RTRIM(a.BusinessUnit) IN ('7250') THEN '0009'		--Medical
				WHEN RTRIM(a.BusinessUnit) IN ('7300') THEN '0039'		--Stores
				WHEN RTRIM(a.BusinessUnit) IN ('7400') THEN '0012'		--Purchasing
				WHEN RTRIM(a.BusinessUnit) IN ('7500') THEN '0007'					--HR
				WHEN RTRIM(a.BusinessUnit) IN ('7550', '7560', '7920') THEN '0016'	--Training
				WHEN RTRIM(a.BusinessUnit) IN ('7575') THEN '0034'				--Trade Union
				WHEN RTRIM(a.BusinessUnit) IN ('7600') THEN '0003'				--ICT
				WHEN RTRIM(a.BusinessUnit) IN ('7700', '7750') THEN '0002'		--Finance
				WHEN RTRIM(a.BusinessUnit) IN ('7800', '7860', '1300', '1800', '1900') THEN '0001'		--Management
				WHEN RTRIM(a.BusinessUnit) IN ('7910') THEN '0004'				--BPI
				WHEN RTRIM(a.BusinessUnit) IN ('7930') THEN '0030'				--Legal Affairs
				WHEN RTRIM(a.BusinessUnit) IN ('8000', '9000') THEN '0021'		--General
				WHEN RTRIM(a.BusinessUnit) IN ('3230', '3310') THEN '0011'		--Production
				ELSE '****'		--Not Assigned
			END AS PostCode,
			GETDATE() AS CreatedDate,
			10003632 AS CreatedByEmpNo,
			'ervin' AS CreatedByUser
		FROM tas2.tas.Master_BusinessUnit_JDE_V2 a WITH (NOLOCK)
		WHERE RTRIM(a.CompanyCode) = '00100'
			AND ISNULL(a.CostCenterManager, 0) > 0
			AND ISNULL(a.Superintendent, 0) > 0

		SELECT * FROM tas.UnisPostCostCenterMapping a WITH (NOLOCK)
		ORDER BY a.CostCenter

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 
    
