CREATE PROCEDURE [dbo].[GetModuleDataByModuleId]
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
		COUNT(OI.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Teacher T
		LEFT JOIN Ratings R ON R.EntityId=T.Id AND R.EntityName='Teacher'
		LEFT JOIN Users U ON U.UserId=T.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderItems OI ON  SMB.Id=OI.SMBId
		GROUP BY T.Id,T.[FirstName],T.LastName,T.[ProfilePictureUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=3
	 BEGIN
		SELECT Top(@TopProducts) C.Id,C.[Name],C.[ClassPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(StandardId) as StandardCount,
		COUNT(OI.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM Classes C 
		LEFT JOIN Ratings R ON R.EntityId=C.Id AND R.EntityName='Classes'
		LEFT JOIN Users U ON U.UserId=C.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderItems OI ON  SMB.Id=OI.SMBId
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
		COUNT(OI.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM CareerExpert CE
		LEFT JOIN Ratings R ON R.EntityId=CE.Id AND R.EntityName='CareerExpert'
		LEFT JOIN Users U ON U.UserId=CE.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderItems OI ON  SMB.Id=OI.SMBId
		GROUP BY CE.Id,CE.[FirstName],CE.[LastName],CE.[ProfilePictureUrl]
		ORDER BY Rating DESC
	 END
	 ELSE IF @ModuleId=5
	 BEGIN
		SELECT S.Id,S.[Name],S.[SchoolPhotoUrl] as PhotoUrl,
		COUNT(DISTINCT(BoardId)) as BoardCount,  
		COUNT(DISTINCT(MediumId)) as MediumCount,  
		COUNT(DISTINCT(StandardId)) as StandardCount,  
		COUNT(OI.SubjectId) as SubjectCount,
		FORMAT(ISNULL(AVG(R.Rating),0.0),'N2') as Rating
		FROM School S
		LEFT JOIN Ratings R ON R.EntityId=S.Id AND R.EntityName='School'
		LEFT JOIN Users U ON U.UserId=S.Id AND U.ModuleId=@ModuleId
		LEFT JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
		LEFT JOIN OrderItems OI ON  SMB.Id=OI.SMBId
		GROUP BY S.Id,S.[Name],S.[SchoolPhotoUrl]
		ORDER BY Rating DESC
	 END
END

GO
CREATE PROCEDURE [GetSubjects]
@ModuleId INT,    
@UserId INT,    
@BoardId INT,    
@MediumId INT,    
@StandardId INT    
AS    
BEGIN    
 IF @ModuleId=3  
 BEGIN  
  select S.Id,S.[Name] from Classes C    
  INNER JOIN Users U ON C.Id=U.UserId AND U.ModuleId=@ModuleId    
  INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId    
  INNER JOIN [OrderItems] OI ON OI.SMBId=SMB.Id
  INNER JOIN Subjects S ON S.Id=OI.SubjectId    
  WHERE U.UserId=@UserId    
 END  
  
 IF @ModuleId=2  
 BEGIN  
  select S.Id,S.[Name] from Teacher T    
  INNER JOIN Users U ON T.Id=U.UserId AND U.ModuleId=@ModuleId    
  INNER JOIN StandardMediumBoardMapping SMB ON SMB.BoardId=@BoardId AND SMB.MediumId=@MediumId AND SMB.StandardId=@StandardId    
  INNER JOIN [OrderItems] OI ON OI.SMBId=SMB.Id
  INNER JOIN Subjects S ON S.Id=OI.SubjectId    
  WHERE U.UserId=@UserId  
 END  
   
END 

Go
CREATE PROCEDURE [OrderPaid] 
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
  
 --Add the ShoppingCartData into OrderItems with Amount
 INSERT INTO OrderItems  
 SELECT @Id,SMB.Id,Sub.Id,ISNULL(PL.Amount,0)  
 FROM StandardMediumBoardMapping SMB    
 INNER JOIN ShoppingCartItem SCI ON SCI.SMBId=SMB.Id    
 INNER JOIN Subjects Sub ON Sub.Id=SCI.SubjectId    
 INNER JOIN PackageLevel PL ON PL.EntityLevel=SCI.LevelId AND PL.ModuleId=@ModuleId  
 WHERE SMB.UserId=@UserId    
  
  --Updae the TotalAmount for Order Table
 SELECT @TotalAmount=ISNULL(SUM(Amount),0)  
 FROM OrderItems WHERE OrderId=@Id  
  
 UPDATE [Order]  
 SET TotalAmount=@TotalAmount  
 WHERE Id=@Id

 --Remove the ShoppingCartItem Data once the Order is Paid
 DELETE SCI 
 FROM Users U
 INNER JOIN StandardMediumBoardMapping SMB ON U.Id=SMB.UserId
 INNER JOIN ShoppingCartItem SCI ON SCI.SMBId=SMB.Id
 WHERE U.Id=@UserId

END

Go
CREATE PROCEDURE [dbo].[GetDetailById]
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
