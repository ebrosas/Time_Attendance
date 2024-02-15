/*********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetLastSwipeType
*	Description: This function is used to get the last swipe type
*
*	Date			Author		Rev. #		Comments:
*	17/04/2019		Ervin		1.0			Created
*	09/05/2019		Ervin		1.1			Added condition to check if there is existing manual swipe out
*	23/05/2019		Ervin		1.2			Added condition to check if there are more than 2 swipes and night shift
*	26/05/2019		Ervin		1.3			Added condition to check if the last swipe is greater than 6:00 PM
***********************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetLastSwipeType 
(
	@empNo				INT,
	@attendanceDate		DATETIME 
)
RETURNS VARCHAR(3) 
AS
BEGIN

    DECLARE	@lastSwipeType VARCHAR(3) 

	--Get the last swipe type
	SELECT TOP 1  @lastSwipeType = UPPER(RTRIM(SwipeType)) 
	FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
	WHERE EmpNo = @empNo
		AND SwipeDate = @attendanceDate 
	ORDER BY SwipeTime DESC	

	IF @lastSwipeType = 'IN'
	BEGIN

		--Check if there is manual swipe out in the main gate or foil mill gate (Rev. #1.1)
		IF EXISTS
        (
			SELECT 1 FROM tas.Tran_ManualAttendance a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND CONVERT(DATETIME, CONVERT(VARCHAR, a.dtOUT, 12)) = @attendanceDate
				AND ISNULL(a.[timeOUT], '') <> ''
		)
		OR 
		(
			--Check if there are more than 2 swipes and night shift
			(
				SELECT COUNT(*) FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.SwipeDate = @attendanceDate
			) > 2
			--AND 
			--(
			--	SELECT TOP 1 RTRIM(a.ShiftCode) 
			--	FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
			--	WHERE a.EmpNo = @empNo
			--		AND a.SwipeDate = @attendanceDate
			--	ORDER BY SwipeTime DESC	
			--) IN ('N', 'O')
			AND		
			(
				SELECT TOP 1 CAST(a.SwipeTime AS TIME)
				FROM tas.MainGateTodaySwipeLog a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.SwipeDate = @attendanceDate
				ORDER BY SwipeTime DESC	
			) > CAST('18:00:00' AS TIME)
		)
		SET @lastSwipeType = 'OUT'
    END 

	RETURN @lastSwipeType
END


/*	Debugging:

	SELECT tas.fnGetLastSwipeType(10001419, '05/25/2019') 

*/