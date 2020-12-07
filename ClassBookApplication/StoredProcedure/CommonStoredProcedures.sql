CREATE PROCEDURE [ClassBook_GetModuleDataByModuleId]
	@ModuleId INT    
AS     
BEGIN    
	DECLARE @TopProducts INT
	DECLARE @EntityName VARCHAR(20)
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
			LEFT JOIN StandardMediumBoardMapping SMB ON SMB.EntityId=T.Id AND SMB.ModuleId=@ModuleId
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
			LEFT JOIN StandardMediumBoardMapping SMB ON SMB.EntityId=C.Id AND SMB.ModuleId=@ModuleId
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
		LEFT JOIN StandardMediumBoardMapping SMB ON S.Id=SMB.EntityId AND SMB.ModuleId=@ModuleId
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
	WHERE SMB.EntityId=@Id AND SMB.ModuleId=@ModuleId
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
		LEFT JOIN Classes C ON C.Id=SMB.EntityId AND SMB.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=SMB.EntityId AND SMB.ModuleId=2
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
		LEFT JOIN Classes C ON C.Id=CM.EntityId AND CM.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=CM.EntityId AND CM.ModuleId=2
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
		LEFT JOIN CareerExpert CE ON CE.Id=EM.EntityId AND EM.ModuleId=4
		WHERE SCI.EntityId=@ModuleId AND SCI.ModuleId=@ModuleId

		-- Show allData in one Table
		SELECT * FROM ##TeMp
		UNION 
		SELECT * FROM ##TeMp1
		UNION
		SELECT * FROM ##TeMp2
		ORDER BY TypeOfMapping

		-- Calculate the ClassBookHandlingAmount
		SELECT @ClassBookHandlingAmount=ISNULL(SUM(OurAmount),0) FROM ShoppingCartItems SCI
		WHERE SCI.EntityId=@Id AND SCI.ModuleId=@ModuleId AND TypeOfMapping='Subject' AND [Type]='Physical'

END
GO
CREATE PROCEDURE [ClassBook_GetSubjects]
	@UserId INT,
	@ModuleId INT,    
	@EntityId INT,    
	@BoardId INT,    
	@MediumId INT,    
	@StandardId INT    
AS    
BEGIN    
	DECLARE @LoginEntityId INT
	DECLARE @LoginModuleId INT

	SELECT @LoginEntityId=ISNULL(EntityId,0),@LoginModuleId=ISNULL(ModuleId,0) FROM Users WHERE Id=@UserId

	DECLARE @DistanceFeesForSubject DECIMAL
	SELECT @DistanceFeesForSubject=ISNULL([Value],0) FROM Settings WHERE [Name]='FeesSetting.DistanceFeesForSubject'

	SELECT S.Id,S.[Name],SM.Id as [SubjectMappingId],
	OCI.Id as [OrderCartItemId],
	IIF(SCI.Id IS NULL,0,1) as InCart,
	DistanceFees,
	PhysicalFees
	FROM StandardMediumBoardMapping SMB
	INNER JOIN [SubjectMapping] SM ON SM.SMBId=SMB.Id
	INNER JOIN Subjects S ON S.Id=SM.SubjectId
	LEFT JOIN OrderCartItems OCI ON SM.Id=OCI.MappingId AND OCI.TypeOfMapping='Subject'
	LEFT JOIN ShoppingCartItems SCI ON SCI.MappingId=SM.Id AND SCI.EntityId=@LoginEntityId AND SCI.ModuleId=@LoginModuleId
	WHERE SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId
	AND SMB.ModuleId=@ModuleId AND SMB.EntityId=@EntityId
END

GO
CREATE PROCEDURE [ClassBook_OrderPaid]
 @UserId INT,    
 @ModuleId INT,    
 @PaymentType VARCHAR(100),
 @ReferCode VARCHAR(100)
