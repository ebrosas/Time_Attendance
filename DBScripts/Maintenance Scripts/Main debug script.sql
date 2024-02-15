	--Get the Swipes History
	EXEC tas.Pr_GetEmployeeSwipeInfo  '01/03/2016', '31/03/2016', 10003632, '7600'

	SELECT * FROM [tas].[Master_FireteamMember]