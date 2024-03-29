USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetActionMember_WithSubstitute]    Script Date: 01/08/2017 14:46:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetActionMember_WithSubstitute
*	Description: Retrieves the workflow action member 
*
*	Date:			Author:		Rev. #:		Comments:
*	23/09/2015		Ervin		1.0			Created
*	29/09/2015		Ervin		1.1			Used WFSWIPE workflow request type
*	05/10/2015		Ervin		1.2			Get the substitute in the Leave Requisition System instead of the Common Admin System
*	1/03/2016		Ervin		1.3			Refactored the code to change WFTAS request type to WFSWIPE
*******************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetActionMember_WithSubstitute]
(
	@distListCode varchar(10),
	@costCenter varchar(12),
	@empNo int 
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo int,
	EmpName varchar(50),
	EmpEmail varchar(50)
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
			@CONST_WFSWIPE			varchar(10)
		
	SELECT	@actionMemberEmpNo = 0, 
			@DistListID = 0,
			@serviceProviderEmpNo = 0,
			@SubstituteEmpNo = 0,
			@DistMemID = 0,
			@AppCode = 'TAS',
			@WFSubstituteEmpNo = 0,
			@WFSubstituteEmpName = '',
			@WFSubstituteEmpEmail = '',
			@CONST_WFSWIPE = 'WFSWIPE'
	
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

				--Search for active substitute defined in the "Workflow Substitute Settings" form in ISMS
				IF EXISTS 
				(
					SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter)
				)
				BEGIN

					SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
							@WFSubstituteEmpName = SubstituteEmpName,
							@WFSubstituteEmpEmail = SubstituteEmpEmail
					FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter)
				END

				IF @WFSubstituteEmpNo > 0		
					SET @actionMemberEmpNo = @WFSubstituteEmpNo

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

						--The Superintendent is on-leave, so get the substitute
						--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(0, @costCenter, @AppCode)

						--The Superintendent is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
						SELECT @SubstituteEmpNo = SubEmpNo
						FROM tas.syJDE_LeaveRequisition 
						WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
							AND EmpNo = @serviceProviderEmpNo
							AND RTRIM(LeaveType) = 'AL' 
							AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

						IF (@SubstituteEmpNo > 0)	
						BEGIN
									
							SELECT @actionMemberEmpNo = @SubstituteEmpNo
							FROM tas.Master_Employee_JDE_View a
								INNER JOIN tas.Master_BusinessUnit_JDE b ON rtrim(a.BusinessUnit) = rtrim(b.BusinessUnit)
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

				--Search for active substitute defined in the "Workflow Substitute Settings" form in ISMS
				IF EXISTS (SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter))
				BEGIN

					SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
							@WFSubstituteEmpName = SubstituteEmpName,
							@WFSubstituteEmpEmail = SubstituteEmpEmail
					FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter)
				END
			END

			IF @WFSubstituteEmpNo > 0		
				SET @actionMemberEmpNo = @WFSubstituteEmpNo

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

					--The Cost Center Manager is on-leave, so get the substitute
					--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(1,@costCenter,@AppCode)

					--The Cost Center Manager is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
					SELECT @SubstituteEmpNo = SubEmpNo
					FROM tas.syJDE_LeaveRequisition 
					WHERE RTRIM(RequestStatusSpecialHandlingCode) = 'Closed' 
						AND EmpNo = @serviceProviderEmpNo
						AND RTRIM(LeaveType) = 'AL' 
						AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN LeaveStartDate and LeaveEndDate

					IF (@SubstituteEmpNo > 0)	
					BEGIN
									
						SELECT @actionMemberEmpNo = @SubstituteEmpNo
						FROM tas.Master_Employee_JDE_View a
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
		END
	END

	ELSE 
	BEGIN

		SELECT TOP 1 @DistListID = DistListID 
		FROM tas.syJDE_DistributionList
		WHERE UPPER(RTRIM(DistListCode)) = UPPER(RTRIM(@distListCode))

		IF @DistListID > 0 
		BEGIN

			--Get the Service Provider employee info
			SELECT TOP 1 @serviceProviderEmpNo = ISNULL(DistMemEmpNo,0), 
						@DistMemID = DistMemID
			FROM tas.syJDE_DistributionMember
			WHERE DistMemDistListID = @DistListID

			IF @serviceProviderEmpNo > 0
			BEGIN

				--Search for active substitute defined through the "Workflow Substitute Settings" form in ISMS
				IF EXISTS (SELECT SubstituteSettingID FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter))
				BEGIN

					SELECT	@WFSubstituteEmpNo = SubstituteEmpNo, 
							@WFSubstituteEmpName = SubstituteEmpName,
							@WFSubstituteEmpEmail = SubstituteEmpEmail
					FROM tas.fnGetActiveSubstitute(@serviceProviderEmpNo, @CONST_WFSWIPE, @costCenter)
				END
			END

			IF @WFSubstituteEmpNo > 0		
				SET @actionMemberEmpNo = @WFSubstituteEmpNo

			ELSE
			BEGIN

				--Check if the Service Provider is on-leave
				IF EXISTS 
				(
					SELECT EmpNo FROM tas.Vw_EmployeeAvailability
					WHERE EmpNo = @serviceProviderEmpNo 
						AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) BETWEEN FromDate and ToDate
				)
				BEGIN

					--The service provider is on-leave, so get the substitute.
					--SELECT @SubstituteEmpNo = tas.fnGetSubstituteEmpNo(2, convert(varchar(12), @DistMemID), @AppCode)

					--The Cost Center Manager is on-leave, so get the substitute from the Leave Requisition	System (Rev. #1.2)
					SELECT @SubstituteEmpNo = SubEmpNo
					FROM tas.syJDE_LeaveRequisition 
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
	END
			
	IF @actionMemberEmpNo > 0
	BEGIN

		--Get the employee info
		SELECT	@empName = RTRIM(EmpName),
				@empEmail = LTRIM(RTRIM(b.EAEMAL))
		FROM tas.Master_Employee_JDE_View a
			LEFT JOIN tas.syJDE_F01151 b ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
		WHERE EmpNo = @actionMemberEmpNo
	END

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@actionMemberEmpNo, @empName, @empEmail
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END

