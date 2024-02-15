/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDependentInfo
*	Description: Get the employee's dependents information
*
*	Date			Author		Revision No.	Comments:
*	09/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDependentInfo
(   
	@empNo	INT 
)
AS

	SELECT	b.HUAN8 AS DependentNo,
			LTRIM(RTRIM(b.HUALPH)) AS DependentName,
			LTRIM(RTRIM(c.DRDL01)) AS Relationship,			
			b.HUPU01 AS RelationshipID,
			b.HUSEX AS Sex,
			tas.ConvertFromJulian(b.HUDOB) AS DOB,
			LTRIM(RTRIM(d.T3RMK)) AS CPRNo,
			tas.ConvertFromJulian(d.T3EFTE) AS CPRExpDate,
			tas.ConvertFromJulian(e.T3EFTE) AS ResPermitExpDate
	FROM tas.sy_F08336 a
		INNER JOIN tas.sy_F08901 b ON a.BJPAN8 = b.HUAN8
		LEFT JOIN tas.syJDE_F0005 c ON a.BJRELA = LTRIM(RTRIM(c.DRKY)) AND c.DRSY = '08' AND c.DRRT = 'RL'
		LEFT JOIN tas.syJDE_F00092 d ON a.BJPAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3SDB)) = 'P' AND LTRIM(RTRIM(d.T3TYDT)) = 'LD' AND LTRIM(RTRIM(d.T3KY)) = 'CP-DEP'
		LEFT JOIN tas.syJDE_F00092 e ON a.BJPAN8 = e.T3SBN1 AND LTRIM(RTRIM(e.T3SDB)) = 'P' AND LTRIM(RTRIM(e.T3TYDT)) = 'LD' AND LTRIM(RTRIM(e.T3KY)) = 'PP-DEP'
	WHERE a.BJAN8 = @empNo
	ORDER BY b.HUDOB

GO 

/*	Debugging:

	EXEC tas.Pr_GetDependentInfo 10003191

*/


