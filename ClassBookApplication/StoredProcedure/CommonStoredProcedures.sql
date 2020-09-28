CREATE PROCEDURE [ClassBook_GetModuleDataByModuleId]
@ModuleId INT    
AS     
BEGIN    
	DECLARE @TopProducts INT
	SET @TopProducts=5

	 IF @ModuleId=2
	 BEGIN
		WITH #TempRating(EntityId, EntityName,Ratings) as (
			SELECT 
			EntityId,EntityName,AVG(Rating) as Ratings
			FROM Ratings WHERE EntityName='Teacher'
			GROUP BY EntityId,EntityName
		)
		SELECT Top(@TopProducts) 
			T.Id,
			T.FirstName + ' ' + T.LastName as [Name],
			T.[ProfilePictureUrl] as PhotoUrl,
			COUNT(DISTINCT(BoardId)) as BoardCount,  
			COUNT(DISTINCT(MediumId)) as MediumCount,  
			COUNT(DISTINCT(StandardId)) as StandardCount,  
			COUNT(SM.SubjectId) as SubjectCount,
			FORMAT(ISNULL(AVG(R.Ratings),0.0),'N2') as Rating
		FROM Teacher T
			LEFT JOIN StandardMediumBoardMapping SMB ON SMB.EnityId=T.Id AND SMB.ModuleId=2
			LEFT JOIN SubjectMapping SM ON  SMB.Id=SM.SMBId
			LEFT JOIN #TempRating R ON R.EntityId=T.Id
		GROUP BY T.Id,T.[FirstName],T.LastName,T.[ProfilePictureUrl]
		ORDER BY Rating DESC
		

	 END
	 ELSE IF @ModuleId=3
	 BEGIN
		WITH #TempRating(EntityId, EntityName,Ratings) as (
			SELECT 
			EntityId,EntityName,AVG(Rating) as Ratings
			FROM Ratings WHERE EntityName='Classes'
			GROUP BY EntityId,EntityName
		)
		SELECT Top(@TopProducts) 
			C.Id,C.[Name],C.[ClassPhotoUrl] as PhotoUrl,
			COUNT(DISTINCT(BoardId)) as BoardCount,  
			COUNT(DISTINCT(MediumId)) as MediumCount,  
			COUNT(DISTINCT(StandardId)) as StandardCount,
			COUNT(SM.SubjectId) as SubjectCount,
			FORMAT(ISNULL(AVG(R.Ratings),0.0),'N2') as Rating
		FROM Classes C 
			LEFT JOIN StandardMediumBoardMapping SMB ON SMB.EnityId=C.Id AND SMB.ModuleId=@ModuleId
			LEFT JOIN SubjectMapping SM ON SM.SMBId=SMB.Id
			LEFT JOIN #TempRating R ON R.EntityId=C.Id
		GROUP BY C.Id,C.[Name],C.[ClassPhotoUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=4
	 BEGIN
		SELECT Top(@TopProducts)
			CE.Id,
			CE.FirstName + ' ' + CE.LastName as [Name],
			CE.[ProfilePictureUrl] as PhotoUrl,
			0 as BoardCount,  
			0 as MediumCount,  
			0 as StandardCount,  
			0 as SubjectCount,
			FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM CareerExpert CE
			LEFT JOIN Ratings R ON R.EntityId=CE.Id AND R.EntityName='CareerExpert'
		GROUP BY CE.Id,CE.[FirstName],CE.[LastName],CE.[ProfilePictureUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=5
	 BEGIN
		WITH #TempRating(EntityId, EntityName,Ratings) as (
			SELECT 
			EntityId,EntityName,AVG(Rating) as Ratings
			FROM Ratings WHERE EntityName='School'
			GROUP BY EntityId,EntityName
		)
		SELECT S.Id,S.[Name],S.[SchoolPhotoUrl] as PhotoUrl,
			COUNT(DISTINCT(BoardId)) as BoardCount,  
			COUNT(DISTINCT(MediumId)) as MediumCount,  
			COUNT(DISTINCT(StandardId)) as StandardCount,  
			COUNT(SM.SubjectId) as SubjectCount,
			FORMAT(ISNULL(AVG(R.Ratings),0.0),'N2') as Rating
		FROM School S
		LEFT JOIN StandardMediumBoardMapping SMB ON S.Id=SMB.EnityId AND SMB.ModuleId=5
		LEFT JOIN SubjectMapping SM ON  SMB.Id=SM.SMBId
		LEFT JOIN #TempRating R ON R.EntityId=S.Id
		GROUP BY S.Id,S.[Name],S.[SchoolPhotoUrl]
		ORDER BY Rating DESC
	 END
END

GO
CREATE PROCEDURE [ClassBook_GetDetailById]
	@Id INT,  
	@ModuleId INT  
AS
BEGIN      
	SELECT     
		B.[Name] as BoardName,    
		B.[Id] As BoardId,    
		M.[Name] AS MediumName,    
		M.[Id] As MediumId,    
		S.[Name] AS StandardsName,    
		S.[Id] As StandardsId    
	FROM StandardMediumBoardMapping SMB
		INNER JOIN Board B ON B.Id=SMB.BoardId    
		INNER JOIN [Medium] M ON M.Id=SMB.MediumId    
		INNER JOIN Standards S ON S.Id=SMB.StandardId   
	WHERE SMB.EnityId=@Id AND SMB.ModuleId=@ModuleId
END

GO
CREATE PROCEDURE [ClassBook_GetCartDetailByUserId]
@Id INT=0,
@ModuleId INT=0
As
BEGIN
	SELECT
		CASE
			WHEN U.ModuleId = 2 THEN 'Teacher'
		    WHEN U.ModuleId = 3 THEN 'Class'
			ELSE 'Class'
		END AS [Type],
		ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]) AS ProviderName,
		B.[Name] as BoardName,  
		M.[Name] AS MediumName,  
		S.[Name] AS StandardsName,  
		Sub.[Name] AS SubjectName,
		ISNULL(PL.Amount,0.00) As Amount
	FROM StandardMediumBoardMapping SMB  
		INNER JOIN Board B ON B.Id=SMB.BoardId  
		INNER JOIN [Medium] M ON M.Id=SMB.MediumId  
		INNER JOIN Standards S ON S.Id=SMB.StandardId  
		INNER JOIN ShoppingCartSubjects SCS ON SCS.SMBId=SMB.Id
		INNER JOIN USers U ON U.Id=SMB.UserId
		INNER JOIN Subjects Sub ON Sub.Id=SCS.SubjectId
		LEFT JOIN PackageLevel PL ON PL.EntityLevel=SCS.LevelId  AND PL.ModuleId=@ModuleId
		LEFT JOIN Classes C ON C.Id=U.UserId AND U.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=U.UserId AND U.ModuleId=2
	WHERE SCS.UserId=@Id
