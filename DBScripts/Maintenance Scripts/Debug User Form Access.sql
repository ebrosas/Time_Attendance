	--Get all available forms
	SELECT * FROM genuser.FormAccess a
	WHERE a.FormAppID = (SELECT UDCID FROM genuser.UserDefinedCode WHERE UDCUDCGID = 17 AND RTRIM(UDCCode) = 'TAS3')
	ORDER BY a.FormName

	--Get form level permission given to specific employee
	SELECT * FROM genuser.UserFormAccess a
	WHERE a.UserFrmEmpNo = 10003632
		AND RTRIM(a.UserFrmFormCode) IN
		(
			SELECT FormCode FROM genuser.FormAccess 
			WHERE FormAppID = (SELECT UDCID FROM genuser.UserDefinedCode WHERE UDCUDCGID = 17 AND RTRIM(UDCCode) = 'TAS3')
		)

	SELECT * FROM genuser.UserDefinedCode a
	WHERE a.UDCUDCGID = 17 
		AND RTRIM(UDCCode) = 'TAS3'
