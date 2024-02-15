/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetExpiredLicense
*	Description: This stored procedure is used to retrieve the list of employees with expired license
*
*	Date			Author		Revision No.	Comments:
*	16/01/2023		Ervin		1.0				Created
*
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetExpiredLicense
(
	@actionType		TINYINT = 0,			--(Notes: 0 = Get list of supervisors for expired license, 1 = Get already expired licenses, 2 = Get list of supervisors for about to expire licens, 3 = Get lis of all licenses about expire in 21 days)	
	@daysToExpire	TINYINT = 21,			--(Notes: default value is 21 days)	
	@executionDate	DATETIME = NULL,
	@supervisorNo	INT = 0,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	
)
AS 
BEGIN 
	
	DECLARE	@CONST_NOTIFICATION_COUNTER_THRESHOLD TINYINT = 2		--(Notes: Refers to the maximum number of notification to be sent to the employee)

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, '') = 0
		SET @empNo = NULL

	IF ISNULL(@supervisorNo, '') = 0
		SET @supervisorNo = NULL

	IF @actionType = 0
	BEGIN

		SELECT DISTINCT b.SupervisorNo
		FROM tas.IDCardRegistry a WITH (NOLOCK)
			INNER JOIN tas.Vw_EmployeeDirectory b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.LicenseRegistry c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 d WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(d.BusinessUnit) 
		WHERE c.ExpiryDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
			AND b.SupervisorNo > 0
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			--AND b.SupervisorNo = 10001364		--(Notes: comment this line before deployment used for testing purpose only)			
			--AND NOT EXISTS
			--(
			--	SELECT 1 FROM tas.ExpiredLicenseNotificationLog y WITH (NOLOCK)
			--	WHERE y.EmpNo = a.EmpNo
			--		AND y.SupervisorNo = b.SupervisorNo
			--		AND RTRIM(y.LicenseTypeCode) = RTRIM(c.LicenseTypeCode)
			--		AND y.IssuedDate = c.IssuedDate
			--		AND y.ExpiryDate = c.ExpiryDate
			--		AND y.NotificationCounter >= @CONST_NOTIFICATION_COUNTER_THRESHOLD
			--)
		ORDER BY b.SupervisorNo
    END

	ELSE IF @actionType = 1
	BEGIN
    
		SELECT 
			RTRIM(b.BusinessUnit) AS CostCenter, RTRIM(d.BusinessUnitName) AS CostCenterName,
			a.EmpNo, a.EmpName, b.Position,
			c.LicenseNo, c.LicenseTypeCode, c.LicenseTypeDesc, c.IssuedDate, c.ExpiryDate, 
			--c.RegistryID AS LicenseRegistryID,
			b.SupervisorNo, e.SupervisorName, e.SupervisorEmail,
			d.Superintendent, f.SuperintendentName, f.SuperintendentEmail,
			d.CostCenterManager, g.ManagerName, g.ManagerEmail		
			--,c.* 
		FROM tas.IDCardRegistry a WITH (NOLOCK)
			INNER JOIN tas.Vw_EmployeeDirectory b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.LicenseRegistry c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 d WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(d.BusinessUnit) 
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SupervisorName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SupervisorEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = b.SupervisorNo
			) e
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SuperintendentName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SuperintendentEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.Superintendent
			) f
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS ManagerName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS ManagerEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.CostCenterManager
			) g
		WHERE c.ExpiryDate < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (b.SupervisorNo = @supervisorNo OR @supervisorNo IS NULL)
			--AND NOT EXISTS
			--(
			--	SELECT 1 FROM tas.ExpiredLicenseNotificationLog y WITH (NOLOCK)
			--	WHERE y.EmpNo = a.EmpNo
			--		AND y.SupervisorNo = b.SupervisorNo
			--		AND RTRIM(y.LicenseTypeCode) = RTRIM(c.LicenseTypeCode)
			--		AND y.IssuedDate = c.IssuedDate
			--		AND y.ExpiryDate = c.ExpiryDate
			--		AND y.NotificationCounter >= @CONST_NOTIFICATION_COUNTER_THRESHOLD
			--)
		GROUP BY b.SupervisorNo,
			b.BusinessUnit, d.BusinessUnitName, a.EmpNo, a.EmpName, b.Position,
			c.LicenseNo, c.LicenseTypeCode, c.LicenseTypeDesc, c.IssuedDate, c.ExpiryDate,
			b.SupervisorNo, e.SupervisorName, e.SupervisorEmail,
			d.Superintendent, f.SuperintendentName, f.SuperintendentEmail,
			d.CostCenterManager, g.ManagerName, g.ManagerEmail	
		ORDER BY b.BusinessUnit, a.EmpNo
	END 

	ELSE IF @actionType = 2
	BEGIN

		SELECT DISTINCT b.SupervisorNo
		FROM tas.IDCardRegistry a WITH (NOLOCK)
			INNER JOIN tas.Vw_EmployeeDirectory b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.LicenseRegistry c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 d WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(d.BusinessUnit) 
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SupervisorName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SupervisorEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = b.SupervisorNo
			) e
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SuperintendentName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SuperintendentEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.Superintendent
			) f
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS ManagerName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS ManagerEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.CostCenterManager
			) g
		WHERE c.ExpiryDate > CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) 
			AND c.ExpiryDate = DATEADD(DAY, @daysToExpire, CONVERT(DATETIME, CONVERT(VARCHAR, @executionDate, 12)))  
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (b.SupervisorNo = @supervisorNo OR @supervisorNo IS NULL)
			AND NOT EXISTS
			(
				SELECT 1 FROM tas.ExpiredLicenseNotificationLog y WITH (NOLOCK)
				WHERE y.EmpNo = a.EmpNo
					AND y.SupervisorNo = b.SupervisorNo
					AND RTRIM(y.LicenseTypeCode) = RTRIM(c.LicenseTypeCode)
					AND y.IssuedDate = c.IssuedDate
					AND y.ExpiryDate = c.ExpiryDate
					--AND y.NotificationCounter >= @CONST_NOTIFICATION_COUNTER_THRESHOLD
			)
		ORDER BY b.SupervisorNo
    END

	ELSE IF @actionType = 3
	BEGIN

		SELECT 
			RTRIM(b.BusinessUnit) AS CostCenter, RTRIM(d.BusinessUnitName) AS CostCenterName,
			a.EmpNo, a.EmpName, b.Position,
			c.LicenseNo, c.LicenseTypeCode, c.LicenseTypeDesc, c.IssuedDate, 
			c.ExpiryDate, 			
			b.SupervisorNo, e.SupervisorName, e.SupervisorEmail,
			d.Superintendent, f.SuperintendentName, f.SuperintendentEmail,
			d.CostCenterManager, g.ManagerName, g.ManagerEmail		
		FROM tas.IDCardRegistry a WITH (NOLOCK)
			INNER JOIN tas.Vw_EmployeeDirectory b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1
			INNER JOIN tas.LicenseRegistry c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 d WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(d.BusinessUnit) 
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SupervisorName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SupervisorEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = b.SupervisorNo
			) e
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS SuperintendentName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS SuperintendentEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.Superintendent
			) f
			OUTER APPLY
			(
				SELECT LTRIM(RTRIM(x.YAALPH)) AS ManagerName, LTRIM(RTRIM(ISNULL(y.EAEMAL, ''))) AS ManagerEmail
				FROM tas.syJDE_F060116 x WITH (NOLOCK)
					LEFT JOIN tas.syjde_F01151 y WITH (NOLOCK) ON x.YAAN8 = y.EAAN8 AND y.EAIDLN = 0 AND y.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(y.EAETP))) = 'E' 
				WHERE CAST(x.YAAN8 AS INT) = d.CostCenterManager
			) g
		WHERE c.ExpiryDate > CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) 
			--AND DATEADD(DAY, @daysToExpire, c.ExpiryDate) <= DATEADD(DAY, @daysToExpire, CONVERT(DATETIME, CONVERT(VARCHAR, @executionDate, 12)))  
			AND c.ExpiryDate = DATEADD(DAY, @daysToExpire, CONVERT(DATETIME, CONVERT(VARCHAR, @executionDate, 12)))  
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (b.SupervisorNo = @supervisorNo OR @supervisorNo IS NULL)
			AND NOT EXISTS
			(
				SELECT 1 FROM tas.ExpiredLicenseNotificationLog y WITH (NOLOCK)
				WHERE y.EmpNo = a.EmpNo
					AND y.SupervisorNo = b.SupervisorNo
					AND RTRIM(y.LicenseTypeCode) = RTRIM(c.LicenseTypeCode)
					AND y.IssuedDate = c.IssuedDate
					AND y.ExpiryDate = c.ExpiryDate
					--AND y.NotificationCounter >= @CONST_NOTIFICATION_COUNTER_THRESHOLD
			)
		GROUP BY b.SupervisorNo,
			b.BusinessUnit, d.BusinessUnitName, a.EmpNo, a.EmpName, b.Position,
			c.LicenseNo, c.LicenseTypeCode, c.LicenseTypeDesc, c.IssuedDate, c.ExpiryDate,
			b.SupervisorNo, e.SupervisorName, e.SupervisorEmail,
			d.Superintendent, f.SuperintendentName, f.SuperintendentEmail,
			d.CostCenterManager, g.ManagerName, g.ManagerEmail	
		ORDER BY b.BusinessUnit, a.EmpNo
    END 

END 


/*	Debug:

PARAMETERS:
	@actionType		TINYINT = 0,			--(Notes: 0 = Get list of supervisors, 1 = Get already expired licenses, 2 = Get all licenses about expire in the next 21 days)	
	@daysToExpire	TINYINT = 21,			--(Notes: default value is 21 days)	
	@executionDate	DATETIME = NULL,
	@supervisorNo	INT = 0,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	

	EXEC tas.Pr_GetExpiredLicense 0										--Get supervsiros of already expired licenses
	EXEC tas.Pr_GetExpiredLicense 1, 0, null, 10001364					--Get already expired licenses
	EXEC tas.Pr_GetExpiredLicense 2, 21, '01/29/2023'					--Get supervisors of licenses about expire in next 21 days
	EXEC tas.Pr_GetExpiredLicense 3, 21, '01/29/2023', 10001364			--Get all licenses about expire in the next 21 days
	EXEC tas.Pr_GetExpiredLicense 3, 21, '01/29/2023', 10001899

*/