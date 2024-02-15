USE [Gen_Purpose]
GO

/****** Object:  View [genuser].[EmployeeMaster]    Script Date: 22/09/2020 11:16:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/************************************************************************************************************

View Name				:	GenUser.EmployeeMaster
Description				:	This view returns all individual employees and group accounts.
							This is widely use in getting employee information.

Created By				:	Noel G. Francisco
Date Created			:	17 October 2007

Column Names
	EmpNo				:	(int) Employee No.
	EmpName				:	(varchar 40) Employee Name
	Cost Center			:	(varrchar 12) Cost Center of the employee
	WorkCostCenter		:	(varchar 12) Cost Center where the employee is currently working
	Company				:	(varchar 5) Company code of the Cost Center
	SupervisorNo		:	(int) Employee No. of the employee's supervisor
	TelephoneExt		:	(int) Telephone Extension of the employee
	Status				:	(char 1) Current Employment Status of the employee, active if numeric otherwise not
	GroupType			:	(varchar 3) Determines if Individual (E) or Group Type (UG)
	DateOfBirth			:	(datetime) The date of birth of the employee, NULL if GroupType is UG
	Gender				:	(varchar 1) Gender of the employee, NULL if GroupType is UG
	Destination			:	(varchar 3) Destination code or city of origin of the employee, NULL if GroupType is UG
	PayGrade			:	(int) Pay Grade level of the employee, 0 if GroupType is UG
	EmpClass			:	(varchar 3) Employee Class (E - Easterner, W - Westerner, B - Bahraini)
	TicketClass			:	(varchar 5) Ticket Class availment of the employee
	EmpPositionID		:	(varchar 8) Employee Position ID
	EmpPositionDesc		:	(varchar 30) Employee Position description

Revision History:
	1.0					NG					2007.10.17 12:13
	Created

	2.0					NGF					2008.02.26 08:33
	Added the employee position id and description

	2.1					NGF					2008.08.30 12:52
	Modified the reference for the Position ID from YAPOS to YAJBCD

	2.2					NGF					2012.05.30 14:57
	Updated the status in case the termination date is still greater than the current date

	2.3					Ervin				2014.03.03 14:10
	Set the employees to active when the c.YAPAST = 'I'

	2.4					Ervin				2014.07.13 10:10
	Fixed the following bug: Conversion failed when converting the varchar value 'E1' to data type int. 

	2.5					Ervin				2014.07.13 10:10
	Added condition to return 0 for employee pay status if YAPAST = 'A' (Aspire Employees)

	2.6					Shoukhat			2015.11.25 09:10
	Fetch the working cost center into the "Cost Center" field. Added "Actual_CostCenter" field

	2.7					Shoukhat			2016.09.05 09:10
	Modified the paygrade fetching to make 0 instead of -1

	2.8					Ervin				2016.09.20 14:35
	Added a condition that sets the PayGrade to 1 if employee no. between 10002000 and 10002999 and value of "YAPGRD" is empty string. This applies to Employee Exit Pass

	2.9					Ervin				2016.09.27 14:09
	Set Pay Grade to 1 for all Aspire employees

	3.0					Shoukhat			17-Oct-2017 09:30 AM
	Added email

	3.1					Ervin				12-Feb-2020 09:00 AM
	Added condition for ABAT1 = 'X' which refers to the contractors who have assigned contract expiry date
******************************************************************************************************************************************************************************************/

