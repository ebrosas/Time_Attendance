USE [tas2]
GO

/****** Object:  UserDefinedFunction [tas].[fnCheckWorkplaceEnabled]    Script Date: 28/08/2022 08:39:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfLeaveExist
*	Description: This function is used to determine if whether the employee is required to swipe in the workplace reader 
*
*	Date:				Author:		Rev.#:		Comments:
*	07/04/2022			Ervin		1.0			Created
*	29/06/2022			Ervin		1.1			Checks if employee is exempted from swiping at the workplace readers
*	06/08/2022			Ervin		1.2			Added logic to check workplace exclusion by reader no.
*	17/08/2022			Ervin		1.3			Added filter to check if SourceID not equal to 2
**************************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnCheckWorkplaceEnabled]
(
	@empNo	INT 
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo				INT,
	CostCenter			VARCHAR(12),
	IsWorkplaceEnabled	BIT,
	IsSyncTimesheet		BIT,
	IsAdminBldgEnabled	BIT  
)
AS
BEGIN

	DECLARE	@isWorkplaceEnabled	BIT = 0,
			@isSyncTimesheet	BIT = 0,
			@isAdminBldgEnabled	BIT = 0, 
			@costCenter			VARCHAR(12) = '',
			@shiftPatCode		VARCHAR(2) = '',
			@isDayShift			BIT = 0,
			@payGrade			INT = 0
		
	SELECT	@costCenter = RTRIM(a.BusinessUnit),
			@shiftPatCode = RTRIM(b.ShiftPatCode),
			@isDayShift = ISNULL(c.IsDayShift, 0),
			@payGrade = a.GradeCode
	FROM tas.Master_Employee_JDE a WITH (NOLOCK)
		INNER JOIN tas.Master_EmployeeAdditional b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		INNER JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(b.ShiftPatCode) = RTRIM(c.ShiftPatCode)
		OUTER APPLY 
		(
			SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
			WHERE IsActive = 1 
				AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
		) d
	WHERE a.EmpNo = @empNo

	--Check first if employee is required to swipe in the workplace readers in the plant which use the UNIS system
	IF EXISTS
	(
		SELECT 1 
		FROM tas.WorkplaceReaderSetting a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode = 8 AND b.ReaderNo BETWEEN 41 AND 70 AND ISNULL(b.SourceID, '0') <> 2		--(Notes: Reader Nos. 41 to 70 refers to workplace reader devices that use UNIS system)  Rev. #1.2
		WHERE a.IsActive = 1
			AND RTRIM(a.CostCenter) = @costCenter
	)
	AND NOT EXISTS		--Rev. #1.1
	(
		SELECT 1 
		FROM tas.WorkplaceSwipeExclusion a WITH (NOLOCK)
			--INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode = 8 AND b.ReaderNo BETWEEN 41 AND 70		--(Notes: Reader Nos. 41 to 70 refers to workplace reader devices that use UNIS system)  Rev. #1.2
			CROSS APPLY
			(
				SELECT GenericNo AS ExcludedReaderNo 
				FROM tas.fnParseStringArrayToInt(RTRIM(a.ReaderNoList), ',') x
					INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.GenericNo = y.ReaderNo AND y.LocationCode = 8 AND y.ReaderNo BETWEEN 41 AND 70 AND ISNULL(y.SourceID, '0') <> 2
			) b
		WHERE RTRIM(a.CostCenter) = @costCenter
			AND a.EmpNo = @empNo
			AND a.IsActive = 1
	)
	AND @isDayShift = 0
	AND @payGrade <= 8
	BEGIN	

		SELECT	TOP 1 
				@isWorkplaceEnabled = ISNULL(a.IsActive, 0),
				@isSyncTimesheet = ISNULL(a.IsSyncTimesheet, 0)
		FROM tas.WorkplaceReaderSetting a WITH (NOLOCK) 
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode = 8 AND b.ReaderNo BETWEEN 41 AND 70 AND ISNULL(b.SourceID, 0) <> 2
		WHERE RTRIM(a.CostCenter) = @costCenter
			AND a.IsActive = 1
	END 

	IF @isWorkplaceEnabled = 0
	BEGIN

		--Now, check if employee is required to swipe in the Admin Bldg. readers which use the ALPETA System
		IF EXISTS
		(
			SELECT 1 
			FROM tas.WorkplaceReaderSetting a WITH (NOLOCK)
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode = 8 AND b.SourceID = 2				--(Notes: SourceID = 2 refers to Admin Bldg. readers)  Rev. #1.2
			WHERE a.IsActive = 1
				AND RTRIM(a.CostCenter) = @costCenter
		)
		AND NOT EXISTS
		(
			SELECT 1 
			FROM tas.WorkplaceSwipeExclusion a WITH (NOLOCK)
				CROSS APPLY
				(
					SELECT GenericNo AS ExcludedReaderNo 
					FROM tas.fnParseStringArrayToInt(RTRIM(a.ReaderNoList), ',') x
						INNER JOIN tas.Master_AccessReaders y WITH (NOLOCK) ON x.GenericNo = y.ReaderNo AND y.LocationCode = 8 AND y.SourceID = 2
				) b
			WHERE RTRIM(a.CostCenter) = @costCenter
				AND a.EmpNo = @empNo
				AND a.IsActive = 1
		)
		BEGIN

			SET @isAdminBldgEnabled = 1

			SELECT	TOP 1
					@isWorkplaceEnabled = ISNULL(a.IsActive, 0),
					@isSyncTimesheet = ISNULL(a.IsSyncTimesheet, 0)
			FROM tas.WorkplaceReaderSetting a WITH (NOLOCK) 
				INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.ReaderNo = b.ReaderNo AND b.LocationCode = 8 AND b.SourceID = 2
			WHERE RTRIM(a.CostCenter) = @costCenter
				AND a.IsActive = 1
        END 
    END 

	INSERT INTO @rtnTable 
	SELECT @empNo, @costCenter, @isWorkplaceEnabled, @isSyncTimesheet, @isAdminBldgEnabled

	RETURN

END 

/*	Debug:

	SELECT * FROM tas.fnCheckWorkplaceEnabled(10003632)
	SELECT * FROM  tas.fnCheckWorkplaceEnabled(10003662)
	SELECT * FROM  tas.fnCheckWorkplaceEnabled(10003631)	
	SELECT * FROM  tas.fnCheckWorkplaceEnabled(10001645)	
	SELECT * FROM  tas.fnCheckWorkplaceEnabled(10001415)		--2110 
	SELECT * FROM  tas.fnCheckWorkplaceEnabled(10003492)		--2111 

*/

GO