END


GO
CREATE PROCEDURE [ClassBook_GetSubjects]
	@ModuleId INT,    
	@UserId INT,    
	@BoardId INT,    
	@MediumId INT,    
	@StandardId INT    
AS    
BEGIN    
	IF @ModuleId=3
	BEGIN  
		SELECT S.Id,S.[Name],SMB.Id as [SMBMappingId] from Classes C    
		INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId 
		AND SMB.EnityId=C.Id AND SMB.ModuleId=@ModuleId
		INNER JOIN [SubjectMapping] SM ON SM.SMBId=SMB.Id
		INNER JOIN Subjects S ON S.Id=SM.SubjectId    
	END  
  
	IF @ModuleId=2  
	BEGIN  
		select S.Id,S.[Name],SMB.Id as [SMBMappingId] from Teacher T    
		INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId
		AND SMB.EnityId=T.Id AND SMB.ModuleId=@ModuleId
		INNER JOIN [SubjectMapping] SM ON SM.SMBId=SMB.Id
		INNER JOIN Subjects S ON S.Id=SM.SubjectId
	END  
END 

GO
CREATE PROCEDURE [ClassBook_OrderPaid] 
	@UserId INT,  
	@ModuleId INT,  
	@PaymentType VARCHAR(100)  
AS    
BEGIN   
 DECLARE @Id INT  
 DECLARE @TotalAmount DECIMAL  

 --Insert Data for Order Table
 INSERT INTO [Order] VALUES(@UserId,@PaymentType,GETDATE(),GETDATE(),1,0)  
 SET @Id=SCOPE_IDENTITY()
  
 --Add the ShoppingCartData into OrderSubjects with Amount
 INSERT INTO OrderSubjects  
 SELECT @Id,SMB.Id,Sub.Id,ISNULL(PL.Amount,0)  
 FROM StandardMediumBoardMapping SMB    
 INNER JOIN ShoppingCartSubjects SCS ON SCS.SMBId=SMB.Id    
 INNER JOIN Subjects Sub ON Sub.Id=SCS.SubjectId    
 INNER JOIN PackageLevel PL ON PL.EntityLevel=SCS.LevelId AND PL.ModuleId=@ModuleId  
 WHERE SCS.UserId=@UserId    
  
  --Updae the TotalAmount for Order Table
 SELECT @TotalAmount=ISNULL(SUM(Amount),0)  
 FROM OrderSubjects WHERE OrderId=@Id  
  
 UPDATE [Order]  
 SET TotalAmount=@TotalAmount  
 WHERE Id=@Id

 --Remove the ShoppingCartSubjects Data once the Order is Paid
 DELETE FROM ShoppingCartSubjects
 WHERE UserId=@UserId

END

GO
CREATE PROCEDURE [ClassBook_GetCourses]
AS
BEGIN	
	SELECT 
		CASE
			WHEN CM.ModuleId = 2 THEN 'Teacher'
		    WHEN CM.ModuleId = 3 THEN 'Class'
			ELSE 'Class'
		END AS [Type],
		CS.[Name] as CourseName,
		ISNULL(C.Name,T.[FirstName] + ' ' + T.[LastName]) as CourseProviderName,
		CS.ImageUrl,
		0 As Rating,
		CC.[Name] as CategoryName
	FROM Courses CS
	INNER JOIN CourseCategory CC ON CS.CategoryId=CC.Id
	LEFT JOIN CourseMapping CM ON CM.CourseId=CS.Id
	LEFT JOIN Classes C ON C.Id=CM.EntityId AND CM.ModuleId=3
	LEFT JOIN Teacher T ON T.Id=CM.EntityId AND CM.ModuleId=2
END