ALTER VIEW [genuser].[EmployeeMaster]
AS

	SELECT 
		CAST(a.ABAN8 AS INT) AS EmpNo, LTRIM(RTRIM(a.ABALPH)) AS EmpName,
		CASE WHEN ISNULL(b.WorkingBusinessUnit, '') <> ''
				THEN LTRIM(RTRIM(b.WorkingBusinessUnit))
				ELSE
					CASE WHEN a.ABAT1 = 'E' THEN LTRIM(RTRIM(c.YAHMCU))
						WHEN a.ABAT1 IN ('UG', 'X') THEN LTRIM(RTRIM(a.ABMCU)) 
					END
		END AS CostCenter,
		LTRIM(RTRIM(b.WorkingBusinessUnit)) AS WorkCostCenter,
		LTRIM(RTRIM(ISNULL(c.YAHMCO, ''))) AS Company, CAST(c.YAANPA AS INT) AS SupervisorNo,
		0 AS TelephoneExt,
		CASE WHEN (c.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(c.YADT) OR UPPER(LTRIM(RTRIM(c.YAPAST))) IN ('I', 'A')) 
			 THEN '0' ELSE c.YAPAST 
			 END AS [Status],
		UPPER(a.ABAT1) AS GroupType,
		CASE WHEN c.YADOB IS NOT NULL THEN tas.ConvertFromJulian(c.YADOB) ELSE NULL END AS DateOfBirth,
		UPPER(c.YASEX) AS Gender,
		UPPER(LTRIM(RTRIM(c.YAP019))) AS Destination,
		
		--CASE WHEN ISNUMERIC(c.YAPGRD) = 1 THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(c.YAPGRD, '0')))) ELSE -1 END AS PayGrade,
		--ver 2.7
		CASE WHEN ISNUMERIC(ISNULL(c.YAPGRD, '0')) = 1 THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(c.YAPGRD, '0')))) 
			WHEN c.YAPGRD = '' AND CAST(a.ABAN8 AS INT) BETWEEN 10002000 AND 10002999 THEN 1	--Rev. #2.8
			WHEN ISNUMERIC(ISNULL(c.YAPGRD, '0')) = 0 AND LTRIM(RTRIM(c.YAHMCU)) = '7920' THEN 1	--Rev. #2.9
			ELSE 0 
		END AS PayGrade,
		--***

		UPPER(LTRIM(RTRIM(ISNULL(c.YAP002, '')))) AS EmpClass,
		CASE
			WHEN UPPER(LTRIM(RTRIM(c.YAP002))) = 'B' THEN
				CASE
					WHEN CASE WHEN ISNUMERIC(c.YAPGRD) = 1 THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(c.YAPGRD, '0')))) ELSE 0 END >= 12 THEN 'BC'
					ELSE 'EX'
				END
			ELSE ISNULL(d.TCTicketClass, '')
		END AS TicketClass,
		LTRIM(RTRIM(ISNULL(c.YAJBCD, ''))) AS EmpPositionID,
		LTRIM(RTRIM(ISNULL(e.DRDL01, ''))) AS EmpPositionDesc,
	    CASE WHEN a.ABAT1 = 'E' THEN LTRIM(RTRIM(c.YAHMCU))
				WHEN a.ABAT1 IN ('UG', 'X') THEN LTRIM(RTRIM(a.ABMCU)) 
			END AS Actual_CostCenter,
		LTRIM(RTRIM(g.EAEMAL)) AS EmpEmail,
		CASE WHEN ISNULL(h.T3EFT, 0) = 0 
			THEN tas.ConvertFromJulian(ISNULL(c.YADST, 0)) 
			ELSE tas.ConvertFromJulian(h.T3EFT) 
		END AS DateJoined,
		tas.ConvertFromJulian(c.YADT) AS DateResigned
	FROM JDE_PRODUCTION.PRODDTA.F0101 a WITH (NOLOCK) 
		LEFT OUTER JOIN tas2.tas.Master_EmployeeAdditional b WITH (NOLOCK) ON CAST(a.ABAN8 AS INT) = b.EmpNo 
		LEFT OUTER JOIN JDE_PRODUCTION.PRODDTA.F060116 c WITH (NOLOCK) ON a.ABAN8 = c.YAAN8 
		LEFT JOIN genuser.TicketClass AS d WITH (NOLOCK) ON UPPER(LTRIM(RTRIM(c.YAP002))) = d.TCEmpClass AND 
			CASE WHEN ISNUMERIC(c.YAPGRD) = 1 THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(c.YAPGRD, '0')))) ELSE 0 END = d.TCPayGrade -- ver 2.7
		LEFT JOIN JDE_PRODUCTION.PRODCTL.F0005 AS e WITH (NOLOCK) ON LTRIM(RTRIM(c.YAJBCD)) = LTRIM(RTRIM(e.DRKY)) AND e.DRSY = '06' AND e.DRRT = 'G'
		LEFT JOIN JDE_PRODUCTION.PRODDTA.F01151 g WITH (NOLOCK) ON a.ABAN8 = g.EAAN8 AND g.EAIDLN = 0 AND g.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(g.EAETP))) = 'E'
		LEFT JOIN genuser.F00092 h WITH (NOLOCK) ON c.YAAN8 = h.T3SBN1 AND LTRIM(RTRIM(h.T3TYDT)) = 'WH' AND LTRIM(RTRIM(h.T3SDB)) = 'E'
	WHERE 
		(
			(a.ABAT1 = 'E' AND a.ABAN8 > 10000000) OR a.ABAT1 IN ('UG', 'X')
		) 	

/*	Debug:

	SELECT * FROM genuser.EmployeeMaster

*/


GO


