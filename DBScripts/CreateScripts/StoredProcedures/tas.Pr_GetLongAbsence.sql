/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetLongAbsence
*	Description: This stored procedure is used to search for employees who are on annual leave, sick leave or unpaid leave for a long time
*
*	Date			Author		Revision No.	Comments:
*	30/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetLongAbsence
(   	
	@processDate	DATETIME, 
	@showSLP		BIT = 1, 
	@showUL			BIT = 1, 
	@showAbsent		BIT = 1
)
AS

	DECLARE @ch1		VARCHAR(1),
			@ch2		VARCHAR(2),
			@ch3		VARCHAR(3),
			@QQ			VARCHAR(1),
			@startDate	DATETIME,
			@endDate	DATETIME

	SET @ch1 = char(160)
	SET @ch2 = char(160) +  char(160) 
	SET @ch3 = char(160) +  char(160) + char(160)
	SET @QQ = ''''

	SELECT	@startDate	= DATEADD(d, -29, @processDate),
			@endDate	= @processDate

	CREATE TABLE #Tmp1 
	(
		DT			DATETIME, 
		EmpNo		INT, 
		txt			VARCHAR(3),
		Flg			VARCHAR(1)
	)
	CREATE CLUSTERED    INDEX [{649EEC1E-B579-4E8C-BB3B-4997F8426536}] 	ON #Tmp1(empno)
	CREATE NONCLUSTERED INDEX [{586A6357-87C8-11D1-8BE3-0000F8754DA1}]	ON #Tmp1(DT)
	
	CREATE TABLE #Tmp2 
	(
		EmpNo 				INT, 
		EmpName				VARCHAR(50),
		ActualCostCenter	VARCHAR(12), 
		CostCenter			VARCHAR(12),
		CostCenterName		VARCHAR(40),		
		txt 				VARCHAR(300),
		Flg					VARCHAR(100)
	)
	
	INSERT INTO #Tmp1
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
	FROM tas.Tran_Timesheet a
	WHERE a.DT BETWEEN @startDate AND @endDate
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
	ORDER BY a.DT, a.EmpNo
	
	IF @showSLP = 1 
		UPDATE #Tmp1 SET Flg = 'y' WHERE Txt = 'SLP' 
	ELSE 
		UPDATE #Tmp1 SET Flg = 'n' WHERE Txt = 'SLP'

	IF @showUL = 1 
		UPDATE #Tmp1 SET Flg = 'y' WHERE Txt = 'UL.' 
	ELSE 
		UPDATE #Tmp1 SET Flg = 'n' WHERE Txt = 'UL.'

	IF @showAbsent = 1 
		UPDATE #Tmp1 SET Flg = 'y' WHERE Txt = 'A..' 
	ELSE 
		UPDATE #Tmp1 SET Flg = 'n' WHERE Txt = 'A..'
	
	UPDATE #Tmp1 SET Flg = '' WHERE Txt IN ('Pub' , 'Ddw' , 'O..')

	----------------------------
	INSERT INTO #Tmp2
	SELECT 	a.EmpNo ,
			a.EmpName,
			a.ActualCostCenter,
			a.BusinessUnit,
			a.BusinessUnitName,
			coalesce(T01.txt , @ch2 + '|') +
			coalesce(T02.txt , @ch2 + '|') +
			coalesce(T03.txt , @ch2 + '|') +
			coalesce(T04.txt , @ch2 + '|') +
			coalesce(T05.txt , @ch2 + '|') +
			coalesce(T06.txt , @ch2 + '|') +
			coalesce(T07.txt , @ch2 + '|') +
			coalesce(T08.txt , @ch2 + '|') +
			coalesce(T09.txt , @ch2 + '|') +
			coalesce(T10.txt , @ch2 + '|') +
			coalesce(T11.txt , @ch2 + '|') +
			coalesce(T12.txt , @ch2 + '|') +
			coalesce(T13.txt , @ch2 + '|') +
			coalesce(T14.txt , @ch2 + '|') +
			coalesce(T15.txt , @ch2 + '|') +
			coalesce(T16.txt , @ch2 + '|') +
			coalesce(T17.txt , @ch2 + '|') +
			coalesce(T18.txt , @ch2 + '|') +
			coalesce(T19.txt , @ch2 + '|') +
			coalesce(T20.txt , @ch2 + '|') +
			coalesce(T21.txt , @ch2 + '|') +
			coalesce(T22.txt , @ch2 + '|') +
			coalesce(T23.txt , @ch2 + '|') +
			coalesce(T24.txt , @ch2 + '|') +
			coalesce(T25.txt , @ch2 + '|') +
			coalesce(T26.txt , @ch2 + '|') +
			coalesce(T27.txt , @ch2 + '|') +
			coalesce(T28.txt , @ch2 + '|') +
			coalesce(T29.txt , @ch2 + '|') +
			coalesce(T30.txt , @ch2 + '|') Txt,

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
			coalesce(T30.Flg , 'n' ) Flg
	FROM
	(
		SELECT	a.EmpNo, 
				a.EmpName,
				a.BusinessUnit,
				b.BusinessUnitName,
				a.ActualCostCenter
		--FROM tas.Master_Employee_JDE a
		FROM tas.Master_Employee_JDE_View_V2 a
			LEFT JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
		WHERE a.DateResigned is NULL
			AND ISNUMERIC(a.PayStatus) = 1
			AND ISNULL(a.ActualCostCenter, 'EMPTY') NOT IN ('EMPTY', '8500000')
	) a
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 0, @startDate) ) T01  on a.EmpNo = T01.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 1,@startDate) ) T02  on a.EmpNo = T02.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 2,@startDate) ) T03  on a.EmpNo = T03.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 3,@startDate) ) T04  on a.EmpNo = T04.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 4,@startDate) ) T05  on a.EmpNo = T05.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 5,@startDate) ) T06  on a.EmpNo = T06.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 6,@startDate) ) T07  on a.EmpNo = T07.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 7,@startDate) ) T08  on a.EmpNo = T08.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 8,@startDate) ) T09  on a.EmpNo = T09.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 9,@startDate) ) T10  on a.EmpNo = T10.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 10,@startDate) ) T11  on a.EmpNo = T11.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 11,@startDate) ) T12  on a.EmpNo = T12.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 12,@startDate) ) T13  on a.EmpNo = T13.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 13,@startDate) ) T14  on a.EmpNo = T14.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 14,@startDate) ) T15  on a.EmpNo = T15.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 15,@startDate) ) T16  on a.EmpNo = T16.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 16,@startDate) ) T17  on a.EmpNo = T17.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 17,@startDate) ) T18  on a.EmpNo = T18.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 18,@startDate) ) T19  on a.EmpNo = T19.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 19,@startDate) ) T20  on a.EmpNo = T20.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 20,@startDate) ) T21  on a.EmpNo = T21.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 21,@startDate) ) T22  on a.EmpNo = T22.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 22,@startDate) ) T23  on a.EmpNo = T23.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 23,@startDate) ) T24  on a.EmpNo = T24.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 24,@startDate) ) T25  on a.EmpNo = T25.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 25,@startDate) ) T26  on a.EmpNo = T26.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 26,@startDate) ) T27  on a.EmpNo = T27.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 27,@startDate) ) T28  on a.EmpNo = T28.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 28,@startDate) ) T29  on a.EmpNo = T29.EmpNo
	left join (select txt , Flg, empno from #Tmp1 where dt = DATEADD(D, 29,@startDate) ) T30  on a.EmpNo = T30.EmpNo

	DECLARE @Title VARCHAR(500)
	SET @Title =	tas.lpad( day(DATEADD(D,00,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,01,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,02,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,03,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,04,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,05,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,06,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,07,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,08,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,09,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,10,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,11,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,12,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,13,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,14,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,15,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,16,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,17,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,18,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,19,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,20,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,21,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,22,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,23,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,24,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,25,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,26,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,27,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,28,@startDate)) ,  2 , '0' ) + '|' +
					tas.lpad( day(DATEADD(D,29,@startDate)) ,  2 , '0' ) + '|' 
	
	UPDATE #Tmp2 set Txt = replace(Txt, 'SLP', 'SL|')
	UPDATE #Tmp2 set Txt = replace(Txt, 'Pub', 'Ph|')
	UPDATE #Tmp2 set Txt = replace(Txt, 'Ddw', 'Dh|')
	UPDATE #Tmp2 set Txt = replace(Txt, '...', @ch2 + '|')
	UPDATE #Tmp2 set Txt = replace(Txt, '..', @ch1 + '|')
	UPDATE #Tmp2 set Txt = replace(Txt, '.', '|')

	IF @showSLP = 1 UPDATE #Tmp2 SET Txt = REPLACE(Txt , 'SL', '<b>SL</b>')
	IF @showUL = 1 UPDATE #Tmp2 SET Txt = REPLACE(Txt , 'UL', '<b>UL</b>')
	IF @showAbsent = 1 UPDATE #Tmp2 SET Txt = REPLACE(Txt , 'A' + @ch1 , '<b>A' + @ch1 + '</b>' )

	DECLARE  @sql VARCHAR(500)
	SET @sql = ''
	--SET @sql = @sql + 'SELECT EmpNo, EmpName, ActualCostCenter, CostCenter, CostCenterName, Txt [' + @Title + '] ' 
	SET @sql = @sql + 'SELECT EmpNo, EmpName, ActualCostCenter, CostCenter, CostCenterName, Txt AS AttendanceHistoryValue ' 
	SET @sql = @sql + 'FROM #Tmp2 '
	SET @sql = @sql + 'WHERE Flg like ' + @QQ +  '%yyyyy%'  + @QQ  + ' '
	SET @sql = @sql + 'ORDER BY EmpNo'
	
	EXECUTE (@sql)
	
	SELECT	@startDate AS FromDate, 
			@endDate AS ToDate,
			@Title AS AttendanceHistoryTitle

	DROP TABLE #Tmp1
	DROP TABLE #Tmp2

GO

/*	Debugging:

PARAEMTERS:
	@processDate	DATETIME, 
	@showSLP		BIT = 1, 
	@showUL			BIT = 1, 
	@showAbsent		BIT = 1

	exec tas.Pr_GetLongAbsence '31/03/2016', 1, 1, 1

*/

