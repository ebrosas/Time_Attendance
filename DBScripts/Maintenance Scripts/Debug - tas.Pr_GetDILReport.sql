DECLARE	@empNo			INT = 0,
		@costCenter		VARCHAR(12) = '',
		@startDate		DATETIME = NULL,
		@endDate		DATETIME = NULL

SELECT	@empNo			= 10001227,
		@costCenter		= ''

	--Validate parameters
	IF ISNULL(@startDate, '') = '' OR @startDate = CONVERT(DATETIME, '')  
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = '' OR @endDate = CONVERT(DATETIME, '')  
		SET @endDate = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL	
		
	SELECT * FROM
	(	
		SELECT	a.AutoID,
				a.BusinessUnit,
				RTRIM(e.BUname) AS BusinessUnitName,
				a.EmpNo,
				d.EmpName,				
				a.DT,

				CASE WHEN b.EffectiveDate IS NOT NULL
					THEN 'UD' 
					ELSE a.DIL_Entitlement
				END AS DIL_Entitlement,
				CASE WHEN b.EffectiveDate IS NOT NULL
					THEN 'Used Department' 
					ELSE c.[DESCRIPTION] 
				END AS DIL_Desc,
				
				a.AbsenceReasonCode,
				b.EffectiveDate AS DateUsed,
				CASE 
					WHEN 
						(
							(
								CONVERT(VARCHAR, a.DT, 12) >= CONVERT(VARCHAR, DateAdd(M, -1  * f.Months_DILexpiry, GETDATE()), 12) 
								OR a.DT BETWEEN g.StartDate AND g.EndDate
							)
							AND RTRIM(a.DIL_Entitlement) IN ('ES', 'EA', 'AD')
						)
						THEN 
							CASE WHEN RTRIM(a.DIL_Entitlement) IN ('EA', 'AD') 
							THEN 'Approved DIL'					
							ELSE 'Entitled to DIL (Inactive)'	
							END  
					WHEN 
						(
							(
								CONVERT(VARCHAR, a.DT, 12) < CONVERT(VARCHAR, DateAdd(M, -1  * f.Months_DILexpiry, GETDATE()), 12) 
							)
							AND RTRIM(a.DIL_Entitlement) IN ('ES', 'EA', 'AD')
						)
						THEN 'Expired DIL' 					
					ELSE 'Used'									
				END AS Remarks
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Tran_Absence b ON a.EmpNo = b.EmpNo AND a.AutoID = b.XID_TS_DIL_ENT
			LEFT JOIN tas.Master_UDCValues_JDE c ON RTRIM(a.DIL_Entitlement) = LTRIM(RTRIM(c.CODE)) AND RTRIM(c.UDCKey) = '55  -1'
			INNER JOIN tas.Master_Employee_JDE d ON a.EmpNo = d.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(a.BusinessUnit) = RTRIM(e.BU)
			LEFT JOIN tas.DILExtensionSetting g ON (RTRIM(a.BusinessUnit) = RTRIM(g.CostCenter) OR a.EmpNo = g.EmpNo) AND g.IsActive = 1,	--Rev. #1.2
			tas.System_Values f
		WHERE 
			a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, '140919', 12))	--(Notes: Exclude Attendance Date equal to 19/09/2014)
			AND ISNULL(a.DIL_Entitlement, '') <> ''
			--AND d.DateResigned IS NULL	--Rev. #1.3

		UNION
    
		SELECT	a.AutoID,
				a.BusinessUnit,
				RTRIM(e.BUname) AS BusinessUnitName,
				a.EmpNo,
				d.EmpName,
				a.DT,
				a.DIL_Entitlement,
				c.[DESCRIPTION] AS DIL_Desc,		
				a.AbsenceReasonCode,
				NULL AS DateUsed,
				CASE 
					WHEN 
						(
							(
								CONVERT(VARCHAR, a.DT, 12) >= CONVERT(VARCHAR, DateAdd(M, -1  * f.Months_DILexpiry, GETDATE()), 12) 
								OR a.DT BETWEEN g.StartDate AND g.EndDate
							)
							AND RTRIM(a.DIL_Entitlement) IN ('ES', 'EA', 'AD')
						)
						THEN 
							CASE WHEN RTRIM(a.DIL_Entitlement) IN ('EA', 'AD') 
							THEN 'Approved DIL'					
							ELSE 'Entitled to DIL (Inactive)'	
							END  
					WHEN 
						(
							(
								CONVERT(VARCHAR, a.DT, 12) < CONVERT(VARCHAR, DateAdd(M, -1  * f.Months_DILexpiry, GETDATE()), 12) 
							)
							AND RTRIM(a.DIL_Entitlement) IN ('ES', 'EA', 'AD')
						)
						THEN 'Expired DIL' 					
					ELSE 'Used'									
				END AS Remarks
		FROM tas.Tran_Timesheet a
			--INNER JOIN tas.Tran_Absence b ON a.EmpNo = b.EmpNo AND a.AutoID = b.XID_TS_DIL_ENT
			LEFT JOIN tas.Master_UDCValues_JDE c ON RTRIM(a.DIL_Entitlement) = LTRIM(RTRIM(c.CODE)) AND RTRIM(c.UDCKey) = '55  -1'
			INNER JOIN tas.Master_Employee_JDE d ON a.EmpNo = d.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(e.BU))
			LEFT JOIN tas.DILExtensionSetting g ON (RTRIM(a.BusinessUnit) = RTRIM(g.CostCenter) OR a.EmpNo = g.EmpNo) AND g.IsActive = 1,  --Rev. #1.2
			tas.System_Values f
		WHERE 
			a.IsLastRow = 1
			AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, '140919', 12))	--(Notes: Exclude Attendance Date equal to 19/09/2014)
			AND ISNULL(a.DIL_Entitlement, '') <> ''
			AND RTRIM(a.DIL_Entitlement) NOT IN ('UA', 'UD', 'PE','RE')
			--AND d.DateResigned IS NULL	--Rev. #1.3
			AND a.AutoID NOT IN (SELECT XID_TS_DIL_ENT FROM tas.Tran_Absence WHERE XID_TS_DIL_ENT IS NOT NULL) 
			AND NOT EXISTS
			(
				SELECT tbl1.AutoID 
				FROM tas.Tran_Absence_DIL tbl1 
					INNER join tas.Tran_DIL_Consumption tbl2 on tbl2.RequisitionNo = tbl1.AutoID
				WHERE RTRIM(ISNULL(StatusCode, '')) NOT IN ('101','110') 
					AND ISNULL(tbl2.XID_TS_DIL_ENT, 0) = a.AutoID
			)
	) A
	WHERE 	
		(A.EmpNo = @empNo OR @empNo IS NULL)
		AND (LTRIM(RTRIM(A.BusinessUnit)) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND 
		(
			(A.DT BETWEEN @startDate AND @endDate AND @startDate IS NOT NULL AND @endDate IS NOT NULL)
			OR
            (@startDate IS NULL AND @endDate IS null)
		)
	ORDER BY A.EmpNo, LTRIM(RTRIM(A.Remarks)), A.DT DESC 