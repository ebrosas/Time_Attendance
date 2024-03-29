USE [ServiceMgmt]
GO
/****** Object:  StoredProcedure [serviceuser].[Pr_GetMIRDetail]    Script Date: 30/09/2017 13:54:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/************************************************************************************************************************************************
*	Revision History
*
*	Name: serviceuser.Pr_GetAIRDetail
*	Description: Retrieves information about the Medical Incident Report
*
*	Date			Author		Rev.#		Comments
*	21/10/2015		Ervin		1.0			Created
*	15/11/2015		Ervin		1.1			Added "TotalDaysLost"
*	14/03/2017		Ervin		1.2			Added condition to filter records by Accident Type
*	28/03/2017		Ervin		1.3			Added new fields for the implementation of the workflow
*	30/03/2017		Ervin		1.4			Added "@assignTypeID" and "@assignedToEmpNo" parameters
************************************************************************************************************************************************/

ALTER PROCEDURE [serviceuser].[Pr_GetMIRDetail]
(
	@MIRRequestNo				int = 0,
	@serviceRequestNo			int = 0,
	@statusHandlingCode			varchar(50) = '',
	@empNo						int = 0,
	@empName					varchar(100) = null,
	@costCenter					varchar(12) = null,
	@startDateReported			datetime = null,
	@endDateReported			datetime = null,
	@startDateSubmitted			datetime = null,
	@endDateSubmitted			datetime = null,
	@draftTypeID				tinyint = 0,		--(Note: 0 = All; 1 = Draft Request; 2 = Submitted Request)	
	@createdByTypeID			tinyint = 0,		--(Note: 0 = All; 1 = Me; 2 = Others)
	@userEmpNo					int = 0,
	@accidentTypeArray			VARCHAR(200) = '',
	@assignTypeID				TINYINT = 0,		--(Note: 1 = All; 2 = Me; 3 = Others)
	@assignedToEmpNo			INT = 0
)	
AS

	--Declare constants
	DECLARE @CONST_MINOR					VARCHAR(10),
			@CONST_ILL_HEALTH				VARCHAR(10),
			@CONST_LTI						VARCHAR(10),
			@CONST_RESTRCITED_WORK_AREA		VARCHAR(10),
			@CONST_FATALITY					VARCHAR(10),
			@CONST_NON_INDUSTRIAL			VARCHAR(10),
			@CONST_FIRST_AID_CASES			VARCHAR(10),
			@CONST_OTHERS					VARCHAR(10),
			@CONST_APPCODE					VARCHAR(10)

	--Initialize constants
	SELECT	@CONST_MINOR					= 'ICMINOR',
			@CONST_ILL_HEALTH				= 'ICILHEALTH',
			@CONST_LTI						= 'ICLTI',
			@CONST_RESTRCITED_WORK_AREA		= 'ICRWA',
			@CONST_FATALITY					= 'ICFATAL',
			@CONST_NON_INDUSTRIAL			= 'ICNONINDTL',
			@CONST_FIRST_AID_CASES			= 'ICFIRSTAID',
			@CONST_OTHERS					= 'ICOTHERS',
			@CONST_APPCODE					= 'GRMSMS'


	--Validate parameters
	IF ISNULL(@MIRRequestNo, 0) = 0
		SET @MIRRequestNo = NULL

	IF ISNULL(@serviceRequestNo, 0) = 0
		SET @serviceRequestNo = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@empName, '') = ''
		SET @empName = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDateReported, '') = ''
		SET @startDateReported = NULL

	IF ISNULL(@endDateReported, '') = ''
		SET @endDateReported = NULL

	IF ISNULL(@startDateSubmitted, '') = ''
		SET @startDateSubmitted = NULL

	IF ISNULL(@endDateSubmitted, '') = ''
		SET @endDateSubmitted = NULL

	IF ISNULL(@userEmpNo, 0) = 0
		SET @userEmpNo = NULL

	IF ISNULL(@draftTypeID, 0) = 0
		SET @draftTypeID = NULL

	IF ISNULL(@createdByTypeID, 0) = 0
		SET @createdByTypeID = NULL

	IF ISNULL(@statusHandlingCode, '') = '' 
		SET @statusHandlingCode = NULL

	IF ISNULL(@accidentTypeArray, '') = ''
		SET @accidentTypeArray = NULL 

	IF ISNULL(@assignTypeID, 0) = 0
		SET @assignTypeID = NULL

	SELECT 
		a.MIRRequestNo,
		a.ServiceRequestNo,
		a.DateReported,
		a.TimeReported,
		a.TimeEventOccured,
		a.EmpNo,
		b.EmpName,
		LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS EmpPosition,
		RTRIM(b.BusinessUnit) AS EmpCostCenter,
		RTRIM(c.BusinessUnitName) AS EmpCostCenterName,
		d.YAANPA as EmpSupervisorNo,
		f.EmpName as EmpSupervisorName,
		c.CostCenterManager AS EmpManagerNo,
		g.EmpName as EmpManagerName,
		UPPER(ISNULL(a.ContractorName, '')) AS ContractorName,
		UPPER(ISNULL(a.ContractorOccupation, '')) AS ContractorOccupation,
		a.ContractorSupervisorEmpNo,
		h.EmpName AS ContractorSupervisorEmpName,
		a.ExactLocation,
		a.CostCenter,
		RTRIM(i.BusinessUnitName) AS CostCenterName,
		a.OtherDepartment,
		a.AccidentDescription,
		a.InjuryDescription,
		a.IsMinor,
		a.IsLTI,
		a.IsFatality,
		a.IsFirstAidCase,
		a.IsIllHealth,
		a.IsRestrictedWorkArea,
		a.IsNonIndustrial,
		a.IsOtherInjuryClass,
		a.OtherInjuryClassDesc,
		a.IsTraumaticAmputation,
		a.IsForeignObject,
		a.IsOtherForeignObject,
		a.OtherForeignObjectDesc,
		a.IsHeadScalp,
		a.IsBothLegs,
		a.IsRightWrist,
		a.IsAsphyxia,
		a.IsNeck,
		a.IsRightLeg,
		a.IsLeftWrist,
		a.IsBurn,
		a.IsBothEyes,
		a.IsLeftLeg,
		a.IsForehead,
		a.IsContusion,
		a.IsRightEye,
		a.IsBothKnees,
		a.IsBothElbows,
		a.IsWound,
		a.IsLeftEye,
		a.IsRightKnee,
		a.IsRightElbow,
		a.IsFracture,
		a.IsChest,
		a.IsLeftKnee,
		a.IsLeftElbow,
		a.IsSkinIrritation,
		a.IsMouthNose,
		a.IsBothAnkles,
		a.IsFinger,
		a.IsDislocation,
		a.IsBothArms,
		a.IsRightAnkle,
		a.IsAbdomen,
		a.IsElectricShock,
		a.IsRightArm,
		a.IsLeftAnkle,
		a.IsGroin,
		a.IsHeatExhaustion,
		a.IsLeftArm,
		a.IsBothHips,
		a.IsBothFeet,
		a.IsHernia,
		a.IsBack,
		a.IsRightHip,
		a.IsRightFoot,
		a.IsInflammation,
		a.IsUpperBack,
		a.IsLeftHip,
		a.IsLeftFoot,
		a.IsSprain,
		a.IsMidBack,
		a.IsBothHands,
		a.IsToe,
		a.IsMultipleInjuries,
		a.IsLowerBack,
		a.IsRightHand,
		a.IsShoulders,
		a.IsPuncture,
		a.IsLeftEar,
		a.IsLeftHand,
		a.IsOtherNatureInjury,
		a.IsSoftTissueInjury,
		a.IsRightEar,
		a.IsBothWrists,
		a.OtherNatureDesc,
		a.CreatedDate,
		a.CreatedByEmpNo,
		a.CreatedByUser,
		a.CreatedByEmpName,
		a.CreatedByPhoneExt,
		a.CreatedByUserEmail,

		--LTRIM(RTRIM(ISNULL(j.EAEMAL, ''))) AS CreatorEmail,
		RTRIM(CreatedByUserEmail) AS CreatorEmail,

		a.LastUpdateTime,
		a.LastUpdateEmpNo,
		a.LastUpdateUser,
		a.LastUpdateEmpName,
		a.IsDraft,
		a.SubmittedDate,
		a.IsClosed,
		a.ClosedDate,
		a.StatusID,
		a.StatusCode,
		a.StatusDesc,
		a.StatusHandlingCode,
		a.TotalDaysLost,
		a.CurrentlyAssignedEmpNo,
		a.CurrentlyAssignedEmpName,
		a.CurrentlyAssignedEmpEmail,
		a.ServiceProviderTypeCode,
		DistListCode
	FROM serviceuser.MIRRequest a
		LEFT JOIN serviceuser.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
		LEFT JOIN serviceuser.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
		LEFT JOIN serviceuser.syJDE_F060116 d on a.EmpNo = d.YAAN8
		LEFT JOIN serviceuser.syJDE_F08001 e on LTRIM(RTRIM(d.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
		LEFT JOIN serviceuser.Master_Employee_JDE_View f on d.YAANPA = f.EmpNo
		LEFT JOIN serviceuser.Master_Employee_JDE_View g on c.CostCenterManager = g.EmpNo
		LEFT JOIN serviceuser.Master_Employee_JDE_View h on a.ContractorSupervisorEmpNo = h.EmpNo
		LEFT JOIN serviceuser.Master_BusinessUnit_JDE i ON LTRIM(RTRIM(a.CostCenter)) = LTRIM(RTRIM(i.BusinessUnit))
		--LEFT JOIN serviceuser.syJDE_F01151 j ON a.CreatedByEmpNo = j.EAAN8 AND j.EAIDLN = 0 AND j.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(j.EAETP))) = 'E'
	WHERE 
		(a.MIRRequestNo = @MIRRequestNo OR @MIRRequestNo IS NULL)
		AND (a.ServiceRequestNo = @serviceRequestNo OR @serviceRequestNo IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND 
		(	
			RTRIM(b.EmpName) LIKE '%' + RTRIM(@empName) + '%' 
			OR
			RTRIM(a.ContractorName) LIKE '%' + RTRIM(@empName) + '%' 
			OR
			@empName IS NULL
		)
		AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		AND 
		(
			CONVERT(DATETIME, CONVERT(VARCHAR, a.DateReported, 12)) BETWEEN @startDateReported AND @endDateReported
			OR 
			(@startDateReported IS NULL AND @endDateReported IS NULL)
		)
		AND 
		(
			CONVERT(DATETIME, CONVERT(VARCHAR, a.SubmittedDate, 12)) BETWEEN @startDateSubmitted AND @endDateSubmitted
			OR 
			(@startDateSubmitted IS NULL AND @endDateSubmitted IS NULL)
		)
		AND 
		(
			(a.IsDraft = 1 AND @draftTypeID = 1)
			OR
			(ISNULL(a.IsDraft, 0) = 0 AND @draftTypeID = 2)
			OR
			@draftTypeID IS NULL
		)
		AND 
		(
			(
				@createdByTypeID = 1	--Created by Me
				AND
				@userEmpNo > 0 AND a.CreatedByEmpNo = @userEmpNo
			)
			OR
			(
				@createdByTypeID = 2	--Created by Others
				AND
				@userEmpNo > 0 AND a.CreatedByEmpNo <> @userEmpNo
			)
			OR
			@createdByTypeID IS NULL
		)
		AND (RTRIM(a.StatusHandlingCode) = @statusHandlingCode OR @statusHandlingCode IS NULL)
		AND	--Rev. #1.2
		(
			(PATINDEX('%' + @CONST_MINOR + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsMinor = 1)
			OR
			(PATINDEX('%' + @CONST_ILL_HEALTH + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsIllHealth = 1)
			OR
			(PATINDEX('%' + @CONST_LTI + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsLTI = 1)
			OR
			(PATINDEX('%' + @CONST_RESTRCITED_WORK_AREA + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsRestrictedWorkArea = 1)
			OR
			(PATINDEX('%' + @CONST_FATALITY + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsFatality = 1)
			OR
			(PATINDEX('%' + @CONST_NON_INDUSTRIAL + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsNonIndustrial = 1)
			OR
			(PATINDEX('%' + @CONST_FIRST_AID_CASES + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsFirstAidCase = 1)
			OR
			(PATINDEX('%' + @CONST_OTHERS + '%', UPPER(RTRIM(@accidentTypeArray))) > 0 AND a.IsOtherInjuryClass = 1)
			OR @accidentTypeArray IS NULL
		)
		AND
		(
			@assignTypeID = 2 AND a.CurrentlyAssignedEmpNo = @userEmpNo AND RTRIM(a.StatusCode) <> '02'		--Me
			OR
            (
				@assignTypeID = 3 AND RTRIM(a.StatusCode) <> '02'	--Others 
				AND 
				(
					@assignedToEmpNo > 0 AND a.CurrentlyAssignedEmpNo = @assignedToEmpNo	
					OR
                    (
						ISNULL(@assignedToEmpNo, 0) = 0 AND a.CurrentlyAssignedEmpNo <> @userEmpNo
						AND RTRIM(a.CostCenter) IN
						(
							SELECT RTRIM(PermitCostCenter) FROM serviceuser.sy_PermitCostCenter a
								INNER JOIN serviceuser.sy_UserDefinedCode b on a.PermitAppID = b.UDCID
							WHERE RTRIM(b.UDCCode) = @CONST_APPCODE
								AND PermitEmpNo = @userEmpNo
						)
					)
				)
			)	
			OR
            (
				@assignTypeID = 1 AND RTRIM(a.StatusCode) <> '02'	--All
				AND
                (
					(@userEmpNo > 0 AND a.CurrentlyAssignedEmpNo = @userEmpNo)
					OR
                    RTRIM(a.CostCenter) IN
					(
						SELECT RTRIM(PermitCostCenter) FROM serviceuser.sy_PermitCostCenter a
							INNER JOIN serviceuser.sy_UserDefinedCode b on a.PermitAppID = b.UDCID
						WHERE RTRIM(b.UDCCode) = @CONST_APPCODE
							AND PermitEmpNo = @userEmpNo
					)
				)
			)
			OR @assignTypeID IS NULL
		)
	ORDER BY a.MIRRequestNo DESC

