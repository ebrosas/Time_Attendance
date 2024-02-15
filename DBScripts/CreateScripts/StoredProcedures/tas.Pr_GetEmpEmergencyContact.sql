/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpEmergencyContact
*	Description: Get the emergency contact numbers of an employee
*
*	Date:			Author:		Rev.#		Comments:
*	12/08/2018		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmpEmergencyContact
(
	@loadType		TINYINT,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0,
	@searchString	VARCHAR(100) = '',
	@userEmpNo		INT = 0
)
AS
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
		
	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@searchString, '') = ''
		SET @searchString = NULL

	IF @loadType = 0		--Get employee list
	BEGIN

		DECLARE @userCostCenter		VARCHAR(12),
				@isSystemAdmin		BIT 

		SELECT	@userCostCenter		= '',
				@isSystemAdmin		= 0

		IF @userEmpNo > 0
		BEGIN
        
			--Get the user's cost center
			SELECT @userCostCenter = RTRIM(a.BusinessUnit)
			FROM tas.Master_Employee_JDE a
			WHERE a.EmpNo = @userEmpNo

			SELECT @isSystemAdmin =  a.IsAdmin	
			FROM tas.fnIsTASAdministrator(@userEmpNo) a

			IF @isSystemAdmin = 1
			BEGIN
            
				SELECT	a.EmpNo,
						a.EmpName,
						a.Position,
						a.SupervisorNo,
						RTRIM(c.EmpName) AS SupervisorName,
						a.BusinessUnit,
						b.MCDC AS BusinessUnitName,
						a.Religion,
						a.JobCategory,
						a.Sex,
						a.GradeCode,
						a.PayStatus,
						a.DateJoined,
						a.YearsOfService,
						a.DateOfBirth,
						a.Age,
						a.TelephoneExt,
						a.MobileNo,
						a.TelNo,
						a.FaxNo,
						a.EmpEmail
				FROM tas.Vw_EmployeeDirectory a WITH (NOLOCK)
					LEFT JOIN tas.syJDE_F0006 b WITH (NOLOCK) ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(b.MCMCU))
					LEFT JOIN tas.Master_Employee_JDE_View c WITH (NOLOCK) ON a.SupervisorNo = c.EmpNo
				WHERE 
					a.EmpNo > 10000000
					AND ISNUMERIC(a.PayStatus) = 1
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND
					(
						UPPER(RTRIM(a.EmpName)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.JobCategory)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Religion)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Sex)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Position)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.TelephoneExt)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.MobileNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.TelNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.FaxNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.EmpEmail)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR @searchString IS NULL
					)
				ORDER BY a.BusinessUnit, a.EmpNo
			END
            
			ELSE 
			BEGIN

					SELECT	a.EmpNo,
						a.EmpName,
						a.Position,
						a.SupervisorNo,
						RTRIM(c.EmpName) AS SupervisorName,
						a.BusinessUnit,
						b.MCDC AS BusinessUnitName,
						a.Religion,
						a.JobCategory,
						a.Sex,
						a.GradeCode,
						a.PayStatus,
						a.DateJoined,
						a.YearsOfService,
						a.DateOfBirth,
						a.Age,
						a.TelephoneExt,
						a.MobileNo,
						a.TelNo,
						a.FaxNo,
						a.EmpEmail
				FROM tas.Vw_EmployeeDirectory a WITH (NOLOCK)
					LEFT JOIN tas.syJDE_F0006 b WITH (NOLOCK) ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(b.MCMCU))
					LEFT JOIN tas.Master_Employee_JDE_View c WITH (NOLOCK) ON a.SupervisorNo = c.EmpNo
					OUTER APPLY 
					(
						SELECT @userCostCenter AS CostCenter

						UNION 

						SELECT RTRIM(PermitCostCenter) AS CostCenter
						FROM tas.syJDE_PermitCostCenter a WITH (NOLOCK)
							INNER JOIN tas.syJDE_UserDefinedCode b WITH (NOLOCK) on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = 'TAS3'
							AND PermitEmpNo = @userEmpNo
					) d 
				WHERE 
					a.EmpNo > 10000000
					AND ISNUMERIC(a.PayStatus) = 1
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND
					(
						UPPER(RTRIM(a.EmpName)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.JobCategory)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Religion)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Sex)) LIKE UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.Position)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.TelephoneExt)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.MobileNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.TelNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.FaxNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR
						UPPER(RTRIM(a.EmpEmail)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
						OR @searchString IS NULL
					)
					AND (RTRIM(a.BusinessUnit) = RTRIM(d.CostCenter))
				ORDER BY a.BusinessUnit, a.EmpNo
            END 
		END 
	END 

	ELSE IF @loadType = 1		--Get emergency contact persons list
	BEGIN

		SELECT	a.CPAN8 AS EmpNo,
				a.CPCNLN  AS RelatedPersonID,
				a.CPRALP AS RelatedPersonName,
				a.CPRELY  AS RelationTypeID,
				LTRIM(RTRIM(c.DRDL01)) AS RelationTypeDesc,
				b.WPRCK7  AS LineNumberID,
				b.WPPHTP  AS PhoneNumberType,
				LTRIM(RTRIM(d.DRDL01)) AS PhoneNumberDesc,
				b.WPAR1  AS PhonePrefix,	
				b.WPPH1  AS PhoneNumber		
		FROM tas.sy_F01112 a WITH (NOLOCK)
			INNER JOIN tas.sy_F0115 b WITH (NOLOCK) ON a.CPAN8 = b.WPAN8 AND a.CPCNLN = b.WPCNLN AND  b.WPIDLN = 0
			LEFT JOIN tas.syJDE_F0005 c WITH (NOLOCK) ON LTRIM(RTRIM(c.DRSY)) = '01' AND LTRIM(RTRIM(c.DRRT)) = 'RT' AND LTRIM(RTRIM(a.CPRELY)) = LTRIM(RTRIM(c.DRKY))
			LEFT JOIN tas.syJDE_F0005 d WITH (NOLOCK) ON LTRIM(RTRIM(d.DRSY)) = '01' AND LTRIM(RTRIM(d.DRRT)) = 'PH' AND LTRIM(RTRIM(b.WPPHTP)) = LTRIM(RTRIM(d.DRKY))
		WHERE CAST(a.CPAN8 AS INT) = @empNo
		ORDER BY a.CPCNLN
	END 

GO 

/*	Debug:

PARAMETERS:
	@loadType		TINYINT,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0,
	@searchString	VARCHAR(100) = '',
	@userEmpNo		VARCHAR(12) = 0

	EXEC tas.Pr_GetEmpEmergencyContact 0, '', 0, '', 10003452
	EXEC tas.Pr_GetEmpEmergencyContact 0, '', 0, '', 10001988
	EXEC tas.Pr_GetEmpEmergencyContact 1, '', 10003632

*/