	--Get all registered AMS systems
	SELECT * FROM genuser.UserDefinedCode a
	WHERE a.UDCUDCGID = 17

	--Check allowed cost center givern to user by application ID
	SELECT * FROM genuser.PermitCostCenter a
	WHERE a.PermitAppID = (SELECT UDCID FROM genuser.UserDefinedCode WHERE UDCUDCGID = 17 AND RTRIM(UDCCode) = 'TAS3')
		AND a.PermitEmpNo = 10003632

	