AS      
BEGIN     
 DECLARE @Id INT    
 DECLARE @TotalActualAmount DECIMAL  
 DECLARE @TotalOurAmount DECIMAL  
 DECLARE @ChannelPartnerId INT=0

 SELECT @ChannelPartnerId=ISNULL(Id,0) FROM ChannelPartner WHERE ReferCode=@ReferCode
  
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

 IF @ModuleId=2 OR @ModuleId=3
 BEGIN
	--Update SubjectMapping
	UPDATE SubjectMapping
	SET Active=1 
	WHERE Id in(
	SELECT SM.Id FROM [Order] O 
	INNER JOIN OrderCartItems OCI ON O.Id=OCI.OrderId
	INNER JOIN SubjectMapping SM ON OCI.MappingId=SM.Id AND OCI.TypeOfMapping='Subject'
	WHERE O.Id=@Id)

	UPDATE CourseMapping
	SET Active=1 
	WHERE Id in(
	SELECT CM.Id FROM [Order] O 
	INNER JOIN OrderCartItems OCI ON O.Id=OCI.OrderId
	INNER JOIN CourseMapping CM ON OCI.MappingId=CM.Id AND OCI.TypeOfMapping='Course'
	WHERE O.Id=@Id)
 END
 EXEC onthef1x_ChannelPartner.onthef1x.Classbook_CalculateCommision @Id,@ChannelPartnerId,@TotalOurAmount,'ClassBook'
END


GO
CREATE PROCEDURE [ClassBook_GetCourses]
AS  
BEGIN   

 SELECT   
	CASE  
		WHEN CM.ModuleId = 2 THEN 'Teacher'  
		WHEN CM.ModuleId = 3 THEN 'Class'  
		ELSE 'Not Assign'  
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

