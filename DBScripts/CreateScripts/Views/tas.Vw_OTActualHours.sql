/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_OTActualHours
*	Description: Retrieves the overtime actual work hours breakdown 
*
*	Date:			Author:		Rev. #:		Comments:
*	13/03/2018		Ervin		1.0			Created
*	05/08/2018		Ervin		1.1			Added WITH (NOLOCK) clause
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_OTActualHours
AS		
	
	SELECT	tas.ConvertFromJulian(a.GBUPMJ) AS DateUpdated, 
			(a.GBCTRY * 100) + a.GBFY AS FiscalYear,
			LTRIM(RTRIM(a.GBMCU)) AS CostCenter,
			a.GBAN01 / 100 AS January,
			a.GBAN02 / 100 AS February,
			a.GBAN03 / 100 AS March,
			a.GBAN04 / 100 AS April,
			a.GBAN05 / 100 AS May,
			a.GBAN06 / 100 AS June,
			a.GBAN07 / 100 AS July,
			a.GBAN08 / 100 AS August,
			a.GBAN09 / 100 AS September,
			a.GBAN10 / 100 AS October,
			a.GBAN11 / 100 AS November,
			a.GBAN12 / 100 AS December
	FROM tas.sy_F0902 a WITH (NOLOCK)
	WHERE 
		LTRIM(RTRIM(a.GBOBJ)) = '533150'		--Object Account
		AND LTRIM(RTRIM(a.GBSUB)) = '1100'		--Subledger
		AND LTRIM(RTRIM(a.GBLT)) = 'BU'			--Ledger Type

GO

/* Testing:

	SELECT * FROM tas.Vw_OTActualHours a
	ORDER BY FiscalYear DESC
	
*/
