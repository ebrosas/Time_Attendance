/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetOTReason
*	Description: Get the list of overtime reasons
*
*	Date			Author		Revision No.	Comments:
*	30/09/2016		Ervin		1.0				Created
*	20/05/2018		Ervin		1.1				Added 'ROT - OT for Ramadan' 
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetOTReason
(
	@loadType	TINYINT = 0
)
AS

	IF @loadType = 1
	BEGIN
    
		--Get overtime reasons for approval
		SELECT Code, [Description] 
		FROM tas.Master_OTReasons_JDE
		WHERE CODE IN 
		(
			'AL',
			'BD',
			'EW',
			'MS',
			'PD',
			'PH',
			'PM',
			'SD',
			'SR',
			'TR',
			'DF',
			'CAL',
			'CBD',
			'CSR',
			'CDF',
			'COMS',
			'COEW',
			'ACS',
			'CCS',
			'MA',
			'ROT'		--Rev. #1.1
		) 
		ORDER BY [Description]
	END 

	ELSE IF @loadType = 2
	BEGIN
    
		--Get overtime reasons for rejection
		SELECT	Code, [Description] 
		FROM tas.Master_OTReasons_JDE a
		WHERE SUBSTRING(LTRIM(RTRIM(a.CODE)), 1, 2) IN ('RO')
		ORDER BY a.[Description]
	END 

	ELSE 
	BEGIN

		--Get all overtime reasons
		SELECT Code, [Description] 
		FROM tas.Master_OTReasons_JDE a
		ORDER BY a.[Description]
	END 

GO

/*	Debug:

	EXEC tas.Pr_GetOTReason			--All
	EXEC tas.Pr_GetOTReason	1		--OT approved
	EXEC tas.Pr_GetOTReason 2		--OT rejected

*/