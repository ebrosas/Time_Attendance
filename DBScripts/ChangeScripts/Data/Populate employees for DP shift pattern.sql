/**************************************************************************************************************************
	Notes:	This data script will be used to assign Production Day Shift Workers with the new shift pattern code "DP",
			which is created for day shift workers who will be excluded in the Flexi-timing
**************************************************************************************************************************/

DECLARE	@isCommitTran		BIT,
		@effectiveDate		DATETIME,			--(Note: Effective date should always be 1 day greater than the execution date
		@endingDate			DATETIME,
		@changeType			VARCHAR(10),		--(Note: D = Permanent; T = Temporary)
		@newShiftPatCode	VARCHAR(2),		
		@newShiftPointer	NUMERIC(18,0),		--(Note: 1 = Sunday; 2 = Monday; 3 = Tuesday; 4 = Wednesday; 5 = Thursday; 6 = Friday; 7 = Saturday)
		@lastUpdateUser		VARCHAR(50),
		@lastUpdateTime 	DATETIME

SELECT	@isCommitTran		= 0,
		@effectiveDate		= '04/16/2016',		--(Note: Use dd/MM/yyyy in test database, Use MM/dd/yyyy in production database)
		@endingDate			= NULL,
		@changeType			= 'D',				--(Note: D = Permanent)
		@newShiftPatCode	= 'DP',				--(Note: DP = Plant Day Shift Work Schedule)		
		@newShiftPointer	= 7,				--(Note: 7 = Saturday)
		@lastUpdateUser		= 'System Admin',
		@lastUpdateTime		= GETDATE()		

	--Create a transaction
	BEGIN TRAN T1

	INSERT INTO tas.Tran_ShiftPatternChanges
    (
		EmpNo,
        EffectiveDate,
        EndingDate,
        ShiftPatCode,
        ShiftPointer,
        ChangeType,
        LastUpdateUser,
        LastUpdateTime
	)
    SELECT	A.EmpNo,
			@effectiveDate AS EffectiveDate,
			@endingDate AS EndingDate,
			@newShiftPatCode AS ShiftPatCode,
			@newShiftPointer AS ShiftPointer,
			@changeType AS ChangeType,
			@lastUpdateUser AS LastUpdateUser,
			@lastUpdateTime AS LastUpdateTime
	FROM
    (
		SELECT 
			a.EmpNo,
			a.EmpName,
			LTRIM(RTRIM(ISNULL(c.JMDL01, ''))) AS Position,
			CASE WHEN LTRIM(RTRIM(g.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
				WHEN LTRIM(RTRIM(g.ABAT1)) = 'UG' THEN LTRIM(RTRIM(g.ABMCU)) 
			END AS ActualCostCenter,
			a.BusinessUnit AS CostCenter,
			--d.BusinessUnitName,
			a.GradeCode,
			e.ShiftPatCode,		
			CONVERT(VARCHAR, f.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR, f.DepartFrom, 108) AS ShiftTiming,
			f.ShiftCode,
			CONVERT(VARCHAR, f.ArrivalTo, 108) AS ArrivalFrom,
			CONVERT(VARCHAR, f.DepartFrom, 108) AS DepartFrom,
			CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END AS EmpStatus,
			g.ABAT1,
			g.ABMCU
		FROM tas.Master_Employee_JDE_View a
			INNER JOIN tas.syJDE_F060116 b on a.EmpNo = b.YAAN8
			LEFT JOIN tas.syJDE_F08001 c on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(c.JMJBCD))
			--LEFT JOIN tas.Master_BusinessUnit_JDE d ON ltrim(rtrim(a.BusinessUnit)) = ltrim(rtrim(d.BusinessUnit))
			INNER JOIN tas.Master_EmployeeAdditional e ON a.EmpNo = e.EmpNo
			INNER JOIN tas.Master_ShiftTimes f ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.syJDE_F0101 g ON a.EmpNo = CAST(g.ABAN8 AS INT) 
		WHERE ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
			AND  RTRIM(f.ShiftCode) = 'D'
			AND 
			(
				(CONVERT(TIME, f.ArrivalTo) = CONVERT(TIME, '07:30:00') AND CONVERT(TIME, f.DepartFrom) = CONVERT(TIME, '16:00:00'))
				OR 
				RTRIM(e.ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			)
			AND
            (
				(
					a.BusinessUnit NOT IN 
					(
						'6100',
						'7100',
						'7150',
						'7300',
						'7400',
						'7500',
						'7550',
						'7575',
						'7600',
						'7700',
						'7725',
						'7800',
						'7910',
						'7930',
						'6007800'
					)
					OR 
					(
						CASE WHEN g.ABAT1 = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
							WHEN g.ABAT1 = 'UG' THEN LTRIM(RTRIM(g.ABMCU)) 
						END  
					) = '7920'
				)
			)
			AND a.GradeCode <= 11
	) A
	ORDER BY A.EmpNo

	--Check the affected records
	SELECT	
		CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(d.YAHMCU))
			WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
		END AS ActualCostCenter,
		b.BusinessUnit AS CostCenter, 
		a.EmpNo, b.EmpName, b.PayStatus, b.GradeCode, 
		a.EffectiveDate, a.EndingDate, a.ShiftPatCode, a.ShiftPointer, a.ChangeType
	FROM tas.Tran_ShiftPatternChanges a
		INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.syJDE_F0101 c ON a.EmpNo = CAST(c.ABAN8 AS INT) 
		LEFT JOIN tas.syJDE_F060116 d on a.EmpNo = d.YAAN8
	WHERE a.EmpNo IN
	(
		SELECT a.EmpNo
		FROM tas.Master_Employee_JDE_View a
			INNER JOIN tas.syJDE_F060116 b on a.EmpNo = b.YAAN8
			LEFT JOIN tas.syJDE_F08001 c on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(c.JMJBCD))
			LEFT JOIN tas.Master_BusinessUnit_JDE d on ltrim(rtrim(a.BusinessUnit)) = ltrim(rtrim(d.BusinessUnit))
			INNER JOIN tas.Master_EmployeeAdditional e ON a.EmpNo = e.EmpNo
			INNER JOIN tas.Master_ShiftTimes f ON RTRIM(e.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.syJDE_F0101 g ON a.EmpNo = CAST(g.ABAN8 AS INT) 
		WHERE ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
			AND  RTRIM(f.ShiftCode) = 'D'
			AND 
			(
				(CONVERT(TIME, f.ArrivalTo) = CONVERT(TIME, '07:30:00') AND CONVERT(TIME, f.DepartFrom) = CONVERT(TIME, '16:00:00'))
				OR 
				RTRIM(e.ShiftPatCode) IN ('D', 'D6', 'G', 'X')
			)
			AND 
			(
				(
					a.BusinessUnit NOT IN 
					(
						'6100',
						'7100',
						'7150',
						'7300',
						'7400',
						'7500',
						'7550',
						'7575',
						'7600',
						'7700',
						'7725',
						'7800',
						'7910',
						'7930',
						'6007800'
					)
					OR 
					(
						CASE WHEN g.ABAT1 = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
							WHEN g.ABAT1 = 'UG' THEN LTRIM(RTRIM(g.ABMCU)) 
						END  
					) = '7920'
				)
			)
			AND a.GradeCode <= 11
	)
	AND RTRIM(a.ShiftPatCode) = @newShiftPatCode
	ORDER BY 
		CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(d.YAHMCU))
			WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
		END, 
		a.EmpNo

	IF @isCommitTran = 1
		COMMIT TRAN T1
	ELSE
		ROLLBACK TRAN T1

/*	Debugging:

	--Get employees with the DP shift pattern
	SELECT	b.BusinessUnit AS CostCenter, 
		CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(d.YAHMCU))
			WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
		END AS ActualCostCenter,
		a.EmpNo, b.EmpName, b.PayStatus, b.GradeCode, 
		a.EffectiveDate, a.EndingDate, a.ShiftPatCode, a.ShiftPointer, a.ChangeType
	FROM tas.Tran_ShiftPatternChanges a
		INNER JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.syJDE_F0101 c ON a.EmpNo = CAST(c.ABAN8 AS INT) 
		LEFT JOIN tas.syJDE_F060116 d on a.EmpNo = d.YAAN8
	WHERE RTRIM(a.ShiftPatCode) = 'DP'
	ORDER BY b.BusinessUnit, a.EmpNo


	BEGIN TRAN T1

	DELETE FROM tas.Tran_ShiftPatternChanges
	WHERE RTRIM(ShiftPatCode) = 'DP'

	COMMIT TRAN T1

*/


