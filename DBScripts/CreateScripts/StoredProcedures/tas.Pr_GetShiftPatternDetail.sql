/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPatternDetail
*	Description: This stored procedure is used to fetch the shift pattern details 
*
*	Date			Author		Rev.#		Comments:
*	10/06/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftPatternDetail
(   
	@loadType			TINYINT,
	@shiftPatCode		VARCHAR(10) = '',
	@isDayShift			TINYINT = 0,	--(Note: 0 = All; 1 = Not day shift; 2 = Day shift only)
	@isFlexitime		TINYINT = 0		--(Note: 0 = All; 1 = Not flexitime; 2 = Flexitime only)
)
AS
	
	--Validate parameters
	IF ISNULL(@shiftPatCode, '') = ''
		SET @shiftPatCode = NULL

	IF ISNULL(@isDayShift, 0) = 0
		SET @isDayShift = NULL

	IF ISNULL(@isFlexitime, 0) = 0
		SET @isFlexitime = NULL

	IF @loadType = 1		--Get shift timing schedules
	BEGIN
    
		SELECT	b.AutoID,
				a.ShiftPatCode,
				a.ShiftPatDescription,
				a.IsDayShift,
				CAST(CASE WHEN d.SettingID IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsFlexitime,
				b.ShiftCode,
				LTRIM(RTRIM(c.DRDL01)) AS ShiftDescription,
				b.ArrivalFrom,
				b.ArrivalTo,
				b.DepartFrom,
				b.DepartTo,
				CASE WHEN (RTRIM(b.ShiftCode) = 'N' AND CAST(b.ArrivalFrom AS TIME) <= CAST('11:59:00' AS TIME)) OR b.ArrivalTo > b.DepartFrom
					THEN --1440 + DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom)
						CASE WHEN DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom) < 0 THEN 1440 + DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom) ELSE DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom) END 
					ELSE DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom)
				END AS DurationNormalDay,
				b.RArrivalFrom,
				b.RArrivalTo,
				b.RDepartFrom,
				b.RDepartTo,
				CASE WHEN (RTRIM(b.ShiftCode) = 'N'  AND CAST(b.RArrivalFrom AS TIME) <= CAST('11:59:00' AS TIME)) OR b.RArrivalTo > b.RDepartFrom
					THEN --1440 + DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom)
						CASE WHEN DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom) < 0 THEN 1440 + DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom) ELSE DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom) END 
					ELSE DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom)
				END AS DurationRamadanDay,
				DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom) AS RDuration,
				b.CreatedByEmpNo,
				ISNULL(b.CreatedByEmpName, 'System Admin') AS CreatedByEmpName,				
				b.CreatedDate,
				b.LastUpdateEmpNo,
				ISNULL(b.LastUpdateEmpName, b.LastUpdateUser) AS LastUpdateEmpName,
				b.LastUpdateTime
		FROM tas.Master_ShiftPatternTitles a
			INNER JOIN tas.Master_ShiftTimes b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
			LEFT JOIN tas.syJDE_F0005 c ON RTRIM(b.ShiftCode) = LTRIM(RTRIM(c.DRKY)) AND LTRIM(RTRIM(c.DRSY)) = '06' AND UPPER(LTRIM(RTRIM(c.DRRT))) = 'SH'
			LEFT JOIN tas.FlexiTimeSetting d ON RTRIM(a.ShiftPatCode) = RTRIM(d.ShiftPatCode) AND RTRIM(b.ShiftCode) = RTRIM(d.ShiftCode)
		WHERE 
			(RTRIM(a.ShiftPatCode) = @shiftPatCode OR @shiftPatCode IS NULL)
			AND
            (
				(ISNULL(a.IsDayShift, 0) = 0 AND @isDayShift = 1) OR 
				(a.IsDayShift = 1 AND @isDayShift = 2) OR
                @isDayShift IS NULL
			)
			AND
            (
				(@isFlexitime = 2 AND d.SettingID IS NOT NULL AND d.IsActive = 1) OR 
				(@isFlexitime= 1 AND (d.SettingID IS NULL OR ISNULL(d.IsActive, 0) = 0)) OR
                @isFlexitime IS NULL
			)
		ORDER BY a.ShiftPatCode, b.ArrivalFrom
	END 	

	ELSE IF @loadType = 2		--Get shift timing sequence
	BEGIN

		SELECT	a.AutoID,
				a.ShiftPatCode, 
				a.ShiftPointer, 
				a.ShiftCode,
				LTRIM(RTRIM(b.DRDL01)) AS ShiftDescription
		FROM tas.Master_ShiftPattern a
			LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.ShiftCode) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) = '06' AND UPPER(LTRIM(RTRIM(b.DRRT))) = 'SH'
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
		ORDER BY a.ShiftPointer
	END 

GO 

/*
	
PARAMETERS:
	@loadType			TINYINT,
	@shiftPatCode		VARCHAR(10) = '',
	@isDayShift			TINYINT = 0, 
	@isFlexitime		TINYINT = 0

	EXEC tas.Pr_GetShiftPatternDetail 1
	EXEC tas.Pr_GetShiftPatternDetail 1, 'KK'
	EXEC tas.Pr_GetShiftPatternDetail 1, '', 2
	EXEC tas.Pr_GetShiftPatternDetail 1, '', 0, 2
	EXEC tas.Pr_GetShiftPatternDetail 2, 'DP'

*/