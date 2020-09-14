CREATE PROCEDURE [dbo].[GetModuleDataByModuleId]
@ModuleId INT    
AS     
BEGIN    
	 IF @ModuleId=2
	 BEGIN
		SELECT T.Id,
		T.FirstName + ' ' + T.LastName as [Name],
		T.[ProfilePictureUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Teacher T
		LEFT JOIN Ratings R ON R.EntityId=T.Id AND R.EntityName='Teacher'
		LEFT JOIN MappingData MD ON MD.AssignToId=T.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY T.Id,T.[FirstName],T.LastName,T.[ProfilePictureUrl]
	 END
	 ELSE IF @ModuleId=3
	 BEGIN
		SELECT C.Id,C.[Name],C.[ClassPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Classes C
		LEFT JOIN Ratings R ON R.EntityId=C.Id AND R.EntityName='Classes'
		LEFT JOIN MappingData MD ON MD.AssignToId=C.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY C.Id,C.[Name],C.[ClassPhotoUrl]
	 END
	 ELSE IF @ModuleId=4
	 BEGIN
		SELECT CE.Id,
		CE.FirstName + ' ' + CE.LastName as [Name],
		CE.[ProfilePictureUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM CareerExpert CE
		LEFT JOIN Ratings R ON R.EntityId=CE.Id AND R.EntityName='CareerExpert'
		LEFT JOIN MappingData MD ON MD.AssignToId=CE.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY CE.Id,CE.[FirstName],CE.[LastName],CE.[ProfilePictureUrl]
	 END
	 ELSE IF @ModuleId=5
	 BEGIN
		SELECT S.Id,S.[Name],S.[SchoolPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM School S
		LEFT JOIN Ratings R ON R.EntityId=S.Id AND R.EntityName='School'
		LEFT JOIN MappingData MD ON MD.AssignToId=S.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY S.Id,S.[Name],S.[SchoolPhotoUrl]
	 END
END

GO
CREATE PROCEDURE [dbo].[GetCartDetailByUserId]
@UserId INT,
@ModuleId INT
AS       
BEGIN  
	SELECT B.[Name] as BoardName,
	M.[Name] AS MediumName,
	S.[Name] AS StandardsName,
	Sub.[Name] AS SubjectName,
	PL.Amount
	FROM StandardMediumBoardMapping SMB
	INNER JOIN Board B ON B.Id=SMB.BoardId
	INNER JOIN [Medium] M ON M.Id=SMB.MediumId
	INNER JOIN Standards S ON S.Id=SMB.StandardId
	INNER JOIN ShoppingCartItem SCI ON SCI.SMBId=SMB.Id
	INNER JOIN Subjects Sub ON Sub.Id=SCI.SubjectId
	INNER JOIN PackageLevel PL ON PL.EntityLevel=SCI.LevelId AND PL.ModuleId=@ModuleId
	WHERE SMB.UserId=@UserId
END