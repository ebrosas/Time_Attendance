/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetWFActionMemberOvertimeRequest
*	Description: Retrieves the workflow action member 
*
*	Date:			Author:		Rev. #:		Comments:
*	23/08/2017		Ervin		1.0			Created
*	12/09/2017		Ervin		1.1			Added filter to return only active Shift Supervisors
*	15/07/2019		Ervin		1.2			Commented code that get the substitute from ISMS if approver is the Cost Center Manager
*	01/12/2020		Ervin		1.3			Refactored the logic in fetching the action members for "HEADNSUPER" distribution group code
*******************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetWFActionMemberOvertimeRequest
(
	@distListCode	VARCHAR(10),
	@costCenter		VARCHAR(12),
	@empNo			INT,
	@otRequestNo	BIGINT	 
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo			INT,
	EmpName			VARCHAR(50),
	EmpEmail		VARCHAR(50)
)
AS
BEGIN

	DECLARE @myTable TABLE 
	(
		EmpNo int,
		EmpName varchar(50),
		EmpEmail varchar(50)
	)

	--Declare field variables
	DECLARE	@empName varchar(50),
			@empEmail varchar(50)

	--Initialize field variables
	SELECT	@empName	= '',
			@empEmail	= ''

	DECLARE	@actionMemberEmpNo		int,
			@DistListID				int,
			@serviceProviderEmpNo	int,
			@SubstituteEmpNo		int,
			@DistMemID				int,
			@AppCode				varchar(10),
			@WFSubstituteEmpNo		int,
			@WFSubstituteEmpName	varchar(50),
			@WFSubstituteEmpEmail	varchar(50), 
			@CONST_WFTAS			varchar(10),
			@recordCount			INT 
		
	SELECT	@actionMemberEmpNo		= 0, 
			@DistListID				= 0,
			@serviceProviderEmpNo	= 0,
			@SubstituteEmpNo		= 0,
			@DistMemID				= 0,
			@AppCode				= 'TAS3',
			@WFSubstituteEmpNo		= 0,
			@WFSubstituteEmpName	= '',
			@WFSubstituteEmpEmail	= '',
			@CONST_WFTAS			= 'WFSWIPE',
			@recordCount			= 0
	
	IF RTRIM(@distListCode) = 'HEADNSUPER'		--Head of Department and Shift Supervisor
	BEGIN

		IF ISNULL(@costCenter, '') = '' AND @empNo > 0
		BEGIN

			SELECT @costCenter = RTRIM(b.BusinessUnit)
			FROM tas.syJDE_F060116 a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.YAAN8 = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE YAAN8 = @empNo	
		END

		IF ISNULL(@costCenter, '') <> ''
		BEGIN

			--Get the Superintendent
			SELECT @serviceProviderEmpNo = MCAN8 
			FROM tas.External_JDE_F0006 a WITH (NOLOCK) 
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.mcanpa = b.EmpNo 	
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE LTRIM(RTRIM(MCMCU)) = LTRIM(RTRIM(@costCenter))

			IF @serviceProviderEmpNo > 0
			BEGIN

				--Search for active substitute defined in the "Workflow Substitute Settings" form in ISMS
				IF EXISTS 
				(
					SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
				)
				BEGIN

					SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
							@WFSubstituteEmpName = SubstituteEmpName,
							@WFSubstituteEmpEmail = SubstituteEmpEmail
					FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
				END

				IF @WFSubstituteEmpNo > 0		
					SET @actionMemberEmpNo = @WFSubstituteEmpNo

				ELSE
				BEGIN

					--Check if the Superintendent is on-leave
					IF EXISTS 
					(
						SELECT a.MasterLeaveID 
						FROM tas.Master_CurrentLeaves a WITH (NOLOCK)
						WHERE a.EmpNo = @serviceProviderEmpNo
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN a.FromDate AND a.ToDate
					)
					BEGIN

						--The Superintendent is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
						SELECT @SubstituteEmpNo = SubEmpNo
						FROM tas.syJDE_LeaveRequisition WITH (NOLOCK) 
						WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
							AND EmpNo = @serviceProviderEmpNo
							AND RTRIM(LeaveType) = 'AL' 
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

						IF (@SubstituteEmpNo > 0)	
						BEGIN
									
							SELECT @actionMemberEmpNo = @SubstituteEmpNo
							FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
								INNER JOIN tas.Master_BusinessUnit_JDE b WITH (NOLOCK) ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
							WHERE EmpNo = @SubstituteEmpNo
						END
						ELSE 
						BEGIN

							--Get the Superintendent
							SELECT @actionMemberEmpNo = @serviceProviderEmpNo
						END
					END

					ELSE 
					BEGIN
				
						--Get the Superintendent
						SELECT @actionMemberEmpNo = @serviceProviderEmpNo
					END
				END
			END						
		END

		IF @actionMemberEmpNo > 0
		BEGIN

			--Get the employee info
			SELECT	@empName = RTRIM(EmpName),
					@empEmail = LTRIM(RTRIM(b.EAEMAL))
			FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
				LEFT JOIN tas.syJDE_F01151 b WITH (NOLOCK) ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
			WHERE EmpNo = @actionMemberEmpNo

			IF @otRequestNo > 0
			BEGIN
            
				DECLARE	@origEmpNo			INT,
						@origCostCenter		VARCHAR(12),
						@DT					DATETIME,
						@shiftCode			VARCHAR(10)

				--Get the originator employee information
				SELECT	@origEmpNo = a.EmpNo,
						@origCostCenter = RTRIM(a.CostCenter),
						@DT = a.DT,
						@shiftCode = RTRIM(ISNULL(b.Actual_ShiftCode, b.ShiftCode))
				FROM tas.OvertimeRequest a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.TS_AutoID = b.AutoID
				WHERE a.OTRequestNo = @otRequestNo

				--Populate data to the table
				INSERT INTO @myTable  
				SELECT	@actionMemberEmpNo, @empName, @empEmail

				UNION
                
				/***************************************************** Start of Rev. #1.3 **************************************************************************
					Notes:
					- The block of code below will return the Supervisor of the employee with the same Shift Code
					- Additional supervisor is added based on the action member defined in the ff distribution list codes: SUPER5300, SUPER5200, SUPER3250
				****************************************************************************************************************************************************/
				SELECT DISTINCT x.EmpNo, x.EmpName, x.EmpEmail
				FROM
				(
					SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
							LTRIM(RTRIM(a.YAALPH)) AS EmpName,
							LTRIM(RTRIM(ISNULL(c.EAEMAL, ''))) AS EmpEmail,
							CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
								WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
							END AS CostCenter
					FROM tas.syJDE_F060116 a
						LEFT JOIN tas.syJDE_F08001 b on LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.JMJBCD))
						LEFT JOIN tas.syjde_F01151 c ON a.YAAN8 = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
						LEFT JOIN tas.syJDE_F0101 d WITH (NOLOCK) ON a.YAAN8 = d.ABAN8
						INNER JOIN tas.Tran_ShiftPatternUpdates e WITH (NOLOCK) ON CAST(a.YAAN8 AS INT) = e.EmpNo AND e.DateX = @DT
					WHERE 
						LTRIM(RTRIM(ISNULL(b.JMDL01, ''))) LIKE '%' + 'SUPERVISOR' + '%'
						AND ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END) = 1
						AND RTRIM(e.Effective_ShiftCode) = @shiftCode	

					UNION
    
					--Get the Supervisor from 5300 - Electrical Engineering cost center (Rev. #1.4)
					SELECT	a.EmpNo, 
							a.EmpName,
							LTRIM(RTRIM(ISNULL(c.EAEMAL, ''))) AS EmpEmail,
							CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
								WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
							END AS CostCenter
					FROM tas.fnGetActionMember_WithSubstitute_V2('SUPER5300', 'ALL', 0) a
						INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT)
						LEFT JOIN tas.syjde_F01151 c ON b.YAAN8 = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
						LEFT JOIN tas.syJDE_F0101 d WITH (NOLOCK) ON b.YAAN8 = d.ABAN8
						
					UNION
    
					--Get the Supervisor from 5200 - Mechanical Engineering cost center (Rev. #1.4) 
					SELECT	a.EmpNo, 
							a.EmpName,
							LTRIM(RTRIM(ISNULL(c.EAEMAL, ''))) AS EmpEmail,
							CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
								WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
							END AS CostCenter
					FROM tas.fnGetActionMember_WithSubstitute_V2('SUPER5200', 'ALL', 0) a
						INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT)
						LEFT JOIN tas.syjde_F01151 c ON b.YAAN8 = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
						LEFT JOIN tas.syJDE_F0101 d WITH (NOLOCK) ON b.YAAN8 = d.ABAN8

					UNION
    
					--Get the Supervisor from 3250 - Roll Grinding cost center (Rev. #1.4) 
					SELECT	a.EmpNo, 
							a.EmpName,
							LTRIM(RTRIM(ISNULL(c.EAEMAL, ''))) AS EmpEmail,
							CASE WHEN LTRIM(RTRIM(d.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
								WHEN LTRIM(RTRIM(d.ABAT1)) = 'UG' THEN LTRIM(RTRIM(d.ABMCU)) 
							END AS CostCenter
					FROM tas.fnGetActionMember_WithSubstitute_V2('SUPER3250', 'ALL', 0) a
						INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT)
						LEFT JOIN tas.syjde_F01151 c ON b.YAAN8 = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
						LEFT JOIN tas.syJDE_F0101 d WITH (NOLOCK) ON b.YAAN8 = d.ABAN8
				) x
				--INNER JOIN tas.Tran_ShiftPatternUpdates y WITH (NOLOCK) ON x.EmpNo = y.EmpNo AND y.DateX = @DT
				WHERE x.CostCenter = @origCostCenter
					--AND RTRIM(y.Effective_ShiftCode) = @shiftCode	
				/***************************************************** End of Rev. #1.3 **********************************************************************/

				/* Old code commented
				SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
						LTRIM(RTRIM(a.YAALPH)) AS EmpName,
						LTRIM(RTRIM(ISNULL(c.EAEMAL, ''))) AS EmpEmail
				FROM tas.syJDE_F060116 a WITH (NOLOCK)
					LEFT JOIN tas.syJDE_F08001 b WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.JMJBCD))
					LEFT JOIN tas.syjde_F01151 c WITH (NOLOCK) ON a.YAAN8 = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
					INNER JOIN tas.Tran_Timesheet d WITH (NOLOCK) ON CAST(a.YAAN8 AS INT) = d.EmpNo AND d.DT = @DT AND d.IsLastRow = 1
				WHERE 
					LTRIM(RTRIM(ISNULL(b.JMDL01, ''))) LIKE '%' + 'SUPERVISOR' + '%'
					AND RTRIM(d.BusinessUnit) = @origCostCenter
					AND RTRIM(d.ShiftCode) = @shiftCode		--(Note: This condition will filter the Shift Superviors who are in the same shift as the Originator Employee)
					AND ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END) = 1	--Rev. #1.1
				*/
			END
            
			ELSE
            BEGIN 

				--Populate data to the table
				INSERT INTO @myTable  
				SELECT	@actionMemberEmpNo, @empName, @empEmail
			END 
			
			GOTO SKIP_HERE_MULTIPLE_ASSIGNEE						
		END
	END

	ELSE IF RTRIM(@distListCode) = 'CCSUPERDNT'		--Cost Center Superintendent
	BEGIN

		IF ISNULL(@costCenter, '') = '' AND @empNo > 0
		BEGIN

			SELECT @costCenter = RTRIM(b.BusinessUnit)
			FROM tas.syJDE_F060116 a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.YAAN8 = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE YAAN8 = @empNo	
		END

		IF ISNULL(@costCenter, '') <> ''
		BEGIN

			--Get the Superintendent
			SELECT @serviceProviderEmpNo = MCAN8 
			FROM tas.External_JDE_F0006 a WITH (NOLOCK) 
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.mcanpa = b.EmpNo 	
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE LTRIM(RTRIM(MCMCU)) = LTRIM(RTRIM(@costCenter))

			IF @serviceProviderEmpNo > 0
			BEGIN

				--Search for active substitute defined in the "Workflow Substitute Settings" form in ISMS
				IF EXISTS 
				(
					SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
				)
				BEGIN

					SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
							@WFSubstituteEmpName = SubstituteEmpName,
							@WFSubstituteEmpEmail = SubstituteEmpEmail
					FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
				END

				IF @WFSubstituteEmpNo > 0		
					SET @actionMemberEmpNo = @WFSubstituteEmpNo

				ELSE
				BEGIN

					--Check if Superintendent is on-leave
					IF EXISTS 
					(
						SELECT a.MasterLeaveID 
						FROM tas.Master_CurrentLeaves a WITH (NOLOCK)
						WHERE a.EmpNo = @serviceProviderEmpNo
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN a.FromDate AND a.ToDate

						--SELECT EmpNo FROM tas.Vw_EmployeeAvailability
						--WHERE EmpNo = @serviceProviderEmpNo 
						--	AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
					)
					BEGIN

						--The Superintendent is on-leave, so get the substitute
						--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(0, @costCenter, @AppCode)

						--The Superintendent is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
						SELECT @SubstituteEmpNo = SubEmpNo
						FROM tas.syJDE_LeaveRequisition WITH (NOLOCK) 
						WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
							AND EmpNo = @serviceProviderEmpNo
							AND RTRIM(LeaveType) = 'AL' 
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

						IF (@SubstituteEmpNo > 0)	
						BEGIN
									
							SELECT @actionMemberEmpNo = @SubstituteEmpNo
							FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
								INNER JOIN tas.Master_BusinessUnit_JDE b WITH (NOLOCK) ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
							WHERE EmpNo = @SubstituteEmpNo
						END
						ELSE 
						BEGIN

							--Get the Superintendent
							SELECT @actionMemberEmpNo = @serviceProviderEmpNo
						END
					END

					ELSE 
					BEGIN
				
						--Get the Superintendent
						SELECT @actionMemberEmpNo = @serviceProviderEmpNo
					END
				END
			END						
		END
	END

	ELSE IF RTRIM(@distListCode) = 'CCMANAGER'	--Cost Center Manager
	BEGIN

		IF ISNULL(@costCenter, '') = '' AND @empNo > 0
		BEGIN

			SELECT @costCenter = RTRIM(b.BusinessUnit)
			FROM tas.syJDE_F060116 a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.YAAN8 = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE YAAN8 = @empNo	
		END

		IF ISNULL(@costCenter, '') <> '' 
		BEGIN

			--Get the Cost Center Manager
			SELECT @serviceProviderEmpNo = MCANPA 
			FROM tas.External_JDE_F0006 a WITH (NOLOCK) 
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.mcanpa = b.EmpNo 	
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
			WHERE LTRIM(RTRIM(MCMCU)) = LTRIM(RTRIM(@costCenter))

			--Start of Rev. #1.2
			--IF @serviceProviderEmpNo > 0
			--BEGIN

			--	--Search for active substitute defined in the "Workflow Substitute Settings" form in ISMS
			--	IF EXISTS (SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter))
			--	BEGIN

			--		SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
			--				@WFSubstituteEmpName = SubstituteEmpName,
			--				@WFSubstituteEmpEmail = SubstituteEmpEmail
			--		FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
			--	END
			--END

			--IF @WFSubstituteEmpNo > 0		
			--	SET @actionMemberEmpNo = @WFSubstituteEmpNo

			--ELSE
			--BEGIN

				--Check if Cost Center Manager is on-leave
				IF EXISTS 
				(	
					SELECT a.MasterLeaveID 
					FROM tas.Master_CurrentLeaves a WITH (NOLOCK)
					WHERE a.EmpNo = @serviceProviderEmpNo
						AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN a.FromDate AND a.ToDate

					--SELECT EmpNo FROM tas.Vw_EmployeeAvailability
					--WHERE EmpNo = @serviceProviderEmpNo 
					--	AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
				)
				BEGIN

					--The Cost Center Manager is on-leave, so get the substitute
					--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(1,@costCenter,@AppCode)

					--The Cost Center Manager is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
					SELECT @SubstituteEmpNo = SubEmpNo
					FROM tas.syJDE_LeaveRequisition WITH (NOLOCK) 
					WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
						AND EmpNo = @serviceProviderEmpNo
						AND RTRIM(LeaveType) = 'AL' 
						AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

					IF (@SubstituteEmpNo > 0)	
					BEGIN
									
						SELECT @actionMemberEmpNo = @SubstituteEmpNo
						FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
							INNER JOIN tas.Master_BusinessUnit_JDE b ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
						WHERE EmpNo = @SubstituteEmpNo
					END

					ELSE 
					BEGIN

						--Get the Cost Center Manager
						SELECT @actionMemberEmpNo = @serviceProviderEmpNo
					END
				END

				ELSE 
				BEGIN
				
					--Get the Cost Center Manager
					SELECT @actionMemberEmpNo = @serviceProviderEmpNo
				END
			END
		--END
	END

	ELSE 
	BEGIN

		SELECT TOP 1 @DistListID = DistListID 
		FROM tas.syJDE_DistributionList WITH (NOLOCK)
		WHERE UPPER(RTRIM(DistListCode)) = UPPER(RTRIM(@distListCode))

		IF @DistListID > 0 
		BEGIN

			IF UPPER(RTRIM(@costCenter)) <> 'ALL'
			BEGIN
            
				--Get the Service Provider employee info
				SELECT TOP 1 @serviceProviderEmpNo = ISNULL(DistMemEmpNo,0), 
							@DistMemID = DistMemID
				FROM tas.syJDE_DistributionMember WITH (NOLOCK)
				WHERE DistMemDistListID = @DistListID

				IF @serviceProviderEmpNo > 0
				BEGIN

					--Search for active substitute defined through the "Workflow Substitute Settings" form in ISMS
					IF EXISTS (SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter))
					BEGIN

						SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
								@WFSubstituteEmpName = SubstituteEmpName,
								@WFSubstituteEmpEmail = SubstituteEmpEmail
						FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					END
				END

				IF @WFSubstituteEmpNo > 0		
					SET @actionMemberEmpNo = @WFSubstituteEmpNo

				ELSE
				BEGIN

					--Check if the Service Provider is on-leave
					IF EXISTS 
					(
						SELECT a.MasterLeaveID 
						FROM tas.Master_CurrentLeaves a WITH (NOLOCK)
						WHERE a.EmpNo = @serviceProviderEmpNo
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN a.FromDate AND a.ToDate

						--SELECT EmpNo FROM tas.Vw_EmployeeAvailability
						--WHERE EmpNo = @serviceProviderEmpNo 
						--	AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
					)
					BEGIN

						--The service provider is on-leave, so get the substitute.
						--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(2, convert(varchar(12), @DistMemID), @AppCode)

						--The Cost Center Manager is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
						SELECT @SubstituteEmpNo = SubEmpNo
						FROM tas.syJDE_LeaveRequisition WITH (NOLOCK) 
						WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
							AND EmpNo = @serviceProviderEmpNo
							AND RTRIM(LeaveType) = 'AL' 
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

						IF (@SubstituteEmpNo > 0)
							SET @actionMemberEmpNo = @SubstituteEmpNo
						ELSE
							SET @actionMemberEmpNo = @serviceProviderEmpNo
					END
					ELSE 
						SET @actionMemberEmpNo = @serviceProviderEmpNo
				END
			END 

			ELSE
            BEGIN

				SELECT @recordCount = count(DistMemEmpNo)
				FROM tas.syJDE_DistributionMember a WITH (NOLOCK)
					INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.DistMemEmpNo = b.EmpNo 
					LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
				WHERE DistMemDistListID = @DistListID 

				IF @recordCount = 1
				BEGIN
					
					--Get the Service Provider Emp. No.
					SELECT TOP 1 
						@serviceProviderEmpNo = DistMemEmpNo, 
						@DistMemID = DistMemID
					FROM tas.syJDE_DistributionMember a WITH (NOLOCK)
						INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.DistMemEmpNo = b.EmpNo 
						LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON rtrim(ltrim(b.BusinessUnit)) = rtrim(ltrim(c.BusinessUnit))
					WHERE DistMemDistListID = @DistListID 

					--Check if there is active substitute defined in the "genuser.WFSubstituteSetting" table 
					IF @serviceProviderEmpNo > 0
					BEGIN

						--Search for active substitute defined through the "Workflow Substitute Settings" form in ISMS
						IF EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter))
						BEGIN
							SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
									@WFSubstituteEmpName = SubstituteEmpName,
									@WFSubstituteEmpEmail = SubstituteEmpEmail
							FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
						END												
					END
			
					IF @WFSubstituteEmpNo > 0
					BEGIN
                    
						SET @actionMemberEmpNo = @WFSubstituteEmpNo
					END
                    
					ELSE
					BEGIN
						--Check if the Service Provider is on-leave
						IF EXISTS 
						(
							SELECT a.MasterLeaveID 
							FROM tas.Master_CurrentLeaves a WITH (NOLOCK)
							WHERE a.EmpNo = @serviceProviderEmpNo
								AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN a.FromDate AND a.ToDate

							--SELECT EmpNo FROM tas.Vw_EmployeeAvailability
							--WHERE EmpNo = @serviceProviderEmpNo 
							--	AND convert(datetime,getdate(),101) between FromDate and ToDate
						)
						BEGIN

							--Service Provider is on-leave, so get the substitute
							--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(2, convert(varchar(12), @DistMemID), @AppCode)

							--The Service Provider is on-leave, so get the substitute from the Leave Requisition System (Rev. #1.2)
							SELECT @SubstituteEmpNo = SubEmpNo
							FROM tas.syJDE_LeaveRequisition WITH (NOLOCK) 
							WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
								AND EmpNo = @serviceProviderEmpNo
								AND RTRIM(LeaveType) = 'AL' 
								AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

							IF (@SubstituteEmpNo > 0)
								SET @actionMemberEmpNo = @SubstituteEmpNo
							ELSE
								SET @actionMemberEmpNo = @serviceProviderEmpNo
						END
						ELSE 
							SET @actionMemberEmpNo = @serviceProviderEmpNo
					END
                END 

				ELSE
                BEGIN
					--Populate data to the table
					INSERT INTO @myTable  
					SELECT DISTINCT
						CASE WHEN EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							THEN (SELECT SubstituteEmpNo FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							ELSE ISNULL(DistMemEmpNo, 0)
							END AS EmpNo, 

						CASE WHEN EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							THEN (SELECT RTRIM(SubstituteEmpName) FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							ELSE ISNULL(b.EmpName, '')
							END AS EmpName, 

						CASE WHEN EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							THEN (SELECT RTRIM(SubstituteEmpEmail) FROM Gen_Purpose.genuser.fnGetActiveSubstitute(a.DistMemEmpNo, @CONST_WFTAS, RTRIM(b.BusinessUnit)))
							ELSE ISNULL(a.DistMemEmpEmail, '')
							END AS EmpEmail
					FROM tas.syJDE_DistributionMember a
						INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
						LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
					WHERE DistMemDistListID = @DistListID 

					GOTO SKIP_HERE_MULTIPLE_ASSIGNEE
                END 
            END 
		END
	END
			
	IF @actionMemberEmpNo > 0
	BEGIN

		--Get the employee info
		SELECT	@empName = RTRIM(EmpName),
				@empEmail = LTRIM(RTRIM(b.EAEMAL))
		FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
			LEFT JOIN tas.syJDE_F01151 b WITH (NOLOCK) ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
		WHERE EmpNo = @actionMemberEmpNo
	END

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@actionMemberEmpNo, @empName, @empEmail
	
SKIP_HERE_MULTIPLE_ASSIGNEE:

	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END

/*	Debugging:
	
PARAMETERS:
	@distListCode	VARCHAR(10),
	@costCenter		VARCHAR(12),
	@empNo			INT,
	@otRequestNo	BIGINT	 

	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('HEADNSUPER', '3250', 0, 81017)		--Return multiple action member
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('HEADNSUPER', '5200', 0, 80982)		--Return multiple action member
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('HEADNSUPER', '5300', 0, 80350)		--Return multiple action member
	
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('CCMANAGER', '2110', 0, 1)			--Return 1 action member
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('SRPRODMNGR', '', 0)				--Return 1 action member
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('OPGENMNGR', '', 0)				--Return 1 action member
	SELECT * FROM tas.fnGetWFActionMemberOvertimeRequest('VISITADMIN', 'ALL', 0)			--Returns multiple action member

*/

GO


