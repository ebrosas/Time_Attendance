USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Payroll_Validation_ControlTotals_for_JDE]    Script Date: 22/06/2020 15:06:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Payroll_Validation_ControlTotals_for_JDE
*	Description: This is the stored procedure used in the "TAS and JDE Comparison Report"
*
*	Date:			Author:		Rev. #:		Comments:
*	23/06/2016		Ervin		1.1			Refactored the code
*	21/12/2016		Ervin		1.2			Modified the calculation added link to tas.syJDE_F0618 table
*	23/03/2017		Ervin		1.3			Refactored the code to remove drop and create statements and replace with trancate table statement
*	26/07/2018		Ervin		1.4			Refactored the code to enhance data retrieval performance by adding the WITH (NOLOCK) clause
*************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Payroll_Validation_ControlTotals_for_JDE] 
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	DECLARE @Upload_ID int

	IF OBJECT_ID('tas.tmp_CHK_JDE_TS') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_TS

	IF OBJECT_ID('tas.tmp_CHK_JDE_summry') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_summry

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba010') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba010

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba020') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba020

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba030') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba030

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba095') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba095

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba100A') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba100A

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba100B') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba100B

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba505') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba505

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba506') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba506

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba510') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba510

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba511') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba511

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba105') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba105

	IF OBJECT_ID('tas.tmp_CHK_JDE_pdba106') IS NOT NULL
		TRUNCATE TABLE tas.tmp_CHK_JDE_pdba106

	SELECT @Upload_ID = Upload_ID FROM tas.System_Values 

	INSERT INTO tas.tmp_CHK_JDE_TS
	SELECT 	
		a.autoid,
		a.dt,
		a.empno,
		CAST(a.GradeCode AS INT) grd,
		a.otStarttime OT1, 
		a.otEndtime   OT2, 
		a.nopayhours nph, 	
		a.remarkcode rem, 
		a.duration_shiftallowance_evening E,
		a.duration_shiftallowance_night N,
		IsPublicHoliday Pub,
		IsDILdayWorker DILdw,
		IsDayWorker_OR_Shifter DW,
		shiftcode sh,
		a.BusinessUnit AS BU,
		Duration_Worked_cumulative MinsWorked,
		JobCode,
		IsLastRow
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.PayStatus <>'2'
		LEFT JOIN tas.Master_Employee_JDE_view_Extra c WITH (NOLOCK) ON c.EmpNo = a.EmpNo
	WHERE   
		b.DateResigned IS NULL
		AND a.Upload_ID = @Upload_ID
	ORDER BY a.DT, a.EmpNo, a.AutoID

	-------------------------------------------------------------------------------------
	
	INSERT INTO tas.tmp_CHK_JDE_pdba010 
	SELECT	'010 OT' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt,
			empno, 
			ot1,
			ot2, 
			DATEDIFF(n, ot1, ot2) / 60.0 AS OT    	
	FROM tas.tmp_CHK_JDE_TS WITH (NOLOCK) 
	WHERE ot1 IS NOT NULL 
		AND tas.fmttime(ot1) <> tas.fmttime(ot2)
	
	UPDATE tas.tmp_CHK_JDE_pdba010
	SET txt = '010 OT-'
	WHERE autoid IN 
	(
		SELECT a.autoid 
		FROM	
		(
			SELECT autoid, OT FROM tas.tmp_CHK_JDE_pdba010  WITH (NOLOCK)
		) A,
		(
			SELECT autoid, sum(hoursWorkd) SumJdeOT 
			FROM tas.Tran_Timesheet_JDE WITH (NOLOCK) 
			WHERE PayType = 10 
			GROUP BY AutoID
		) B
		WHERE A.autoid = B.autoid
			AND  a.OT = B.SumJdeOT
	)
	
	INSERT INTO tas.tmp_CHK_JDE_pdba010
	SELECT	'010 OT+' AS txt, 
			a.AutoID, 
			tas.fmtdate(a.DT) AS dt,
			a.EmpNo, 
			a.OTStartTime,
			a.OTEndTime,
			DATEDIFF(n, a.OTStartTime, a.OTEndTime) /60.0 AS OT    
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.AutoID IN (SELECT AutoID FROM tas.TimeSheetUploaded_JDE WITH (NOLOCK) WHERE PDBA = 10 AND Hrs < 0)
		AND a.AutoID NOT IN (SELECT autoid FROM tas.tmp_CHK_JDE_pdba010 WITH (NOLOCK))
	

	---Shift Allowance Night
	INSERT INTO tas.tmp_CHK_JDE_pdba020 
	SELECT	'020 Shift Allowance Night' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt, 
			empno 	
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = CAST(b.YAAN8 AS INT)
	WHERE a.N >= 240
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)
	
	---Shift Allowance Evening
	INSERT INTO tas.tmp_CHK_JDE_pdba030 
	SELECT '030 Shift Allowance Evening' txt, autoid, tas.fmtdate(dt) dt, empno 	
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE a.E >= 240
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)
	
	---Extra pay (Absences Removed)
	INSERT INTO tas.tmp_CHK_JDE_pdba095
	SELECT  '095 Extra pay (Absences Removed)' txt,  autoid, tas.fmtdate(dt) dt, empno	
	FROM
	(
		SELECT autoid, dt, empno
		FROM tas.tmp_CHK_JDE_TS a WITH (NOLOCK) 
			INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
		WHERE ISNULL(a.rem, '') = '' 
			AND a.autoid IN (SELECT AutoID FROM tas.Tran_Timesheet_JDE WITH (NOLOCK) WHERE PayType = 510)
			AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
			--AND a.autoid NOT IN
			--(
			--	SELECT YTITM FROM tas.syJDE_F0618
			--)
	) A
	
	---Extra pay (holiday)
	INSERT INTO tas.tmp_CHK_JDE_pdba100A
	SELECT	'100(A) Extra pay (holiday)' AS txt, 
			autoid, tas.fmtdate(dt) AS dt, 
			empno	
	FROM 
	(
		SELECT  autoid , tas.fmtdate(dt) dt , empno
		FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
			INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
		WHERE  a.Pub = 1   
			AND RTRIM(a.sh) = 'O'
			AND ISNULL(a.dw, 0) = 0  
			AND a.islastrow = 1
			AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
			AND a.autoid NOT IN
			(
				SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
			)
	) A
	
	--Extra pay
	INSERT INTO tmp_CHK_JDE_pdba100B
	SELECT  '100(B) Extra pay (nonsal DW DilDW Sh=O)' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt,
			empno	
	FROM tmp_CHK_JDE_ts a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE a.DILdw = 1 
		AND a.DW = 1 
		AND a.grd <= 8 
		AND RTRIM(a.sh) = 'o'
		AND a.islastrow = 1
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)
	
	---No Pay Hours
	INSERT INTO tas.tmp_CHK_JDE_pdba505
	SELECT	'505 NoPay Hours' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt, 
			empno  	
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE a.nph > 0
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)
	
	---NoPay Hours adjustment
	INSERT INTO tas.tmp_CHK_JDE_pdba506
	SELECT	'506 NoPay Hours adjustment' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt, 
			empno
	--INTO tmp_CHK_JDE_pdba506
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE a.nph = 0 
		AND autoid IN (SELECT AutoID FROM tas.Tran_Timesheet_JDE WITH (NOLOCK) WHERE paytype = 505)
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		--AND a.autoid NOT IN
		--(
		--	SELECT YTITM FROM tas.syJDE_F0618
		--)
	
	---Absences
	INSERT INTO tas.tmp_CHK_JDE_pdba510
	SELECT	'510 Absences' AS txt, 
			a.autoid, 
			tas.fmtdate(a.dt) AS dt, 
			a.empno  	
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE 
		(
			(UPPER(RTRIM(a.rem)) = 'A' AND a.grd <= 8 AND a.islastrow = 1)
			OR
			(UPPER(RTRIM(a.rem)) = 'A' AND a.grd >= 9 AND ISNULL(a.DW, 0) = 0 AND a.islastrow = 1)
		)
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)

	--Absences adjustment
	INSERT INTO tas.tmp_CHK_JDE_pdba511
	SELECT	'511 Absences adjustment' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt, 
			empno	
	FROM tmp_CHK_JDE_TS a WITH (NOLOCK) 
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.empno = b.YAAN8
	WHERE 	
		(
			(a.grd <= 8 AND a.autoid IN (SELECT AutoID FROM tas.Tran_Timesheet_JDE WITH (NOLOCK) WHERE paytype = 511))
			OR
			(grd >= 9 AND ISNULL(a.DW, 0) = 0 AND autoid IN (SELECT AutoID FROM tas.Tran_Timesheet_JDE WITH (NOLOCK) WHERE paytype = 511))
		)
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618 WITH (NOLOCK)
		)

	INSERT INTO tas.tmp_CHK_JDE_pdba105
	SELECT	'105 Work Condition Allowance' AS txt, 
			autoid, 
			tas.fmtdate(dt) AS dt, 
			empno	
	FROM tas.tmp_CHK_JDE_TS A WITH (NOLOCK), 
		tas.External_JDE_F00092 B WITH (NOLOCK)
	WHERE A.BU = B.BU
		AND A.JobCode = B.JobCode
		AND MinsWorked >= 60
		AND  IsLastRow = 1

	------------------------------------------------
	--create Table tmp_CHK_JDE_summry (PDBA varchar(10) , PDBA_Name varchar(50) , TAS_cnt int , Diff_TAS int , JDE_cnt int , Diff_JDE int , Diff Int )
	insert into tmp_CHK_JDE_summry select '010'    , 'OT'   	                     , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba010 where txt <>'010 OT-'
	insert into tmp_CHK_JDE_summry select '020'    , 'Shift Allowance Night'             , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba020
	insert into tmp_CHK_JDE_summry select '030'    , 'Shift Allowance Evening'           , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba030
	insert into tmp_CHK_JDE_summry select '095'    , 'Extra pay (Absences Removed)'      , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba095
	insert into tmp_CHK_JDE_summry select '100(A)' , 'Extra pay (holiday) '              , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba100A
	insert into tmp_CHK_JDE_summry select '100(B)' , 'Extra pay (nonsal DW DilDW Sh=O)'  , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba100B
	insert into tmp_CHK_JDE_summry select '505'    , 'NoPay Hours'   	             , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba505
	insert into tmp_CHK_JDE_summry select '506'    , 'NoPay Hours adjustment'   	     , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba506
	insert into tmp_CHK_JDE_summry select '510'    , 'Absences'                          , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba510
	insert into tmp_CHK_JDE_summry select '511'    , 'Absences Adjustment'               , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba511
	-- Temporarily commented on 15-jan-2008
	-- Insert into tmp_CHK_JDE_summry select '105'    , 'Work Condition Allowance'          , count(*)    , null , null , null ,  0 from tmp_CHK_JDE_pdba105 
	insert into tmp_CHK_JDE_summry select '-1'     , 'OneWorld Batch No.'                , @Upload_ID  , null , null , null ,  0
	-------------------------------------------------

	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=10)   A where PDBA='010'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=20)   A where PDBA='020'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=30)   A where PDBA='030'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=95)   A where PDBA='095'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=100)  A where PDBA='100(A)'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select null cnt                                              )  A where PDBA='100(B)'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=505)  A where PDBA='505'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=506)  A where PDBA='506'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=510)  A where PDBA='510'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=511)  A where PDBA='511'
	update tmp_CHK_JDE_summry set JDE_Cnt = cnt from (select count(*) cnt from TimeSheetUploaded_JDE where pdba=105)  A where PDBA='105'
	-------------------------------------------------

	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_010  where empno is not null) A  where PDBA='010'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_020  where empno is not null) A  where PDBA='020'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_030  where empno is not null) A  where PDBA='030'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_095  where empno is not null) A  where PDBA='095'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_100A where empno is not null) A  where PDBA='100A'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_100B where empno is not null) A  where PDBA='100B'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_505  where empno is not null) A  where PDBA='505'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_506  where empno is not null) A  where PDBA='506'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_510  where empno is not null) A  where PDBA='510'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_511  where empno is not null) A  where PDBA='511'
	update tmp_CHK_JDE_summry set Diff_TAS = cnt from (select count(*) cnt from vu_JDE_mismatch_105  where empno is not null) A  where PDBA='105'
	-------------------------------------------------

	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_010  where Jempno is not null) A   where PDBA='010'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_020  where Jempno is not null) A   where PDBA='020'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_030  where Jempno is not null) A   where PDBA='030'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_095  where Jempno is not null) A   where PDBA='095'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_100A where Jempno is not null) A   where PDBA='100A'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_100B where Jempno is not null) A   where PDBA='100B'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_505  where Jempno is not null) A   where PDBA='505'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_506  where Jempno is not null) A   where PDBA='506'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_510  where Jempno is not null) A   where PDBA='510'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_511  where Jempno is not null) A   where PDBA='511'
	update tmp_CHK_JDE_summry set Diff_JDE = cnt from (select count(*) cnt from vu_JDE_mismatch_105  where Jempno is not null) A   where PDBA='105'

	-------------------------------------------------
	update tmp_CHK_JDE_summry set Diff = JDE_cnt - TAS_cnt
	-------------------------------------------------


/*	Debug:

	exec tas.Payroll_Validation_ControlTotals_for_JDE 

*/

