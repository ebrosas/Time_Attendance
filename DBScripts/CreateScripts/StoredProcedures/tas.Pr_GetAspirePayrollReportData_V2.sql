/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAspirePayrollReportData_V2
*	Description: Get Aspire employees payroll data
*
*	Date:			Author:		Revision #:		Comments:
*	30/11/2016		Ervin		1.0				Created
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAspirePayrollReportData_V2
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@processType	INT = 0		--(Note: 0 => All; 1 => Not yet processed; 2 => Processed)
)
AS	

	IF @processType = 0
		SET @processType = NULL

	SELECT	a.EmpNo, 
			a.EmpName, 
			a.DT, 
			a.PayGrade, 
			a.PayHour,
			a.PayMinute,
			a.PayDescription,
			CASE WHEN a.Processed = 1 THEN 'Processed' ELSE 'Not yet processed' END AS PaymentStatus 
	FROM
	(
		--No Pay Hours
		SELECT a.EmpNo, 
			b.YAALPH AS EmpName, 
			a.DT, 
			CASE WHEN ISNULL(a.NoPayHours, 0) > 0
				THEN tas.fnConvertMinuteToHourString(a.NoPayHours) 
				ELSE ''
			END AS PayHour,
			a.NoPayHours AS PayMinute,
			'No Pay' AS PayDescription,
			1 AS PayOrderNo,
			a.Processed,
			LTRIM(RTRIM(b.YAPGRD)) AS PayGrade
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		WHERE 
			CONVERT(VARCHAR, a.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			AND b.YAPAST IN ('8') 
			AND a.IsLastRow = 1 
			AND ISNULL(a.NoPayHours, 0) > 0 

		UNION 

		--Absences
		SELECT a.EmpNo, 
			b.YAALPH AS EmpName, 
			a.DT, 
			'08:00' AS PayHour, 
			480 AS PayMinute,
			'Absence' AS PayDescription,
			2 AS PayOrderNo,
			a.Processed,
			LTRIM(RTRIM(b.YAPGRD)) AS PayGrade 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		WHERE 
			CONVERT(VARCHAR, a.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			AND b.YAPAST IN ('8') 
			AND a.IsLastRow = 1 
			AND a.RemarkCode = 'A'

		UNION 

		--Overtime 
		SELECT a.EmpNo, 
			b.YAALPH AS EmpName, 
			a.DT, 
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL	
				THEN tas.fnConvertMinuteToHourString(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime)) 
				ELSE ''
			END AS PayHour,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL	
				THEN DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) 
				ELSE 0
			END AS PayMinute,
			'Overtime' AS PayDescription,
			3 AS PayOrderNo,
			a.Processed,
			LTRIM(RTRIM(b.YAPGRD)) AS PayGrade 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		WHERE 
			CONVERT(VARCHAR, a.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			AND b.YAPAST IN ('8') 
			AND a.IsLastRow = 1 
			AND a.OTStartTime IS NOT NULL 
			AND a.OTEndTime IS NOT NULL
	
		UNION 

		--Extra Pay
		SELECT a.EmpNo, 
			b.YAALPH AS EmpName, 
			a.DT, 
			'08:00' AS PayHour, 
			480 AS PayMinute,
			'Extra Pay' AS PayDescription,
			4 AS PayOrderNo,
			a.Processed,
			LTRIM(RTRIM(b.YAPGRD)) AS PayGrade 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		WHERE 
			CONVERT(VARCHAR, a.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12)
			AND b.YAPAST IN ('8') 
			AND a.IsLastRow = 1 
			AND a.IsPublicHoliday = 1
			AND a.ShiftCode = 'O'
			AND ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0
	) AS a
	WHERE 
	(
		a.Processed = CASE WHEN @processType = 1 THEN 0 WHEN @processType = 2 THEN 1 END
		OR @processType IS NULL
	)
	ORDER BY a.PayOrderNo, a.DT, a.EmpNo

GO


/*	Debug:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@processType	INT = 0		--(Note: 0 => All; 1 => Not yet processed; 2 => Processed)

	EXEC tas.Pr_GetAspirePayrollReportData_V2 '11/16/2016', '12/15/2016'
	EXEC tas.Pr_GetAspirePayrollReportData_V2 '11/16/2016', '12/15/2016', 0

	EXEC tas.Pr_GetAspirePayrollReportData_V2 '16/02/2016', '15/03/2016', 0

*/

