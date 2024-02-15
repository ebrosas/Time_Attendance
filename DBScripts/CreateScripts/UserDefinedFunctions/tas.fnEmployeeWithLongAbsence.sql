/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnEmployeeWithLongAbsence
*	Description: Retrieves the list of employees that have long absences
*
*	Date:			Author:		Rev. #:		Comments:
*	24/03/2019		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER FUNCTION tas.fnEmployeeWithLongAbsence
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT
)
RETURNS @rtnTable 
TABLE 
(
	CostCenter				VARCHAR(12),
	EmpNo					INT,
	EmpName					VARCHAR(100),
	AbsentCount				INT,
	AbsentStartDate			DATETIME,
	AbsentEndDate			DATETIME,
	IsDayOffBeforeStartDate	BIT,
	IsDayOffAfterEndDate	BIT,
	DayOffArray				VARCHAR(200) 
)
AS
BEGIN
	
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	DECLARE @tempTable1 TABLE
	(
		AutoID		INT,
		DT			DATETIME, 
		EmpNo		INT,
		EmpName		VARCHAR(100),
		CostCenter	VARCHAR(12),
		IsAbsent	BIT 
	)
	
	DECLARE	@CONST_ABSENT_THRESHOLD INT = 3

	INSERT INTO @tempTable1
	SELECT  a.AutoID,
			a.DT,
			a.EmpNo,
			b.EmpName,
			a.BusinessUnit AS CostCenter,
			CASE WHEN RTRIM(a.RemarkCode) = 'A' THEN 1 ELSE 0 END 
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND RTRIM(b.Company) IN ('00100') 
	WHERE a.RemarkCode = 'A'
		AND DT BETWEEN @startDate AND @endDate
		AND a.IsLastRow = 1
		AND NOT b.EmpNo BETWEEN 10005000 AND  10005999
		AND NOT (b.EmpNo BETWEEN 10002000 AND  10002999 AND RTRIM(b.ActualCostCenter) = '7600')
		AND (a.EmpNo = @empNo OR @empNo IS NULL)

	DECLARE @tempTable2 TABLE
	(				
		EmpNo		INT,
		EmpName		VARCHAR(50),
		CostCenter	VARCHAR(12),
		DT			DATETIME, 
		PrevDT		DATETIME, 
		AutoID		INT,
		PrevAutoID	INT,
		DayDiff		INT,
		AbsentCount	INT 
	)

	INSERT INTO @tempTable2 
	SELECT EmpNo, EmpName, CostCenter, DT, PrevDT, AutoID, PrevAutoID, DayDiff, AbsentCount
	FROM
    (
		SELECT DISTINCT 
			a.AutoID, 
			LAG(a.AutoID, 1, 0) OVER (ORDER BY a.AutoID) AS PrevAutoID,
			CostCenter, a.EmpNo, a.EmpName, cnt AS AbsentCount, a.DT, 
			CASE WHEN LAG(a.DT, 1, 0) OVER (ORDER BY a.DT) = CAST('' AS DATETIME) 
				THEN a.DT
				ELSE LAG(a.DT, 1, 0) OVER (ORDER BY a.DT)
			END AS PrevDT,
			DATEDIFF
			(
				DAY, 
				CASE WHEN LAG(a.DT, 1, 0) OVER (ORDER BY a.DT) = CAST('' AS DATETIME) 
					THEN a.DT
					ELSE LAG(a.DT, 1, 0) OVER (ORDER BY a.DT)
				END, 
				DT
			) AS DayDiff
		FROM
		(
			SELECT t.*
			FROM 
			(
				SELECT t.*, COUNT(*) OVER (PARTITION BY EmpNo) AS cnt
				FROM 
				(
					SELECT *, (row_number() over (partition by EmpNo order by IsAbsent)) AS grp
					FROM @tempTable1
				) t
				WHERE IsAbsent = 1
			 ) t
			WHERE cnt >= @CONST_ABSENT_THRESHOLD
		) a
	) x

	INSERT INTO @rtnTable 
	SELECT DISTINCT 
		CostCenter, a.EmpNo, a.EmpName, 
		--a.AbsentCount, 
		cnt AS AbsentCount, 
		b.AbsentStartDate, b.AbsentEndDate,
		c.IsDayOffBeforeStartDate, c.IsDayOffAfterEndDate, c.DayOffArray
	FROM 
    (
		SELECT t.*
		FROM 
		(
			SELECT t.*, COUNT(*) OVER (PARTITION BY EmpNo) AS cnt
			FROM 
			(
				SELECT *, (row_number() over (partition by EmpNo order by IsAbsent)) AS grp
				FROM @tempTable1
			) t
			WHERE IsAbsent = 1
		 ) t
		WHERE cnt >= @CONST_ABSENT_THRESHOLD
	) a
	--(
	--	SELECT x.* 
	--	FROM @tempTable2 x
	--		CROSS APPLY 
	--		(
	--			SELECT * FROM @tempTable2
	--			WHERE DayDiff >= 3
	--		) y
	--	WHERE x.AutoID <> y.PrevAutoID
	--) a
	CROSS APPLY
    (
		SELECT EmpNo, MIN(DT) AS AbsentStartDate, MAX(DT) AS AbsentEndDate
		FROM tas.Tran_Timesheet WITH (NOLOCK)
		WHERE RemarkCode = 'A'
			AND DT BETWEEN @startDate AND @endDate
			AND IsLastRow = 1
			AND EmpNo = a.EmpNo
		GROUP BY EmpNo
	) b
	CROSS APPLY
	(
		SELECT * FROM tas.fnGetDayOffToFlagAbsent(a.EmpNo, b.AbsentStartDate, b.AbsentEndDate)
	) c
	WHERE ISNULL(c.DayOffArray, '') <> ''
	ORDER BY CostCenter, a.EmpNo

	RETURN 

END 

/*	Debug:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME

	SELECT * FROM tas.fnEmployeeWithLongAbsence('01/01/2019', '12/31/2019', 0)
	SELECT * FROM tas.fnEmployeeWithLongAbsence('01/01/2019', '12/31/2019', 10003258)
	
		
*/