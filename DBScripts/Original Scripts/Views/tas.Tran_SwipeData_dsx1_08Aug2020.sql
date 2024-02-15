USE [tas2]
GO

/****** Object:  View [tas].[Tran_SwipeData_dsx1]    Script Date: 08/08/2020 15:52:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Tran_SwipeData_dsx1
*	Description: Retrieves data from JDE_CRP.[CRPCTL].[F0006] table
*
*	Date:			Author:		Rev. #:		Comments:
*	03/09/2013		Ervin		1.0			Created
*	22/03/2020		Ervin		1.1			Refactored the logic in fetching the employee no. value
*****************************************************************************************************************************************/

ALTER VIEW [tas].[Tran_SwipeData_dsx1] 
AS 

	SELECT	
		EmpNo = CASE WHEN ISNUMERIC(FName) = 1 
				THEN 
					CASE WHEN CONVERT(INT, FName) <= 9999 
					THEN CONVERT(INT, FName) + 10000000
					ELSE CONVERT(INT, FName) END
				ELSE 0 END,
		TimeDate DT,
		Loc LocationCode,
		Dev ReaderNo,
		Event EventCode,
		'A' Source
	FROM tas.External_DSX_evnlog a WITH (NOLOCK)
	WHERE ISNULL(a.FName,'') <> ''

GO


