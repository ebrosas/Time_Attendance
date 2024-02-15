/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnEmployeeLongAbsence
*	Description: Retrieves the list of employees that have long absences
*
*	Date:			Author:		Rev. #:		Comments:
*	21/03/2019		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER FUNCTION	tas.fnEmployeeLongAbsence
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)
)
RETURNS @rtnTable 
TABLE 
(
	CostCenter				VARCHAR(12),
	EmpNo					INT,
	EmpName					VARCHAR(50),
	AttendanceHistory		VARCHAR(300),
	AbsentStartDate			DATETIME,
	AbsentEndDate			DATETIME,
	IsDayOffBeforeStartDate	BIT,
	IsDayOffAfterEndDate	BIT,
	DayOffArray				VARCHAR(200) 
)
AS
BEGIN

	DECLARE	@showAbsent		BIT = 1,
			@showSLP		BIT = 0, 
			@showUL			BIT = 0		

	DECLARE @ch1		VARCHAR(1),
			@ch2		VARCHAR(2),
			@ch3		VARCHAR(3)			

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SET @ch1 = char(160)
	SET @ch2 = char(160) +  char(160) 
	SET @ch3 = char(160) +  char(160) + char(160)

	DECLARE @tempTable1 TABLE
	(
		DT			DATETIME, 
		EmpNo		INT, 
		txt			VARCHAR(3),
		Flg			VARCHAR(1)
	)
	
	DECLARE @tempTable2 TABLE 
	(
		EmpNo 				INT, 
		EmpName				VARCHAR(50),
		ActualCostCenter	VARCHAR(12), 
		CostCenter			VARCHAR(12),
		CostCenterName		VARCHAR(40),		
		txt 				VARCHAR(300),
		Flg					VARCHAR(100)
	)
	
	INSERT INTO @tempTable1
	SELECT  a.DT,
			a.EmpNo,
 			CASE WHEN RTRIM(a.RemarkCode) = 'A' THEN tas.rpad(LTRIM(RTRIM(a.RemarkCode)), 3, '.') 
          		WHEN LEN(a.LeaveType) > 0 THEN tas.rpad(LTRIM(RTRIM(a.LeaveType)), 3, '.') 
				WHEN a.IsPublicHoliday = 1 THEN 'Pub'
				WHEN a.IsDILdayWorker = 1 THEN 'Ddw'
	  			WHEN ISNULL(a.ShiftCode, '') = '' THEN '...'
	  			ELSE tas.rpad(LTRIM(RTRIM(a.shiftCode)), 3, '.' )
			END AS Txt,
			'n' Flg
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.DT BETWEEN @startDate AND @endDate
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
	ORDER BY a.DT, a.EmpNo
	
	IF @showSLP = 1 
		UPDATE @tempTable1 SET Flg = 'y' WHERE Txt = 'SLP' 
	ELSE 
		UPDATE @tempTable1 SET Flg = 'n' WHERE Txt = 'SLP'

	IF @showUL = 1 
		UPDATE @tempTable1 SET Flg = 'y' WHERE Txt = 'UL.' 
	ELSE 
		UPDATE @tempTable1 SET Flg = 'n' WHERE Txt = 'UL.'

	IF @showAbsent = 1 
		UPDATE @tempTable1 SET Flg = 'y' WHERE Txt = 'A..' 
	ELSE 
		UPDATE @tempTable1 SET Flg = 'n' WHERE Txt = 'A..'
	
	UPDATE @tempTable1 SET Flg = '' WHERE Txt IN ('Pub' , 'Ddw' , 'O..')

	----------------------------
	INSERT INTO @tempTable2
	SELECT 	a.EmpNo ,
			a.EmpName,
			a.ActualCostCenter,
			a.BusinessUnit,
			a.BusinessUnitName,
			coalesce(T01.txt, @ch2 + '|') +
			coalesce(T02.txt, @ch2 + '|') +
			coalesce(T03.txt, @ch2 + '|') +
			coalesce(T04.txt, @ch2 + '|') +
			coalesce(T05.txt, @ch2 + '|') +
			coalesce(T06.txt, @ch2 + '|') +
			coalesce(T07.txt, @ch2 + '|') +
			coalesce(T08.txt, @ch2 + '|') +
			coalesce(T09.txt, @ch2 + '|') +
			coalesce(T10.txt, @ch2 + '|') +
			coalesce(T11.txt, @ch2 + '|') +
			coalesce(T12.txt, @ch2 + '|') +
			coalesce(T13.txt, @ch2 + '|') +
			coalesce(T14.txt, @ch2 + '|') +
			coalesce(T15.txt, @ch2 + '|') +
			coalesce(T16.txt, @ch2 + '|') +
			coalesce(T17.txt, @ch2 + '|') +
			coalesce(T18.txt, @ch2 + '|') +
			coalesce(T19.txt, @ch2 + '|') +
			coalesce(T20.txt, @ch2 + '|') +
			coalesce(T21.txt, @ch2 + '|') +
			coalesce(T22.txt, @ch2 + '|') +
			coalesce(T23.txt, @ch2 + '|') +
			coalesce(T24.txt, @ch2 + '|') +
			coalesce(T25.txt, @ch2 + '|') +
			coalesce(T26.txt, @ch2 + '|') +
			coalesce(T27.txt, @ch2 + '|') +
			coalesce(T28.txt, @ch2 + '|') +
			coalesce(T29.txt, @ch2 + '|') +
			coalesce(T30.txt, @ch2 + '|') AS Txt,
			--coalesce(T30.txt, @ch2 + '|') + 
			--coalesce(T31.txt, @ch2 + '|') AS Txt,

			coalesce(T01.Flg , 'n' ) +
			coalesce(T02.Flg , 'n' ) +
			coalesce(T03.Flg , 'n' ) +
			coalesce(T04.Flg , 'n' ) +
			coalesce(T05.Flg , 'n' ) +
			coalesce(T06.Flg , 'n' ) +
			coalesce(T07.Flg , 'n' ) +
			coalesce(T08.Flg , 'n' ) +
			coalesce(T09.Flg , 'n' ) +
			coalesce(T10.Flg , 'n' ) +
			coalesce(T11.Flg , 'n' ) +
			coalesce(T12.Flg , 'n' ) +
			coalesce(T13.Flg , 'n' ) +
			coalesce(T14.Flg , 'n' ) +
			coalesce(T15.Flg , 'n' ) +
			coalesce(T16.Flg , 'n' ) +
			coalesce(T17.Flg , 'n' ) +
			coalesce(T18.Flg , 'n' ) +
			coalesce(T19.Flg , 'n' ) +
			coalesce(T20.Flg , 'n' ) +
			coalesce(T21.Flg , 'n' ) +
			coalesce(T22.Flg , 'n' ) +
			coalesce(T23.Flg , 'n' ) +
			coalesce(T24.Flg , 'n' ) +
			coalesce(T25.Flg , 'n' ) +
			coalesce(T26.Flg , 'n' ) +
			coalesce(T27.Flg , 'n' ) +
			coalesce(T28.Flg , 'n' ) +
			coalesce(T29.Flg , 'n' ) +
			coalesce(T30.Flg , 'n' ) AS Flg
			--coalesce(T30.Flg , 'n' ) +
			--coalesce(T31.Flg , 'n' ) AS Flg
	FROM
	(
		SELECT	a.EmpNo, 
				a.EmpName,
				a.BusinessUnit,
				b.BusinessUnitName,
				a.ActualCostCenter		
		FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			LEFT JOIN tas.Master_BusinessUnit_JDE b WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
		WHERE a.DateResigned is NULL
			AND ISNUMERIC(a.PayStatus) = 1
			AND RTRIM(a.Company) IN ('00100')
			AND NOT a.EmpNo BETWEEN 10005000 AND  10005999
			AND NOT (a.EmpNo BETWEEN 10002000 AND  10002999 AND RTRIM(a.ActualCostCenter) = '7600')
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
	) a
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 0, @startDate) ) T01  on a.EmpNo = T01.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 1,@startDate) ) T02  on a.EmpNo = T02.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 2,@startDate) ) T03  on a.EmpNo = T03.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 3,@startDate) ) T04  on a.EmpNo = T04.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 4,@startDate) ) T05  on a.EmpNo = T05.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 5,@startDate) ) T06  on a.EmpNo = T06.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 6,@startDate) ) T07  on a.EmpNo = T07.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 7,@startDate) ) T08  on a.EmpNo = T08.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 8,@startDate) ) T09  on a.EmpNo = T09.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 9,@startDate) ) T10  on a.EmpNo = T10.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 10,@startDate) ) T11  on a.EmpNo = T11.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 11,@startDate) ) T12  on a.EmpNo = T12.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 12,@startDate) ) T13  on a.EmpNo = T13.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 13,@startDate) ) T14  on a.EmpNo = T14.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 14,@startDate) ) T15  on a.EmpNo = T15.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 15,@startDate) ) T16  on a.EmpNo = T16.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 16,@startDate) ) T17  on a.EmpNo = T17.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 17,@startDate) ) T18  on a.EmpNo = T18.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 18,@startDate) ) T19  on a.EmpNo = T19.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 19,@startDate) ) T20  on a.EmpNo = T20.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 20,@startDate) ) T21  on a.EmpNo = T21.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 21,@startDate) ) T22  on a.EmpNo = T22.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 22,@startDate) ) T23  on a.EmpNo = T23.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 23,@startDate) ) T24  on a.EmpNo = T24.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 24,@startDate) ) T25  on a.EmpNo = T25.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 25,@startDate) ) T26  on a.EmpNo = T26.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 26,@startDate) ) T27  on a.EmpNo = T27.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 27,@startDate) ) T28  on a.EmpNo = T28.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 28,@startDate) ) T29  on a.EmpNo = T29.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 29,@startDate) ) T30  on a.EmpNo = T30.EmpNo
	left join (select txt , Flg, empno from @tempTable1 where dt = DATEADD(D, 30,@startDate) ) T31  on a.EmpNo = T31.EmpNo

	UPDATE @tempTable2 set Txt = replace(Txt, 'SLP', 'SL|')
	UPDATE @tempTable2 set Txt = replace(Txt, 'Pub', 'Ph|')
	UPDATE @tempTable2 set Txt = replace(Txt, 'Ddw', 'Dh|')
	UPDATE @tempTable2 set Txt = replace(Txt, '...', @ch2 + '|')
	UPDATE @tempTable2 set Txt = replace(Txt, '..', @ch1 + '|')
	UPDATE @tempTable2 set Txt = REPLACE(Txt, '.', '|')

	IF @showSLP = 1 UPDATE @tempTable2 SET Txt = REPLACE(Txt , 'SL', '<b>SL</b>')
	IF @showUL = 1 UPDATE @tempTable2 SET Txt = REPLACE(Txt , 'UL', '<b>UL</b>')
	IF @showAbsent = 1 UPDATE @tempTable2 SET Txt = REPLACE(Txt , 'A' + @ch1 , '<b>A' + @ch1 + '</b>' )

	INSERT INTO @rtnTable 
	SELECT a.ActualCostCenter, a.EmpNo, a.EmpName, a.TXT AS AttendanceHistory,
		b.AbsentStartDate, b.AbsentEndDate,
		c.IsDayOffBeforeStartDate, c.IsDayOffAfterEndDate, c.DayOffArray
	FROM @tempTable2 a
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
	WHERE Flg LIKE '%yyyyy%'
		AND ISNULL(c.DayOffArray, '') <> ''
	ORDER BY EmpNo

	RETURN 

END 

/*	Debug:

	--Live database
	SELECT * FROM tas.fnEmployeeLongAbsence('02/16/2019', '03/15/2019', 0, '') 
	ORDER BY EmpNo

	SELECT * FROM tas.fnEmployeeLongAbsence('03/16/2019', '04/15/2019', 0, '') 
	ORDER BY EmpNo

	--Test database
	SELECT * FROM tas.fnEmployeeLongAbsence('02/16/2016', '03/15/2016', 0, '') 
	ORDER BY EmpNo

	SELECT * FROM tas.fnEmployeeLongAbsence('11/16/2015', '12/15/2015', 0, '') 
	ORDER BY EmpNo


*/