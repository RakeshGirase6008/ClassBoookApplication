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
@ModuleId INT=0,
@ClassBookHandlingAmount DECIMAL OUTPUT
As
BEGIN
		-- Drop the ##Temp Tables
		DECLARE @sql nvarchar(max)        
		SELECT	@sql = isnull(@sql+';', '') + 'drop table ' + quotename(name)        
		FROM	tempdb..sysobjects        
		WHERE	[name] like '##%'        
		EXEC	(@sql)        

		-- Get the SubjectMapping Data for User
		SELECT 
				CASE
					WHEN SMB.ModuleId = 2 THEN 'Teacher'
				    WHEN SMB.ModuleId = 3 THEN 'Class'
					ELSE 'Class'
				END AS [ProviderType],
				SCI.[Type] as [LearningType],
				SCI.ActualAmount AS [ActualFees],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				ISNULL(B.[Name],'') as BoardName,  
				ISNULL(M.[Name],'') AS MediumName,  
				ISNULL(S.[Name],'') AS StandardsName,  
				ISNULL(Sub.[Name],'') AS EnityName,
				SCI.TypeOfMapping
				INTO ##TeMp
		FROM 
		ShoppingCartItems SCI 
		INNER JOIN SubjectMapping SM ON SM.Id=SCI.MappingId AND TypeOfMapping='Subject'
		LEFT JOIN StandardMediumBoardMapping SMB ON SMB.Id=SM.SMBId
		LEFT JOIN Board B ON B.Id=SMB.BoardId  
		LEFT JOIN [Medium] M ON M.Id=SMB.MediumId  
		LEFT JOIN Standards S ON S.Id=SMB.StandardId
		LEFT JOIN Subjects Sub ON Sub.Id=SM.SubjectId
		LEFT JOIN Classes C ON C.Id=SMB.EnityId AND SMB.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=SMB.EnityId AND SMB.ModuleId=2
		WHERE SCI.EntityId=1 AND SCI.ModuleId=1


		-- Get the CourseMapping Data for User
		SELECT
				CASE
					WHEN CM.ModuleId = 2 THEN 'Teacher'
				    WHEN CM.ModuleId = 3 THEN 'Class'
					ELSE 'Class'
				END AS [ProviderType],
				SCI.[Type] as [LearningType],
				SCI.ActualAmount AS [ActualFees],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(CS.[Name],'') AS EnityName,
				SCI.TypeOfMapping
				INTO ##TeMp1
		FROM 
		ShoppingCartItems SCI 
		INNER JOIN CourseMapping CM ON CM.Id=SCI.MappingId AND SCI.TypeOfMapping='Course'
		LEFT JOIN Courses CS ON CS.Id=CM.CourseId
		LEFT JOIN Classes C ON C.Id=CM.EnityId AND CM.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=CM.EnityId AND CM.ModuleId=2
		WHERE SCI.EntityId=1 AND SCI.ModuleId=1

		-- Get the CareerExpertMapping Data for User
		SELECT
				'CareerExpert' AS [ProviderType],
				SCI.[Type] as [LearningType],
				SCI.ActualAmount AS [ActualFees],
				ISNULL(CE.FirstName + ' ' + CE.LastName,'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(E.[Name],'') AS EnityName,
				SCI.TypeOfMapping
				INTO ##TeMp2
		FROM 
		ShoppingCartItems SCI 
		INNER JOIN ExpertiseMapping EM ON EM.Id=SCI.MappingId AND SCI.TypeOfMapping='Expertise'
		LEFT JOIN Expertise E ON E.Id=EM.ExpertiseId
		LEFT JOIN CareerExpert CE ON CE.Id=EM.EnityId AND EM.ModuleId=4
		WHERE SCI.EntityId=@ModuleId AND SCI.ModuleId=@ModuleId

		-- Show allData in one Table
		SELECT * FROM ##TeMp
		UNION 
		SELECT * FROM ##TeMp1
		UNION
		SELECT * FROM ##TeMp2
		ORDER BY TypeOfMapping

		-- Calculate the ClassBookHandlingAmount
		SELECT @ClassBookHandlingAmount=SUM(OurAmount) FROM ShoppingCartItems SCI
		WHERE SCI.EntityId=@Id AND SCI.ModuleId=@ModuleId AND TypeOfMapping='Subject' AND [Type]='Physical'
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
	DECLARE @DistanceFeesForSubject DECIMAL
	SELECT @DistanceFeesForSubject=ISNULL([Value],0) FROM Settings WHERE [Name]='FeesSetting.DistanceFeesForSubject'

	IF @ModuleId=3  
	BEGIN  
		SELECT S.Id,S.[Name],SM.Id as [SubjectMappingId],
		DistanceFees + @DistanceFeesForSubject as DistanceFees,
		PhysicalFees
		FROM Classes C    
		INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId 
		AND SMB.EnityId=C.Id AND SMB.ModuleId=@ModuleId
		INNER JOIN [SubjectMapping] SM ON SM.SMBId=SMB.Id
		INNER JOIN Subjects S ON S.Id=SM.SubjectId    
	END  
  
	IF @ModuleId=2  
	BEGIN  
		SELECT 
		S.Id,S.[Name],SM.Id as [SubjectMappingId],
		DistanceFees + @DistanceFeesForSubject as DistanceFees, 
		PhysicalFees
		FROM Teacher T    
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
	DECLARE @TotalActualAmount DECIMAL
	DECLARE @TotalOurAmount DECIMAL

	--Insert Data for Order Table
	INSERT INTO [Order] VALUES(@UserId,@ModuleId,@PaymentType,GETDATE(),GETDATE(),1,0,0)  
	SET @Id=SCOPE_IDENTITY()  
	 
	--Add the ShoppingCartItems into ShoppingCartItems with Amount
	INSERT INTO OrderCartItems  
	SELECT @Id,MappingId,TypeOfMapping,[Type],ActualAmount,OurAmount
	FROM ShoppingCartItems
	WHERE EntityId=@UserId AND ModuleId=@ModuleId
	 
	 --Updae the TotalAmount for Order Table
	SELECT @TotalActualAmount=ISNULL(SUM(ActualAmount),0),@TotalOurAmount=ISNULL(SUM(OurAmount),0)  
	FROM OrderCartItems WHERE OrderId=@Id  
	 
	UPDATE [Order]  
	SET TotalAmount=@TotalActualAmount +  @TotalOurAmount,OurAmount=@TotalOurAmount
	WHERE Id=@Id

	DELETE FROM ShoppingCartItems
	WHERE EntityId=@UserId AND ModuleId=@ModuleId
END

GO
CREATE PROCEDURE [ClassBook_GetCourse s]
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