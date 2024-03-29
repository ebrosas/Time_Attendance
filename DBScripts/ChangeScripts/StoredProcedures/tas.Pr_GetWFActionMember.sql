/******************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetWFActionMember
*	Description: This stored procedure is used to get the workflow action member
*
*	Date			Author		Rev.#		Comments
*	29/09/2015		Ervin		1.0			Created
*	29/08/2017		Ervin		1.1			Changed workflow application code from "WFTAS" to "WFSWIPE"
*******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetWFActionMember
(
	@empNo			int,
	@distListCode	varchar(10),
	@costCenter		varchar(12) 
)

AS

	--Declare constants
	DECLARE	@CONST_APPCODE			varchar(10),
			@CONST_WFTAS			varchar(10)

	--Initialize constants
	SELECT	@CONST_APPCODE			= 'TASNEW',
			@CONST_WFTAS			= 'WFSWIPE'		--Rev. #1.1

	--Declare variables
	DECLARE @distListID				int,
			@serviceProviderEmpNo	int,
			@substituteEmpNo		int,
			@distMemID				int,
			@recordCount			int,
			@wfSubstituteEmpNo		int,
			@wfSubstituteEmpName	varchar(50),
			@wfSubstituteEmpEmail	varchar(50),
			@actionMemberEmpNo		int

	--Initialize variables
	SELECT	@distListID				= 0, 
			@serviceProviderEmpNo	= 0,
			@substituteEmpNo		= 0,
			@distMemID				= 0,
			@recordCount			= 0,
			@wfSubstituteEmpNo		= 0,
			@wfSubstituteEmpName	= '',
			@wfSubstituteEmpEmail	= '',
			@actionMemberEmpNo		= 0
	
	SELECT TOP 1 @distListID = DistListID 
	FROM tas.syJDE_DistributionList
	WHERE UPPER(RTRIM(DistListCode)) = UPPER(RTRIM(@distListCode))

	IF ISNULL(@distListID, 0) > 0 
	BEGIN

		/***********************************************************************
			Process Distribution Groups
		***********************************************************************/

		IF UPPER(RTRIM(@costCenter)) <> 'ALL'
		BEGIN

			--Get the Service Provider Emp. No.
			SELECT TOP 1 @serviceProviderEmpNo = DistMemEmpNo, 
						 @distMemID = DistMemID
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = @distListID 

			--Check if there is active substitute defined in the "genuser.WFSubstituteSetting" table 
			IF @serviceProviderEmpNo > 0
			BEGIN
				--Search for active substitute defined through the "Workflow Substitute Settings" form in ISMS
				IF EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter))
				BEGIN
					SELECT	@wfSubstituteEmpNo = SubstituteEmpNo, 
							@wfSubstituteEmpName = SubstituteEmpName,
							@wfSubstituteEmpEmail = SubstituteEmpEmail
					FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
				END
			END

			IF @wfSubstituteEmpNo > 0
			
				SELECT 
					@wfSubstituteEmpNo as EmpNo, 
					a.EmpName, 
					LTRIM(RTRIM(c.EAEMAL)) AS EmpEmail,
					a.BusinessUnit, 
					b.BusinessUnitName,
					LTRIM(a.BusinessUnit) as MCMCU
				FROM tas.Master_Employee_JDE_View a
					INNER JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
					LEFT JOIN tas.syJDE_F01151 c ON a.EmpNo = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
				WHERE EmpNo = @wfSubstituteEmpNo
			
			ELSE
			BEGIN

				--Check if the Service Provider is on-leave
				IF EXISTS (SELECT EmpNo FROM tas.Vw_EmployeeAvailability
					WHERE EmpNo = @serviceProviderEmpNo AND convert(datetime,getdate(),101) BETWEEN FromDate and ToDate)
				BEGIN

					--Service Provider is on-leave, so get the substitute using normal process
					SELECT @substituteEmpNo = tas.fnGetSubstituteEmpNo(2, convert(varchar(12), @distMemID), @CONST_APPCODE)

					IF (@substituteEmpNo > 0)
					
						SELECT 
							@substituteEmpNo as EmpNo, 
							a.EmpName, 
							LTRIM(RTRIM(c.EAEMAL)) AS EmpEmail,
							a.BusinessUnit, 
							b.BusinessUnitName
						FROM tas.Master_Employee_JDE_View a
							INNER JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
							LEFT JOIN tas.syJDE_F01151 c ON a.EmpNo = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
						WHERE 
							EmpNo = @substituteEmpNo

					ELSE 
					
						--Get the original Service Provider
						SELECT TOP 1 
							ISNULL(DistMemEmpNo,0) as EmpNo, 
							ISNULL(b.EmpName,'') as EmpName, 
							LTRIM(RTRIM(d.EAEMAL)) AS EmpEmail,
							ISNULL(b.BusinessUnit,'') as BusinessUnit, 
							ISNULL(c.BusinessUnitName,'') as BusinessUnitName
						FROM tas.syJDE_DistributionMember a
							INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
							LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
							LEFT JOIN tas.syJDE_F01151 d ON b.EmpNo = d.EAAN8 AND d.EAIDLN = 0 AND d.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(d.EAETP))) = 'E' 
						WHERE DistMemDistListID = @distListID 
					
				END
				ELSE 
					--Get the original Service Provider
					SELECT TOP 1 
						ISNULL(DistMemEmpNo,0) as EmpNo, 
						ISNULL(b.EmpName,'') as EmpName, 
						LTRIM(RTRIM(d.EAEMAL)) AS EmpEmail,
						ISNULL(b.BusinessUnit,'') as BusinessUnit, 
						ISNULL(c.BusinessUnitName,'') as BusinessUnitName
					FROM tas.syJDE_DistributionMember a
						INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
						LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
						LEFT JOIN tas.syJDE_F01151 d ON b.EmpNo = d.EAAN8 AND d.EAIDLN = 0 AND d.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(d.EAETP))) = 'E' 
					WHERE DistMemDistListID = @distListID 
				
			END
		END

		ELSE 
		BEGIN

			SELECT @recordCount = count(DistMemEmpNo)
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = @distListID 

			IF @recordCount = 1
			BEGIN

				--Get the Service Provider Emp. No.
				SELECT TOP 1 @serviceProviderEmpNo = DistMemEmpNo, 
							 @distMemID = DistMemID
				FROM tas.syJDE_DistributionMember a
					INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
				WHERE DistMemDistListID = @distListID 

				--Check if there is active substitute defined in the "genuser.WFSubstituteSetting" table 
				IF @serviceProviderEmpNo > 0
				BEGIN

					--Search for active substitute defined through the "Workflow Substitute Settings" form in ISMS
					IF EXISTS (SELECT SubstituteSettingID FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter))
					BEGIN
						SELECT	@wfSubstituteEmpNo = SubstituteEmpNo, 
								@wfSubstituteEmpName = SubstituteEmpName,
								@wfSubstituteEmpEmail = SubstituteEmpEmail
						FROM Gen_Purpose.genuser.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					END										
				END
			
				IF @wfSubstituteEmpNo > 0		
				BEGIN
				
					SELECT	@wfSubstituteEmpNo as EmpNo, 
							a.EmpName, 
							LTRIM(RTRIM(c.EAEMAL)) AS EmpEmail,
							a.BusinessUnit, 
							b.BusinessUnitName,
							LTRIM(a.BusinessUnit) as MCMCU
					FROM tas.Master_Employee_JDE_View a
						INNER JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
						LEFT JOIN tas.syJDE_F01151 c ON a.EmpNo = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
					WHERE EmpNo = @wfSubstituteEmpNo
				END

				ELSE
				BEGIN

					--Check if the Service Provider is on-leave
					IF EXISTS (SELECT EmpNo FROM tas.Vw_EmployeeAvailability
						WHERE EmpNo = @serviceProviderEmpNo AND convert(datetime,getdate(),101) BETWEEN FromDate and ToDate)
					BEGIN

						--Service Provider is on-leave, so get the substitute
						SELECT @substituteEmpNo = tas.fnGetSubstituteEmpNo(2, convert(varchar(12), @distMemID), @CONST_APPCODE)

						IF @substituteEmpNo > 0
						BEGIN

							--Get the Service Provider
							SELECT	@substituteEmpNo as EmpNo, 
									a.EmpName, 
									LTRIM(RTRIM(c.EAEMAL)) AS EmpEmail,
									a.BusinessUnit, 
									b.BusinessUnitName
							FROM tas.Master_Employee_JDE_View a
								INNER JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
								LEFT JOIN tas.syJDE_F01151 c ON a.EmpNo = c.EAAN8 AND c.EAIDLN = 0 AND c.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(c.EAETP))) = 'E' 
							WHERE EmpNo = @substituteEmpNo
						END

						ELSE 
						BEGIN

							--Get the original Service Provider
							SELECT TOP 1
								ISNULL(DistMemEmpNo,0) as EmpNo, 
								ISNULL(b.EmpName,'') as EmpName, 
								LTRIM(RTRIM(d.EAEMAL)) AS EmpEmail,
								ISNULL(b.BusinessUnit,'') as BusinessUnit, 
								ISNULL(c.BusinessUnitName,'') as BusinessUnitName
							FROM tas.syJDE_DistributionMember a
								INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
								LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
								LEFT JOIN tas.syJDE_F01151 d ON b.EmpNo = d.EAAN8 AND d.EAIDLN = 0 AND d.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(d.EAETP))) = 'E' 
							WHERE DistMemDistListID = @distListID 
						END
					END
					
					ELSE 
					BEGIN

						--Get the original Service Provider
						SELECT TOP 1
							ISNULL(DistMemEmpNo,0) as EmpNo, 
							ISNULL(b.EmpName,'') as EmpName, 
							LTRIM(RTRIM(d.EAEMAL)) AS EmpEmail,
							ISNULL(b.BusinessUnit,'') as BusinessUnit, 
							ISNULL(c.BusinessUnitName,'') as BusinessUnitName
						FROM tas.syJDE_DistributionMember a
							INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
							LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
							LEFT JOIN tas.syJDE_F01151 d ON b.EmpNo = d.EAAN8 AND d.EAIDLN = 0 AND d.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(d.EAETP))) = 'E' 
						WHERE DistMemDistListID = @distListID 
					END
				END
			END

			ELSE 
			BEGIN

				--Get the Service Provider
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
						ELSE LTRIM(RTRIM(d.EAEMAL))
						END AS EmpEmail, 
		
					ISNULL(b.BusinessUnit, '') AS BusinessUnit, 
					ISNULL(c.BusinessUnitName, '') AS BusinessUnitName
				FROM tas.syJDE_DistributionMember a
					INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
					LEFT JOIN tas.syJDE_F01151 d ON b.EmpNo = d.EAAN8 AND d.EAIDLN = 0 AND d.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(d.EAETP))) = 'E' 
				WHERE DistMemDistListID = @distListID 
			END
		END
	END

	ELSE
	BEGIN

		/***********************************************************************
			Process Built-in Distribution Group
		***********************************************************************/

		IF RTRIM(@distListCode) = 'CCSUPERDNT'	--Cost Center Superintendent
		BEGIN

			IF ISNULL(@costCenter, '') = '' AND @empNo > 0
			BEGIN

				SELECT @costCenter = RTRIM(b.BusinessUnit)
				FROM tas.syJDE_F060116 a
					INNER JOIN tas.Master_Employee_JDE_View b ON a.YAAN8 = b.EmpNo 
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
				WHERE YAAN8 = @empNo	
			END

			IF ISNULL(@costCenter, '') <> ''
			BEGIN

				--Get the Superintendent
				SELECT @serviceProviderEmpNo = MCAN8 
				FROM tas.External_JDE_F0006 a 
					INNER JOIN tas.Master_Employee_JDE_View b ON a.mcanpa = b.EmpNo 	
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
				WHERE LTRIM(RTRIM(MCMCU)) = LTRIM(RTRIM(@costCenter))

				IF @serviceProviderEmpNo > 0
				BEGIN

					--Search for an active substitute defined in ISMS
					IF EXISTS 
					(
						SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					)
					BEGIN

						SELECT	@wfSubstituteEmpNo = SubstituteEmpNo, 
								@wfSubstituteEmpName = SubstituteEmpName,
								@wfSubstituteEmpEmail = SubstituteEmpEmail
						FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					END

					IF @wfSubstituteEmpNo > 0		
						SET @actionMemberEmpNo = @wfSubstituteEmpNo

					ELSE
					BEGIN

						--Check if Superintendent is on-leave
						IF EXISTS 
						(
							SELECT EmpNo FROM tas.Vw_EmployeeAvailability
							WHERE EmpNo = @serviceProviderEmpNo 
								AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
						)
						BEGIN

							--Superintendent is on-leave, so get the substitute
							SELECT @substituteEmpNo = tas.fnGetSubstituteEmpNo(0, @costCenter, @CONST_APPCODE)

							IF (@substituteEmpNo > 0)	
							BEGIN
									
								SELECT @actionMemberEmpNo = @substituteEmpNo
								FROM tas.Master_Employee_JDE_View a
									INNER JOIN tas.Master_BusinessUnit_JDE b ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
								WHERE EmpNo = @substituteEmpNo
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

					IF @actionMemberEmpNo > 0
					BEGIN

						--Get the employee information
						SELECT	a.EmpNo,
								a.EmpName,
								LTRIM(RTRIM(b.EAEMAL)) AS EmpEmail,
								a.BusinessUnit, 
								c.BusinessUnitName
						FROM tas.Master_Employee_JDE_View a
							LEFT JOIN tas.syJDE_F01151 b ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
							LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(a.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
						WHERE EmpNo = @actionMemberEmpNo
					END
				END
			END						
		END

		ELSE IF RTRIM(@distListCode) = 'CCMANAGER'	--Cost Center Manager
		BEGIN

			IF ISNULL(@costCenter, '') = '' AND @empNo > 0
			BEGIN

				SELECT @costCenter = RTRIM(b.BusinessUnit)
				FROM tas.syJDE_F060116 a
					INNER JOIN tas.Master_Employee_JDE_View b ON a.YAAN8 = b.EmpNo 
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
				WHERE YAAN8 = @empNo	
			END

			IF ISNULL(@costCenter, '') <> ''
			BEGIN

				--Get the Cost Center Manager
				SELECT @serviceProviderEmpNo = MCANPA 
				FROM tas.External_JDE_F0006 a 
					INNER JOIN tas.Master_Employee_JDE_View b ON a.mcanpa = b.EmpNo 	
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
				WHERE LTRIM(RTRIM(MCMCU)) = LTRIM(RTRIM(@costCenter))

				IF @serviceProviderEmpNo > 0
				BEGIN

					--Search for an active substitute defined in ISMS
					IF EXISTS 
					(
						SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					)
					BEGIN

						SELECT	@wfSubstituteEmpNo = SubstituteEmpNo, 
								@wfSubstituteEmpName = SubstituteEmpName,
								@wfSubstituteEmpEmail = SubstituteEmpEmail
						FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFTAS, @costCenter)
					END

					IF @wfSubstituteEmpNo > 0		
						SET @actionMemberEmpNo = @wfSubstituteEmpNo

					ELSE
					BEGIN

						--Check if Cost Center Manager is on-leave
						IF EXISTS 
						(
							SELECT EmpNo FROM tas.Vw_EmployeeAvailability
							WHERE EmpNo = @serviceProviderEmpNo 
								AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
						)
						BEGIN

							--Cost Center Manager is on-leave, so get the substitute
							SELECT @substituteEmpNo = tas.fnGetSubstituteEmpNo(1, @costCenter, @CONST_APPCODE)

							IF (@substituteEmpNo > 0)	
							BEGIN
									
								SELECT @actionMemberEmpNo = @substituteEmpNo
								FROM tas.Master_Employee_JDE_View a
									INNER JOIN tas.Master_BusinessUnit_JDE b ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
								WHERE EmpNo = @substituteEmpNo
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

					IF @actionMemberEmpNo > 0
					BEGIN

						--Get the employee information
						SELECT	a.EmpNo,
								a.EmpName,
								LTRIM(RTRIM(b.EAEMAL)) AS EmpEmail,
								a.BusinessUnit, 
								c.BusinessUnitName
						FROM tas.Master_Employee_JDE_View a
							LEFT JOIN tas.syJDE_F01151 b ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
							LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(a.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
						WHERE EmpNo = @actionMemberEmpNo
					END
				END
			END						
		END
	END

