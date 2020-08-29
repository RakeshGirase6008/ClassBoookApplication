CREATE PROCEDURE [dbo].[GetModuleDataByModuleId]
@ModuleId INT    
AS     
BEGIN    
	 IF @ModuleId=2
	 BEGIN
		SELECT T.Id,T.[FirstName],T.[ProfilePictureUrl] as PhotoUrl,  
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount  
		FROM Teacher T
		LEFT JOIN MappingData MD ON MD.AssignToId=T.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY T.Id,T.[FirstName],T.[ProfilePictureUrl]
	 END
	 ELSE IF @ModuleId=3
	 BEGIN
		SELECT C.Id,C.[Name],C.[ClassPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount  
		FROM Classes C
		LEFT JOIN MappingData MD ON MD.AssignToId=C.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY C.Id,C.[Name],C.[ClassPhotoUrl]
	 END
	 ELSE IF @ModuleId=4
	 BEGIN
		SELECT CE.Id,CE.[FirstName],CE.[ProfilePictureUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount  
		FROM CareerExpert CE
		LEFT JOIN MappingData MD ON MD.AssignToId=CE.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY CE.Id,CE.[FirstName],CE.[ProfilePictureUrl]
	 END
	 ELSE IF @ModuleId=5
	 BEGIN
		SELECT S.Id,S.[Name],S.[SchoolPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(SMBS.SubjectId) as SubjectCount  
		FROM School S
		LEFT JOIN MappingData MD ON MD.AssignToId=S.Id AND MD.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON MD.Id=SMB.MappingDataId
		LEFT JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id
		GROUP BY S.Id,S.[Name],S.[SchoolPhotoUrl]
	 END
END