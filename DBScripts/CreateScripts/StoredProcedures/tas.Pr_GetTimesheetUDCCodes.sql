/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetUDCCodes
*	Description: Get list items of various system wide User-defined codes 
*
*	Date			Author		Revision No.	Comments:
*	17/07/2016		Ervin		1.0				Created
*	20/12/2016		Ervin		1.1				Modified the WHERE clause in fetching the Timesheet correction codes
*	01/01/2017		Ervin		1.2				Added Leave Types
*	02/01/2017		Ervin		1.3				Added Religion Codes, Group Codes and Supplier List
*	05/01/2017		Ervin		1.4				Added Company Codes
*	28/02/2017		Ervin		1.5				Added Shift Codes
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetUDCCodes
(   
	@actionType		TINYINT
)
AS

	IF @actionType = 1			--Absent Reason Codes
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = 'RA'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 2		--Timesheet Correction Codes
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = 'T0'
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'AO%' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'RN%' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'ASNS' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'RSES' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'MA%' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'RD%' 
			--AND LTRIM(RTRIM(a.DRKY)) NOT LIKE 'MO%' 
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 3		--Overtime Types
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = 'OT'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 4		--DIL Types
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = '1'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 5		--Leave Types
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = 'LV'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 6		--Religion Codes
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '06'
			AND LTRIM(RTRIM(a.DRRT)) = 'M'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 7		--Supplier List
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.ABAN8)) AS UDCCode,
				LTRIM(RTRIM(a.ABALPH)) AS UDCDesc1, 
				'' AS UDCDesc2
		FROM tas.syJDE_F0101 a
		WHERE LTRIM(RTRIM(a.ABAT1)) = 'V'
		ORDER BY LTRIM(RTRIM(a.ABALPH))
	END 

	ELSE IF @actionType = 8		--Group Codes
	BEGIN
    
		SELECT	LTRIM(RTRIM(a.DRKY)) AS UDCCode,
				LTRIM(RTRIM(a.DRDL01)) AS UDCDesc1, 
				LTRIM(RTRIM(a.DRDL02)) AS UDCDesc2
		FROM tas.syJDE_F0005 a
		WHERE LTRIM(RTRIM(a.DRSY)) = '55'
			AND LTRIM(RTRIM(a.DRRT)) = 'CG'
		ORDER BY LTRIM(RTRIM(a.DRDL01))
	END 

	ELSE IF @actionType = 9		--Company Codes
	BEGIN

		SELECT	a.Company AS UDCCode, 
				a.CompanyName AS UDCDesc1,
				'' AS UDCDesc2
		FROM tas.Master_Company_JDE a
		WHERE LTRIM(RTRIM(a.Company)) IN 
			(
				'00100',
				'00600',
				'00333',
				'00777'
			)
		ORDER BY a.Company
	END 

	ELSE IF @actionType = 10	--Shift Codes
	BEGIN

		SELECT	'D' AS UDCCode,
				'Day' AS UDCDesc1,
				'Day shift' AS UDCDesc2

		UNION
        
		SELECT	'M' AS UDCCode,
				'Morning' AS UDCDesc1,
				'Morning shift' AS UDCDesc2

		UNION
        
		SELECT	'E' AS UDCCode,
				'Evening' AS UDCDesc1,
				'Evening shift' AS UDCDesc2

		UNION
        
		SELECT	'N' AS UDCCode,
				'Night' AS UDCDesc1,
				'Night shift' AS UDCDesc2

		UNION
        
		SELECT	'O' AS UDCCode,
				'Off' AS UDCDesc1,
				'Day-off' AS UDCDesc2
	END 

GO 

/*	Debugging:

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY LTRIM(RTRIM(DRKY))

	EXEC tas.Pr_GetTimesheetUDCCodes 1		--Absent Reason Codes
	EXEC tas.Pr_GetTimesheetUDCCodes 2		--Timesheet Correction Codes
	EXEC tas.Pr_GetTimesheetUDCCodes 3		--Overtime Types
	EXEC tas.Pr_GetTimesheetUDCCodes 4		--DIL Types
	EXEC tas.Pr_GetTimesheetUDCCodes 5		--Leave Types
	EXEC tas.Pr_GetTimesheetUDCCodes 6		--Religion Codes
	EXEC tas.Pr_GetTimesheetUDCCodes 7		--Supplier List
	EXEC tas.Pr_GetTimesheetUDCCodes 8		--Group Codes
	EXEC tas.Pr_GetTimesheetUDCCodes 9		--Company Codes
	EXEC tas.Pr_GetTimesheetUDCCodes 10		--Shift Codes

*/


