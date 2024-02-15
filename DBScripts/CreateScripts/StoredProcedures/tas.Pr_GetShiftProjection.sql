/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftProjection
*	Description: Get the shift projection data based on the specified start date
*
*	Date			Author		Rev.#		Comments:
*	06/11/2016		Ervin		1.0			Created
*	11/02/2019		Ervin		1.1			Modified the logic to allow retrieval of shift projection report for all cost centers
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftProjection
(   
	@startDate		DATETIME, 
	@costCenter		VARCHAR(12) 
)
AS

	SET NOCOUNT ON

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	CREATE TABLE #M 
	(
		CostCenter VARCHAR(12),
		EmpNo int, 
		Pat varchar(12) COLLATE database_default, 
		Po int, 
		MaxPo INT		
	) 

	INSERT INTO #M 
	SELECT a.BusinessUnit, a.EmpNo, null, null, NULL 
	FROM tas.Master_Employee_JDE a WITH (NOLOCK)
	WHERE DateResigned IS NULL 
		AND (RTRIM(BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		
	UPDATE #M 
	SET	Pat = A.ShiftPatCode ,
		Po  = A.ShiftPointer
	FROM tas.Master_EmployeeAdditional A 
	WHERE #M.EmpNo = A.EmpNo

	UPDATE #M 
	SET Pat = null, 
	Po = null 
	WHERE RTRIM(Pat) NOT IN
	(
		SELECT RTRIM(ShiftPatCode) FROM tas.Master_ShiftPattern
	)

	UPDATE #M 
	SET Pat = null, 
	Po = null 
	WHERE Pat IS NULL 
		OR Po IS NULL 
		OR Po < 1

	UPDATE #M 
	SET #M.MaxPo = SP.MaxPo 
	FROM 
	(
		SELECT	ShiftPatCode Pat, 
				Max(ShiftPointer) MaxPo 
		FROM tas.Master_ShiftPattern 
		GROUP BY ShiftPatCode
	) SP 
	WHERE #M.pat = Sp.Pat

	DECLARE @daysFromNow int
	SET @daysFromNow = datediff(d, getdate(), @startDate)

	CREATE TABLE #D 
	(
		CostCenter VARCHAR(12),
		EmpNo int,  
		Pat varchar(12) COLLATE database_default, 
		Po int, 
		MaxPo int,  
		dt DateTime, 
		Po2 int, 
		Po3 int, 
		Sh varchar(2) COLLATE database_default, 
		unique (EmpNo, dt) 
	) 

	INSERT INTO #D 
	SELECT CostCenter, EmpNo, pat , po  , MaxPo , @startDate + Autoid -1 ,  Po + @daysFromNow + Autoid -1 , null , null
	FROM #M,
	(select top 31 AutoID from tas.A10000 where AutoID<=31 order by AutoID) Sr

	UPDATE #D 
	SET Po3 = tas.mod(Po2 , MaxPo  )

	UPDATE #D set #D.Sh = SP.ShiftCode 
	FROM tas.Master_ShiftPattern SP 
	WHERE #D.Pat = Sp.ShiftPatCode 
		AND #D.Po3 = Sp.ShiftPointer

	UPDATE #D set #D.Sh = 'R' 
	FROM tas.Tran_Absence ROA
	WHERE #D.EmpNo = ROA.EmpNo
		AND #D.dt BETWEEN ROA.EffectiveDate AND ROA.EndingDate

	UPDATE #D set #D.Sh = 'L' 
	FROM tas.Tran_Leave_JDE LV
	WHERE #D.EmpNo = LV.EmpNo
		AND #D.dt between LV.FromDate and LV.ToDate

	UPDATE #D 
	SET #D.Sh = '.' 
	WHERE #D.Sh is null

	CREATE TABLE #R 
	(
		CostCenter VARCHAR(12),
		EmpNo int NULL ,
		EmpName varchar(30) COLLATE database_default NULL,
		ShiftPatCode CHAR(2) NULL,
		d1  varchar (2)  COLLATE database_default NULL ,
		d2  varchar (2)  COLLATE database_default NULL ,
		d3  varchar (2)  COLLATE database_default NULL ,
		d4  varchar (2)  COLLATE database_default NULL ,
		d5  varchar (2)  COLLATE database_default NULL ,
		d6  varchar (2)  COLLATE database_default NULL ,
		d7  varchar (2)  COLLATE database_default NULL ,
		d8  varchar (2)  COLLATE database_default NULL ,
		d9  varchar (2)  COLLATE database_default NULL ,
		d10 varchar (2)  COLLATE database_default NULL ,
		d11 varchar (2)  COLLATE database_default NULL ,
		d12 varchar (2)  COLLATE database_default NULL ,
		d13 varchar (2)  COLLATE database_default NULL ,
		d14 varchar (2)  COLLATE database_default NULL ,
		d15 varchar (2)  COLLATE database_default NULL ,
		d16 varchar (2)  COLLATE database_default NULL ,
		d17 varchar (2)  COLLATE database_default NULL ,
		d18 varchar (2)  COLLATE database_default NULL ,
		d19 varchar (2)  COLLATE database_default NULL ,
		d20 varchar (2)  COLLATE database_default NULL ,
		d21 varchar (2)  COLLATE database_default NULL ,
		d22 varchar (2)  COLLATE database_default NULL ,
		d23 varchar (2)  COLLATE database_default NULL ,
		d24 varchar (2)  COLLATE database_default NULL ,
		d25 varchar (2)  COLLATE database_default NULL ,
		d26 varchar (2)  COLLATE database_default NULL ,
		d27 varchar (2)  COLLATE database_default NULL ,
		d28 varchar (2)  COLLATE database_default NULL ,
		d29 varchar (2)  COLLATE database_default NULL ,
		d30 varchar (2)  COLLATE database_default NULL ,
		d31 varchar (2)  COLLATE database_default NULL		
	)

	INSERT INTO #R
	SELECT	sh01.CostCenter,
			sh01.EmpNo , 
			'', 
			NULL,
			sh01.Sh d1, 
			sh02.Sh d2,
			sh03.Sh d3,
			sh04.Sh d4,
			sh05.Sh d5,
			sh06.Sh d6,
			sh07.Sh d7,
			sh08.Sh d8,
			sh09.Sh d9,
			sh10.Sh d10, 
			sh11.Sh d11, 
			sh12.Sh d12,
			sh13.Sh d13,
			sh14.Sh d14,
			sh15.Sh d15,
			sh16.Sh d16,
			sh17.Sh d17,
			sh18.Sh d18,
			sh19.Sh d19,
			sh20.Sh d20,
			sh21.Sh d21, 
			sh22.Sh d22,
			sh23.Sh d23,
			sh24.Sh d24,
			sh25.Sh d25,
			sh26.Sh d26,
			sh27.Sh d27,
			sh28.Sh d28,
			sh29.Sh d29,
			sh30.Sh d30,
			sh31.Sh d31
	FROM 
		(SELECT CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+00) sh01,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+01) sh02,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+02) sh03,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+03) sh04,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+04) sh05,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+05) sh06,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+06) sh07,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+07) sh08,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+08) sh09,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+09) sh10,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+10) sh11,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+11) sh12,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+12) sh13,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+13) sh14,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+14) sh15,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+15) sh16,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+16) sh17,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+17) sh18,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+18) sh19,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+19) sh20,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+20) sh21,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+21) sh22,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+22) sh23,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+23) sh24,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+24) sh25,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+25) sh26,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+26) sh27,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+27) sh28,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+28) sh29,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+29) sh30,
		(select CostCenter, EmpNo , dt ,  Sh from #D where dt = @startDate+30) sh31
	WHERE 
		sh01.EmpNo = sh02.EmpNo
		and	sh01.EmpNo = sh03.EmpNo
		and	sh01.EmpNo = sh04.EmpNo
		and	sh01.EmpNo = sh05.EmpNo
		and	sh01.EmpNo = sh06.EmpNo
		and	sh01.EmpNo = sh07.EmpNo
		and	sh01.EmpNo = sh08.EmpNo
		and	sh01.EmpNo = sh09.EmpNo
		and	sh01.EmpNo = sh10.EmpNo
		and	sh01.EmpNo = sh11.EmpNo
		and	sh01.EmpNo = sh12.EmpNo
		and	sh01.EmpNo = sh13.EmpNo
		and	sh01.EmpNo = sh14.EmpNo
		and	sh01.EmpNo = sh15.EmpNo
		and	sh01.EmpNo = sh16.EmpNo
		and	sh01.EmpNo = sh17.EmpNo
		and	sh01.EmpNo = sh18.EmpNo
		and	sh01.EmpNo = sh19.EmpNo
		and	sh01.EmpNo = sh20.EmpNo
		and	sh01.EmpNo = sh21.EmpNo
		and	sh01.EmpNo = sh22.EmpNo
		and	sh01.EmpNo = sh23.EmpNo
		and	sh01.EmpNo = sh24.EmpNo
		and	sh01.EmpNo = sh25.EmpNo
		and	sh01.EmpNo = sh26.EmpNo
		and	sh01.EmpNo = sh27.EmpNo
		and	sh01.EmpNo = sh28.EmpNo
		and	sh01.EmpNo = sh29.EmpNo
		and	sh01.EmpNo = sh30.EmpNo
		and	sh01.EmpNo = sh31.EmpNo
	ORDER BY sh01.EmpNo

	UPDATE #R 
	SET #R.EmpName = substring(J.EmpName , 1,30)  
	FROM 
	(
		SELECT EmpNo, EmpName 
		FROM tas.Master_Employee_JDE a
		WHERE (RTRIM(LTRIM(a.BusinessUnit)) = RTRIM(@costCenter) OR @costCenter IS NULL)
	) J 
	WHERE #R.EmpNo = J.EmpNo

	UPDATE #R 
	SET #R.ShiftPatCode = RTRIM(a.ShiftPatCode)
	FROM tas.Master_EmployeeAdditional a
	WHERE #R.EmpNo = a.EmpNo

	SELECT * FROM #R
	ORDER BY CostCenter, EmpNo

	DROP TABLE #M
	DROP TABLE #D
	DROP TABLE #R


/*	Debugging:

PARAMETERS:
	@startDate		DATETIME, 
	@costCenter		VARCHAR(12) 

	EXEC tas.Pr_GetShiftProjection '01/01/2019', ''

*/
