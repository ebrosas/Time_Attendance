USE [tas2]
GO

/****** Object:  View [tas].[Master_Employee_JDE_View]    Script Date: 4/11/2016 3:53:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [tas].[Master_Employee_JDE_View]
-- select * from Master_Employee_JDE_view
-- F060116
-- Master_Employee_JDE_TmpRename
AS

SELECT     
	CAST(YAAN8 AS INT) AS EmpNo, 
	YAALPH AS EmpName, 
	YAEEOM AS ReligionCode, 
	YAEEOJ AS JobCategoryCode, 
	YASEX AS SexCode, 
	(CASE 
	WHEN RTRIM(LTRIM(A.WorkingBusinessUnit))='' OR A.WorkingBusinessUnit IS NULL THEN RTRIM(LTRIM(YAHMCU)) 
	ELSE RTRIM(LTRIM(A.WorkingBusinessUnit))
	END) AS BusinessUnit , 
	YAHMCO AS Company, 
	--in case padding is required in grade --tas.lpad(cast(YAPGRD as int) , 2 , '0') AS GradeCode, 
	(CASE WHEN ISNUMERIC(YAPGRD)=1 THEN tas.lpad(CAST(YAPGRD AS INT) , 2 , '0')  ELSE 0 END) GradeCode,
	tas.ConvertFromJulian(YAPSDT) AS DateJoined, 
	tas.ConvertFromJulian(YADT) AS DateResigned,
	YAPAST AS PayStatus,
	tas.ConvertFromJulian(YADOB) AS 'DateOfBirth'
FROM External_JDE_F060116 E
	LEFT JOIN Master_EmployeeAdditional A ON CAST(E.YAAN8 AS INT) = A.empno
WHERE CAST(YAAN8 AS INT) >= 10000000


GO


