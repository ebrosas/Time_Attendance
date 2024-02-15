USE [tas2]
GO

/****** Object:  View [tas].[Tran_SwipeData_dsx1]    Script Date: 15/09/2019 11:42:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*********************************************************************************
*	Revision History
*
*	Name: tas.Tran_SwipeData_dsx1
*	Description: Retrieves data from JDE_CRP.[CRPCTL].[F0006] table
*
*	Date:			Author:		Ref#:		Comments:
*	03/09/2013		EBrosas		N/A			Created
**********************************************************************************/

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
	FROM tas.External_DSX_evnlog
	WHERE ISNULL(FName,'') <> ''

	/* Old code 
		SELECT--Fname Empno,
		(case 	when 	cast (Fname as int) <= 9999 
			then 	cast ( ('1000' +  cast(Fname as varchar)) as int) 
			else	cast (Fname as int) 
		end) empno,
		TimeDate DT,
		Loc LocationCode,
		Dev ReaderNo,
		Event EventCode,
		'A' Source
		FROM tas.External_DSX_evnlog
		WHERE 
			Fname is not null and Fname<>''
	*/

GO


