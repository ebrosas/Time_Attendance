DECLARE	@actionType			TINYINT = 0,	--(Notes: 0 = Check records, 1 = Insert records)
		@isCommitTrans		BIT = 0

	IF @actionType = 0
	BEGIN
    
		SELECT * FROM tas.UnisOfficeCostCenterMapping a WITH (NOLOCK)
		ORDER BY a.CostCenter
	END 
	
	ELSE IF @actionType = 1
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.UnisOfficeCostCenterMapping
		(
			CompanyCode,
			CostCenter,
			OfficeCode,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUser
		)

		SELECT DISTINCT 
			a.CompanyCode AS CompanyID,
			a.BusinessUnit AS CostCenter,
			CASE WHEN RTRIM(a.BusinessUnit) IN ('7600') THEN '0003'			--ICT
				WHEN RTRIM(a.BusinessUnit) IN ('7500', '7700', '6100', '6200', '7750', '7800', '7860', '7930', '7100', '7575', '1300', '1800', '1900') THEN '0001'							--Admin Building
				WHEN RTRIM(a.BusinessUnit) IN 
				(
					'2110', '2111', '2112', '3220', '3230', '3240','3310','3320','3322','3410',
					'3412','3415','3420','3422','3430','3470','3480','3800','3880','3881',
					'3882','3883','3884','3885','3890','3900','3950'
				) THEN '0004'			--Production
				WHEN RTRIM(a.BusinessUnit) IN ('5200', '5300', '5400', '3250', '7910') THEN '0005'							--Engineering
				WHEN RTRIM(a.BusinessUnit) IN ('7250') THEN '0006'													--Medical
				WHEN RTRIM(a.BusinessUnit) IN ('4100', '4200') THEN '0007'											--Technical
				WHEN RTRIM(a.BusinessUnit) IN ('3880', '3881', '3882', '3883', '3884', '3885') THEN '0008'			--Foil Mill
				WHEN RTRIM(a.BusinessUnit) IN ('7300', '7400') THEN '0009'											--Purchasing
				WHEN RTRIM(a.BusinessUnit) IN ('7550', '7560', '7920') THEN '0010'									--Training
				WHEN RTRIM(a.BusinessUnit) IN ('7200', '7150') THEN '0011'											--Main Security
				ELSE '****'		--Not Assigned
			END AS OfficeCode,
			GETDATE() AS CreatedDate,
			10003632 AS CreatedByEmpNo,
			'ervin' AS CreatedByUser
		FROM tas2.tas.Master_BusinessUnit_JDE_V2 a WITH (NOLOCK)
		WHERE RTRIM(a.CompanyCode) = '00100'
			AND ISNULL(a.CostCenterManager, 0) > 0
			AND ISNULL(a.Superintendent, 0) > 0

		SELECT * FROM tas.UnisOfficeCostCenterMapping a WITH (NOLOCK)
		ORDER BY a.CostCenter

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 
    
