CREATE PROCEDURE [dbo].[GetClassesAllData]    
@ModuleId INT    
AS     
BEGIN    
     
   SELECT C.Id,C.[Name],C.[ClassPhotoUrl],  
 COUNT(DISTINCT(BoardId)) as BoardCount,  
 COUNT(DISTINCT(MediumId)) as MediumCount,  
 COUNT(DISTINCT(StandardId)) as StandardCount,  
 COUNT(SMBS.SubjectId) as SubjectCount  
 FROM StandardMediumBoardMapping SMB  
 INNER JOIN MappingData MD ON MD.Id=SMB.MappingDataId  
 INNER JOIN Classes C ON C.Id=MD.AssignToId AND MD.ModuleId=@ModuleId  
 INNER JOIN SMBSubjectMapping SMBS ON  SMBS.SMBId=SMB.Id  
 WHERE MD.ModuleId=@ModuleId  
 GROUP BY C.Id,C.[Name],C.[ClassPhotoUrl]  
END