GO
CREATE PROCEDURE [ClassBook_GetSubscrptionDetailByUserId]
	@Id INT=0,
	@ModuleId INT=0
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
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				ISNULL(B.[Name],'') as BoardName,  
				ISNULL(M.[Name],'') AS MediumName,  
				ISNULL(S.[Name],'') AS StandardsName,  
				ISNULL(Sub.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as SubscriptionDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN SubjectMapping SM ON SM.Id=OCI.MappingId AND TypeOfMapping='Subject'
		LEFT JOIN StandardMediumBoardMapping SMB ON SMB.Id=SM.SMBId
		LEFT JOIN Board B ON B.Id=SMB.BoardId  
		LEFT JOIN [Medium] M ON M.Id=SMB.MediumId  
		LEFT JOIN Standards S ON S.Id=SMB.StandardId
		LEFT JOIN Subjects Sub ON Sub.Id=SM.SubjectId
		LEFT JOIN Classes C ON C.Id=SMB.EntityId AND SMB.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=SMB.EntityId AND SMB.ModuleId=2
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId


		-- Get the CourseMapping Data for User
		SELECT
				CASE
					WHEN CM.ModuleId = 2 THEN 'Teacher'
				    WHEN CM.ModuleId = 3 THEN 'Class'
					ELSE 'Class'
				END AS [ProviderType],
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(CS.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as SubscriptionDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp1
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN CourseMapping CM ON CM.Id=OCI.MappingId AND OCI.TypeOfMapping='Course'
		LEFT JOIN Courses CS ON CS.Id=CM.CourseId
		LEFT JOIN Classes C ON C.Id=CM.EntityId AND CM.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=CM.EntityId AND CM.ModuleId=2
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId

		-- Get the CareerExpertMapping Data for User
		SELECT
				'CareerExpert' AS [ProviderType],
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(CE.FirstName + ' ' + CE.LastName,'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(E.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as SubscriptionDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp2
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN ExpertiseMapping EM ON EM.Id=OCI.MappingId AND OCI.TypeOfMapping='Expertise'
		LEFT JOIN Expertise E ON E.Id=EM.ExpertiseId
		LEFT JOIN CareerExpert CE ON CE.Id=EM.EntityId AND EM.ModuleId=4
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId

		-- Show allData in one Table
		SELECT * FROM ##TeMp
		UNION 
		SELECT * FROM ##TeMp1
		UNION
		SELECT * FROM ##TeMp2
		ORDER BY TypeOfMapping
END

GO
CREATE PROCEDURE [ClassBook_GetTranscationDetailByUserId]
	@Id INT=0,
	@ModuleId INT=0
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
				OCI.Id as TranscatioNo,
				CASE
					WHEN SMB.ModuleId = 2 THEN 'Teacher'
				    WHEN SMB.ModuleId = 3 THEN 'Class'
					ELSE 'Class'
				END AS [ProviderType],
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				ISNULL(B.[Name],'') as BoardName,  
				ISNULL(M.[Name],'') AS MediumName,  
				ISNULL(S.[Name],'') AS StandardsName,  
				ISNULL(Sub.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as OrderDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN SubjectMapping SM ON SM.Id=OCI.MappingId AND TypeOfMapping='Subject'
		LEFT JOIN StandardMediumBoardMapping SMB ON SMB.Id=SM.SMBId
		LEFT JOIN Board B ON B.Id=SMB.BoardId  
		LEFT JOIN [Medium] M ON M.Id=SMB.MediumId  
		LEFT JOIN Standards S ON S.Id=SMB.StandardId
		LEFT JOIN Subjects Sub ON Sub.Id=SM.SubjectId
		LEFT JOIN Classes C ON C.Id=SMB.EntityId AND SMB.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=SMB.EntityId AND SMB.ModuleId=2
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId


		-- Get the CourseMapping Data for User
		SELECT
				OCI.Id as TranscatioNo,
				CASE
					WHEN CM.ModuleId = 2 THEN 'Teacher'
				    WHEN CM.ModuleId = 3 THEN 'Class'
					ELSE 'Class'
				END AS [ProviderType],
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(ISNULL(C.[Name],T.[FirstName] + ' ' + T.[LastName]),'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(CS.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as OrderDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp1
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN CourseMapping CM ON CM.Id=OCI.MappingId AND OCI.TypeOfMapping='Course'
		LEFT JOIN Courses CS ON CS.Id=CM.CourseId
		LEFT JOIN Classes C ON C.Id=CM.EntityId AND CM.ModuleId=3
		LEFT JOIN Teacher T ON T.Id=CM.EntityId AND CM.ModuleId=2
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId

		-- Get the CareerExpertMapping Data for User
		SELECT
				OCI.Id as TranscatioNo,
				'CareerExpert' AS [ProviderType],
				OCI.[Type] as [LearningType],
				OCI.ActualAmount AS [PaidAmount],
				ISNULL(CE.FirstName + ' ' + CE.LastName,'') AS ProviderName,
				'' as BoardName,  
				'' AS MediumName,  
				'' AS StandardsName,  
				ISNULL(E.[Name],'') AS EnityName,
				OCI.TypeOfMapping,
				CONVERT(varchar, PaidDate, 106) as OrderDate,
				CONVERT(varchar, DATEADD(year, 1, O.PaidDate), 106) as [ExpireDate]
				INTO ##TeMp2
		FROM 
		OrderCartItems OCI
		INNER JOIN [Order] O ON OCI.OrderId=O.Id
		INNER JOIN ExpertiseMapping EM ON EM.Id=OCI.MappingId AND OCI.TypeOfMapping='Expertise'
		LEFT JOIN Expertise E ON E.Id=EM.ExpertiseId
		LEFT JOIN CareerExpert CE ON CE.Id=EM.EntityId AND EM.ModuleId=4
		WHERE O.EntityId=@Id AND O.ModuleId=@ModuleId

		-- Show allData in one Table
		SELECT * FROM ##TeMp
		UNION 
		SELECT * FROM ##TeMp1
		UNION
		SELECT * FROM ##TeMp2
		ORDER BY TypeOfMapping
END

GO
CREATE PROCEDURE [ClassBook_GetFavourites]
	@UserId INT
AS
BEGIN
	SELECT 
		CASE
			WHEN F.EntityName = 'Class'   THEN C.[Name]
		    WHEN F.EntityName = 'Teacher' THEN T.FirstName + ' ' + T.LastName
			WHEN F.EntityName = 'Course' THEN CS.[Name]
			WHEN F.EntityName = 'CareerExpert' THEN CE.FirstName + ' ' + CE.LastName
			ELSE ''
		END AS [Name],F.EntityName
		FROM Favourites F
		LEFT JOIN Classes C ON C.Id=F.EntityId AND F.EntityName='Class'
		LEFT JOIN Teacher T ON T.Id=F.EntityId AND F.EntityName='Teacher'
		LEFT JOIN Courses CS ON CS.Id=F.EntityId AND F.EntityName='Course'
		LEFT JOIN CareerExpert CE ON CE.Id=F.EntityId AND F.EntityName='CareerExpert'
	WHERE UserId=@UserId
	ORDER BY F.EntityName
END

GO
CREATE PROCEDURE [ChannelPartner_GetPromotionLevel]
	@ChannelPartnerId INT
AS
BEGIN
	SELECT
	@ChannelPartnerId as ChannelPartnerId,
	PC.Title as CurrentLevel,
	((Select ISNULL(Title,'') from PromotionalCycle PCsub where PCsub.LevelId=(PC.LevelId+1))) As NextLevel,
	PC.AchievementCount as [Target],
	CPM.CurrentCount As Achieved,
	(PC.AchievementCount-CPM.CurrentCount) as Pending
	from ChannelPartnerMapping CPM
	INNER JOIN PromotionalCycle PC ON PC.LevelId=CPM.LevelId 
	WHERE ChannelPartnerId=@ChannelPartnerId
END

GO
CREATE PROCEDURE [ClassBook_GetAllClasses]
	@UserId INT=0
AS
BEGIN
	DECLARE @TopProducts INT
	DECLARE @EntityId INT
	DECLARE @StateId INT
	DECLARE @CityId INT
	SET @TopProducts=5

	IF EXISTS(SELECT [name] FROM tempdb.sys.tables WHERE [name] like '#TempRating%') 
	BEGIN
		DROP TABLE #LocalCustomer;
	END;

	SELECT @EntityId=EntityId FROM Users WHERE Id=@UserId
	SELECT @CityId=CityId,@StateId=StateId FROM Student WHERE Id=@EntityId

	;WITH #TempRating(EntityId, EntityName,Ratings) as (
		SELECT 
		EntityId,EntityName,AVG(Rating) as Ratings
		FROM Ratings WHERE EntityName='Classes'
		GROUP BY EntityId,EntityName
	)

	SELECT Top(@TopProducts) Id,[Name],[Description],C.[ClassPhotoUrl] as PhotoUrl,0 as Favourite,10 as [Ordering],
	FORMAT(ISNULL(AVG(TR.Ratings),0.0),'N2') as Rating
	FROM Classes C
	LEFT JOIN #TempRating TR ON TR.EntityId=C.Id
	WHERE C.CityId=@CityId OR C.StateId=@StateId
	GROUP BY C.Id,C.Name,C.Description,TR.Ratings,C.[ClassPhotoUrl]
	UNION
	SELECT Top(@TopProducts) Id,[Name],[Description],C.[ClassPhotoUrl] as PhotoUrl,0 as Favourite,5 as [Ordering],
	FORMAT(ISNULL(AVG(TR.Ratings),0.0),'N2') as Rating
	FROM Classes C
	LEFT JOIN #TempRating TR ON TR.EntityId=C.Id
	GROUP BY C.Id,C.Name,C.Description,TR.Ratings,C.[ClassPhotoUrl]
	ORDER BY Rating,Ordering DESC
END