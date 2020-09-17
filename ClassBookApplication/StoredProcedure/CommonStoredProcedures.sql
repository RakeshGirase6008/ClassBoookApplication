CREATE PROCEDURE [ClassBook_GetModuleDataByModuleId]
@ModuleId INT    
AS     
BEGIN    
	DECLARE @TopProducts INT
	SET @TopProducts=5

	 IF @ModuleId=2
	 BEGIN
		SELECT T.Id,
		T.FirstName + ' ' + T.LastName as [Name],
		T.[ProfilePictureUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(OS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Teacher T
		LEFT JOIN Ratings R ON R.EntityId=T.Id AND R.EntityName='Teacher'
		LEFT JOIN Users U ON U.UserId=T.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderSubjects OS ON  SMB.Id=OS.SMBId
		GROUP BY T.Id,T.[FirstName],T.LastName,T.[ProfilePictureUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=3
	 BEGIN
		SELECT Top(@TopProducts) C.Id,C.[Name],C.[ClassPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(StandardId) as StandardCount,
		COUNT(OS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Classes C 
		LEFT JOIN Ratings R ON R.EntityId=C.Id AND R.EntityName='Classes'
		LEFT JOIN Users U ON U.UserId=C.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderSubjects OS ON  SMB.Id=OS.SMBId
		GROUP BY C.Id,C.[Name],C.[ClassPhotoUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=4
	 BEGIN
		SELECT CE.Id,
		CE.FirstName + ' ' + CE.LastName as [Name],
		CE.[ProfilePictureUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(OS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM CareerExpert CE
		LEFT JOIN Ratings R ON R.EntityId=CE.Id AND R.EntityName='CareerExpert'
		LEFT JOIN Users U ON U.UserId=CE.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderSubjects OS ON  SMB.Id=OS.SMBId
		GROUP BY CE.Id,CE.[FirstName],CE.[LastName],CE.[ProfilePictureUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=5
	 BEGIN
		SELECT S.Id,S.[Name],S.[SchoolPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(OS.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM School S
		LEFT JOIN Ratings R ON R.EntityId=S.Id AND R.EntityName='School'
		LEFT JOIN Users U ON U.UserId=S.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderSubjects OS ON  SMB.Id=OS.SMBId
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
	DECLARE @UserId INT
	SELECT @UserId=Id from Users where UserId=@Id AND ModuleId=@ModuleId
	
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
	WHERE SMB.UserId=@UserId
END

GO
CREATE PROCEDURE [ClassBook_GetCartDetailByUserId]
@Id INT=0,
@ModuleId INT=0
As
BEGIN
	SELECT 
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
		INNER JOIN Subjects Sub ON Sub.Id=SCS.SubjectId
		LEFT JOIN PackageLevel PL ON PL.EntityLevel=SCS.LevelId  AND PL.ModuleId=@ModuleId  
	WHERE SMB.UserId=@Id
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
		SELECT S.Id,S.[Name] from Classes C    
		INNER JOIN Users U ON C.Id=U.UserId AND U.ModuleId=@ModuleId    
		INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId    
		INNER JOIN [OrderSubjects] OS ON OS.SMBId=SMB.Id
		INNER JOIN Subjects S ON S.Id=OS.SubjectId    
		WHERE U.UserId=@UserId    
	END  
  
	IF @ModuleId=2  
	BEGIN  
		select S.Id,S.[Name] from Teacher T    
		INNER JOIN Users U ON T.Id=U.UserId AND U.ModuleId=@ModuleId    
		INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId    
		INNER JOIN [OrderSubjects] OS ON OS.SMBId=SMB.Id
		INNER JOIN Subjects S ON S.Id=OS.SubjectId    
		WHERE U.UserId=@UserId  
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
 WHERE SMB.UserId=@UserId    
  
  --Updae the TotalAmount for Order Table
 SELECT @TotalAmount=ISNULL(SUM(Amount),0)  
 FROM OrderSubjects WHERE OrderId=@Id  
  
 UPDATE [Order]  
 SET TotalAmount=@TotalAmount  
 WHERE Id=@Id

 --Remove the ShoppingCartSubjects Data once the Order is Paid
 DELETE SCS 
 FROM Users U
 INNER JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
 INNER JOIN ShoppingCartSubjects SCS ON SCS.SMBId=SMB.Id
 WHERE U.Id=@UserId

END