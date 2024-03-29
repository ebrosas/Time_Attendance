USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetEmployeeSwipeInfo_Workplace]    Script Date: 06/06/2023 09:52:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeSwipeInfo_Workplace
*	Description: Retrieves the swipe records of the employees at the workplace readers 
*
*	Date			Author		Rev.#		Comments:
*	09/07/2015		Ervin		1.0			Created
*	12/08/2015		Ervin		1.1			Commented the code that checks for Actual_ShiftCode. 
*	10/01/2016		Ervin		1.2			Undo what has been done in Rev. #1.1
*	13/04/2016		Ervin		1.3			Use the working cost center in the filter condition
*	27/06/2016		Ervin		1.4			Fetch the ShiftCode from "Vw_EmployeeSwipeData_Workplace"
*	25/09/2016		Ervin		1.5			Refactored the filter condition for the working cost center
*	05/02/2020		Ervin		1.6			Refactored the code to enhance performance
*	06/02/2020		Ervin		1.7			Modified the logic in fetching the ShiftCode
*	08/05/2023		Ervin		1.8			Get the shift timing info from "Master_ShiftTimes" table
**************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetEmployeeSwipeInfo_Workplace]
(
	@startDate			datetime,
	@endDate			datetime,
	@empNo				int = 0,
	@costCenter			varchar(12)	= '',
	@locationName		varchar(40)	= '',
	@readerName			varchar(40)	= ''
)
AS
BEGIN

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL
		
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@locationName, '') = ''
		SET @locationName = NULL

	IF ISNULL(@readerName, '') = ''
		SET @readerName = NULL
	
	SELECT 
		a.EmpNo, 
		a.EmpName, 
		a.ShiftPatCode,
		a.ShiftPointer,
		
		g.ShiftCode AS ShiftCode,	--Rev. #1.7

		--CASE WHEN a.ShiftTiming IS NOT NULL 
		--	THEN a.ShiftCode
		--	ELSE f.Actual_ShiftCode
		--END AS ShiftCode,	--Rev. #1.4

		a.ShiftTiming,		
		a.SwipeDate, 
		a.SwipeTime, 	
		a.SwipeCode,				
		a.SwipeLocation, 
		a.SwipeType, 		
		LTRIM(RTRIM(ISNULL(c.JMDL01, ''))) AS Position,			
		a.GradeCode,
		a.EmpStatus,
		a.BusinessUnit, 
		b.BusinessUnitName,	
		RTRIM(d.WorkingBusinessUnit) AS WorkingCostCenter,
		RTRIM(ISNULL(e.BusinessUnitName, '')) AS WorkingCostCenterName,		

		--a.ArrivalFrom,
		--a.ArrivalTo,
		--a.DepartFrom,
		--a.DepartTo,
		--a.RArrivalFrom,
		--a.RArrivalTo,
		--a.RDepartFrom,
		--a.RDepartTo,
		h.ArrivalFrom,
		h.ArrivalTo,
		h.DepartFrom,
		h.DepartTo,
		h.RArrivalFrom,
		h.RArrivalTo,
		h.RDepartFrom,
		h.RDepartTo,

		a.DurationRequired,
		a.Remarks,
		a.IsContractor,
		a.IsDayShift		
	FROM tas.Vw_EmployeeSwipeData_Workplace a WITH (NOLOCK)
		LEFT JOIN tas.Master_BusinessUnit_JDE b WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)	
		LEFT JOIN tas.syJDE_F08001 c WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(c.JMJBCD))
		LEFT JOIN tas.Master_EmployeeAdditional d WITH (NOLOCK) ON a.EmpNo = d.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE e WITH (NOLOCK) on RTRIM(d.WorkingBusinessUnit) = RTRIM(e.BusinessUnit)		
		LEFT JOIN tas.Tran_Timesheet f WITH (NOLOCK) ON a.EmpNo = f.EmpNo AND f.IsLastRow = 1 AND a.SwipeDate = f.DT
		OUTER APPLY tas.fnGetShiftCode(a.EmpNo, a.SwipeDate) g		--Rev. #1.7
		LEFT JOIN tas.Master_ShiftTimes h WITH (NOLOCK) ON RTRIM(d.ShiftPatCode) = RTRIM(h.ShiftPatCode) AND RTRIM(g.ShiftCode) = RTRIM(h.ShiftCode)
	WHERE
		a.EmpNo > 0		
		AND a.SwipeDate BETWEEN DATEADD(d, -1, @startDate) AND @endDate 		
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		
		AND		--Rev. #1.5 
		(
			(RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR RTRIM(ISNULL(d.WorkingBusinessUnit, '')) = RTRIM(@costCenter))
			OR @costCenter IS NULL
		)
		AND (UPPER(RTRIM(a.LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
		AND (UPPER(RTRIM(a.ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)		
	ORDER BY a.IsContractor, a.BusinessUnit, a.SwipeDate, a.EmpNo, a.SwipeTime

END

/*	Debug:

	EXEC [tas].[Pr_GetEmployeeSwipeInfo_Workplace] '04/15/2023', '04/16/2023', 10006124

*/


