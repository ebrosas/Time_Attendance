/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftCode
*	Description: This stored procedure is used to get the shift code list 
*
*	Date			Author		Rev.#		Comments:
*	11/06/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftCode
(   
	@shiftCode	VARCHAR(10) = ''
)
AS
	
	IF ISNULL(@shiftCode, '') = ''
		SET @shiftCode = NULL

	SELECT	LTRIM(RTRIM(a.DRKY)) AS ShiftCode,
			LTRIM(RTRIM(a.DRDL01)) AS ShiftDesc,
			b.ArrivalFrom,
			b.ArrivalTo,
			b.DepartFrom,
			b.DepartTo,
			CASE WHEN LTRIM(RTRIM(a.DRKY)) = 'N'
				THEN 1440 + DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom)
				ELSE DATEDIFF(MINUTE, b.ArrivalTo, b.DepartFrom)
			END AS DurationNormalDay,
			b.RArrivalFrom,
			b.RArrivalTo,
			b.RDepartFrom,
			b.RDepartTo,
			CASE WHEN LTRIM(RTRIM(a.DRKY)) = 'N'
				THEN 1440 + DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom)
				ELSE DATEDIFF(MINUTE, b.RArrivalTo, b.RDepartFrom)
			END AS DurationRamadanDay
	FROM tas.syJDE_F0005 a
		LEFT JOIN tas.Master_ShiftTimes_Setting b ON LTRIM(RTRIM(a.DRKY)) = RTRIM(b.ShiftCode)
	WHERE 
		LTRIM(RTRIM(a.DRSY)) = '06' 
		AND UPPER(LTRIM(RTRIM(a.DRRT))) = 'SH'
		AND RTRIM(LTRIM(ISNULL(a.DRKY, ''))) NOT IN ('')
		AND (LTRIM(RTRIM(a.DRKY)) = @shiftCode OR @shiftCode IS NULL)
	ORDER BY LTRIM(RTRIM(a.DRDL01))

GO 

/*	Debug:
	
	EXEC tas.Pr_GetShiftCode 

*/