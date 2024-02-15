/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfLeaveExist
*	Description: This function is used to determine if the attendance day falls within an existing leave request. If true, then will return the leave type.
*
*	Date:				Author:		Rev.#:		Comments:
*	16/09/2021			Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfLeaveExistAll
(
	@autoID INT	
)
RETURNS VARCHAR(10)
AS
BEGIN

	DECLARE	@leaveType	VARCHAR(10)	= '',
			@empNo		INT = 0,
			@DT			DATETIME = NULL
			

	SELECT	@empNo = a.EmpNo,
			@DT = a.DT
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.AutoID = @autoID

	SELECT  @leaveType =  LTRIM(RTRIM(a.LRY58VCVCD))
	FROM tas.syJDE_F58LV13 a WITH (NOLOCK)
	--(
	--	SELECT TOP 5000 * FROM tas.syJDE_F58LV13 WITH (NOLOCK)
	--	WHERE YEAR(tas.ConvertFromJulian(LRY58VCOTD)) BETWEEN YEAR(GETDATE()) - 1 AND YEAR(GETDATE())
	--) a
	WHERE CAST(a.LRAN8 AS INT) = @empNo
		AND @DT BETWEEN tas.ConvertFromJulian(a.LRY58VCOFD) AND
			CASE
				WHEN tas.ConvertFromJulian(a.LRY58VCOTD) < tas.ConvertFromJulian(a.LRY58VCOFD) THEN --(Note: Check if Leave End Date < Leave Start Date. If true, then set Leave End Date = Leave Resume Date)
					CASE	
						WHEN ISNULL(a.LRY58VCOTD, 0) = 0 THEN tas.ConvertFromJulian(a.LRY58VCOFD)
						WHEN tas.ConvertFromJulian(a.LRY58VCOFD) = tas.ConvertFromJulian(a.LRY58VCOTD) THEN tas.ConvertFromJulian(a.LRY58VCOTD)
						ELSE DATEADD(dd, 1, tas.ConvertFromJulian(a.LRY58VCOTD))
					END
				ELSE tas.ConvertFromJulian(ISNULL(a.LRY58VCOTD, a.LRY58VCOFD))
			END
		AND a.LRY58VCAFG IN 
		(
			'A',	--Approved / Paid     
			'N',	--Approved / Not Paid  
			'W'		--Waiting for Approval      
		)		

	RETURN @leaveType 

END 


/*	Debug:

	SELECT tas.fnCheckIfLeaveExistAll(6570122)

*/

