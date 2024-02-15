/************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetVisitorPassLog
*	Description: Retrieves visitor's log records
*
*	Date:			Author:		Rev. #:			Comments:
*	16/03/2016		Ervin		1.0				Created
************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_Retrieve_VisitorPassLog
(	
	@logID				BIGINT = 0,
	@visitorName		VARCHAR(100) = '',
	@idNumber			VARCHAR(50) = '',
	@visitorCardNo		VARCHAR(20) = '',
	@visitEmpNo			INT = 0,
	@visitCostCenter	VARCHAR(12) = '',
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@blockOption		TINYINT = 0,
	@createdByEmpNo		INT = 0 
)
AS

	DECLARE	@isBlock BIT
    
	--Validate parameters
	IF ISNULL(@logID, 0) = 0
		SET @logID = NULL

	IF ISNULL(@visitorName, '') = ''
		SET @visitorName = NULL

	IF ISNULL(@idNumber, '') = ''
		SET @idNumber = NULL

	IF ISNULL(@visitorCardNo, '') = ''
		SET @visitorCardNo = NULL
	
	IF ISNULL(@visitEmpNo, 0) = 0
		SET @visitEmpNo = NULL

	IF ISNULL(@visitCostCenter, '') = ''
		SET @visitCostCenter = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@createdByEmpNo, 0) = 0
		SET @createdByEmpNo = NULL

	SELECT @isBlock = CASE	WHEN @blockOption = 1 THEN 1	--Yes
							WHEN @blockOption = 2 THEN 0	--No
							ELSE NULL						--All
					  END 

	SELECT	a.LogID,
			a.VisitorName,
			a.IDNumber,
			a.VisitorCardNo,
			a.VisitEmpNo,
			b.EmpName AS VisitEmpName,
			LTRIM(RTRIM(ISNULL(h.JMDL01, ''))) AS VisitEmpPosition,
			ISNULL(CONVERT(VARCHAR(20), i.WPPH1), '') AS VisitEmpExtension,
			b.BusinessUnit AS VisitEmpCostCenter,
			g.BusinessUnitName AS VisitEmpCostCenterName,
			c.YAANPA as VisitEmpSupervisorNo,
			d.EmpName as VisitEmpSupervisorName,
			e.MCANPA as VisitEmpManagerNo, 
			f.EmpName as VisitEmpManagerName,
			a.VisitDate,
			a.VisitTimeIn,
			a.VisitTimeOut,
			a.Remarks,
			a.IsBlock,
			a.CreatedDate,
			a.CreatedByEmpNo,
			a.CreatedByUserID,
			a.CreatedByEmpName,
			a.CreatedByEmpEmail,
			a.LastUpdateTime,
			a.LastUpdateEmpNo,
			a.LastUpdateUserID,
			a.LastUpdateEmpName,
			a.LastUpdateEmpEmail
	FROM tas.VisitorPassLog a
		LEFT JOIN tas.Master_Employee_JDE_View b ON a.VisitEmpNo = b.EmpNo
		LEFT JOIN tas.syJDE_F060116 c on b.EmpNo = c.YAAN8
		LEFT JOIN tas.Master_Employee_JDE_View d on c.YAANPA = d.EmpNo
		LEFT JOIN tas.External_JDE_F0006 e on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(e.MCMCU))
		LEFT JOIN tas.Master_Employee_JDE_View f on e.MCANPA = f.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE g on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(g.BusinessUnit))
		LEFT JOIN tas.syJDE_F08001 h on LTRIM(RTRIM(c.YAJBCD)) = LTRIM(RTRIM(h.JMJBCD))
		LEFT JOIN tas.syJDE_F0115 i on b.EmpNo = i.WPAN8 AND upper(ltrim(rtrim(i.WPPHTP))) = 'EXT'
	WHERE (a.LogID = @logID OR @logID IS NULL)
		AND (UPPER(RTRIM(a.VisitorName)) LIKE '%' + UPPER(RTRIM(@visitorName)) + '%' OR @visitorName IS NULL)
		AND (RTRIM(a.IDNumber) = RTRIM(@idNumber) OR @idNumber IS NULL)
		AND (RTRIM(a.VisitorCardNo) = RTRIM(@visitorCardNo) OR @visitorCardNo IS NULL)
		AND (a.VisitEmpNo = @visitEmpNo OR @visitEmpNo IS NULL)
		AND (RTRIM(b.BusinessUnit) = RTRIM(@visitCostCenter) OR @visitCostCenter IS NULL)
		AND (a.VisitDate BETWEEN @startDate AND @endDate OR (@startDate IS NULL AND @endDate IS NULL))
		AND (a.IsBlock = @isBlock OR @isBlock IS NULL)
		AND (a.CreatedByEmpNo = @createdByEmpNo OR @createdByEmpNo IS NULL)

GO


/*	Debugging:
	
	SELECT * FROM tas.VisitorPassLog

	EXEC tas.Pr_Retrieve_VisitorPassLog 0, ''

Parameters:
	@logID				BIGINT = 0,
	@visitorName		VARCHAR(100) = '',
	@idNumber			VARCHAR(50) = '',
	@visitorCardNo		VARCHAR(20) = '',
	@visitEmpNo			INT = 0,
	@visitCostCenter	VARCHAR(12) = '',
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@blockOption		TINYINT = 0,
	@createdByEmpNo		INT = 0 

*/