using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Domain.Teacher;
using ClassBookApplication.Extension;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ClassBookApplication.Service
{
    public class ClassBookService
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly FileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public ClassBookService(ClassBookManagementContext context,
            FileService fileService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            this._fileService = fileService;
            this._configuration = configuration;
            this._env = env;
            this._httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Common

        /// <summary>
        /// Ge the ConnectionString
        /// </summary>
        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("ClassBookManagementeDatabase");
        }

        /// <summary>
        /// Save the Device Authorization Data
        /// </summary>
        public void SaveDeviceAuthorizationData(Users user, string DeviceId)
        {
            if (!_context.AuthorizeDeviceData.Where(x => x.DeviceId == DeviceId && x.UserId == user.Id).AsNoTracking().Any())
            {
                var AuthorizeDeviceData = new AuthorizeDeviceData
                {
                    UserId = user.Id,
                    DeviceId = DeviceId
                };
                _context.AuthorizeDeviceData.Add(AuthorizeDeviceData);
                _context.SaveChanges();
            }
        }
        /// <summary>
        /// Generate Random Token Key
        /// </summary>
        public string GenerateAuthorizeTokenKey()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789`@#$%^&*";
            var stringChars = new char[50];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        /// <summary>
        /// Generate UniqueNo based on Some parameters
        /// </summary>
        public string GenerateUniqueNo(string uniqueNo, string firstWord, string secondWord)
        {
            int sno;
            if (uniqueNo == null)
            {
                sno = 10001;
            }
            else
            {
                string tt = uniqueNo;
                int result = 0;
                bool success = int.TryParse(new string(tt
                                     .SkipWhile(x => !char.IsDigit(x))
                                     .TakeWhile(x => char.IsDigit(x))
                                     .ToArray()), out result);
                sno = result + 1;
            }
            string cpf = firstWord.Substring(0, 1).ToString().ToUpper();
            string cpl = secondWord.Substring(0, 1).ToString().ToUpper();
            uniqueNo = cpf + cpl + sno.ToString();
            return uniqueNo;
        }

        /// <summary>
        /// Generate password
        /// </summary>
        public string GeneratePassword(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, bool includeSpaces, int lengthOfPassword)
        {
            const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX)
            {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (includeLowercase)
            {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase)
            {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric)
            {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial)
            {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces)
            {
                characterSet += SPACE_CHARACTER;
            }

            char[] password = new char[lengthOfPassword];
            int characterSetLength = characterSet.Length;

            System.Random random = new System.Random();
            for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                bool moreThanTwoIdenticalInARow =
                    characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }

        /// <summary>
        /// Create Body From Parametes
        /// </summary>
        private string CreateBody(string username, string password, string link, string mypageName)
        {
            string webRootPath = _env.WebRootPath;
            string body = string.Empty;
            var target = Path.Combine(webRootPath + "/Content/HtmlTemplates/" + mypageName + ".html");
            using (StreamReader reader = new StreamReader(target.ToString()))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{username}", username);
            body = body.Replace("{password}", password);
            body = body.Replace("{link}", link);
            return body;
        }

        /// <summary>
        /// Create Body From Parametes
        /// </summary>
        public void SendVerificationLinkEmail(string ToEmailId, string GeneratedPassword, string title)
        {
            //var emailBody = CreateBody(ToEmailId, GeneratedPassword, string.Empty, "ActivateMyAccount");
            //title = title.ToString() + " Register";
            //SendEmail(ToEmailId, emailBody, title);
        }

        private bool SendEmail(string EmailTo, string EmailBody, string Subject)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("servicesautohub@gmail.com");
                mail.To.Add(EmailTo);
                mail.Subject = Subject;
                mail.Body = EmailBody;
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment("C:\\file.zip"));
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("servicesautohub@gmail.com", "@Ganapati20");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            return true;
        }

        /// <summary>
        /// SaveUserData
        /// </summary>
        public Users SaveUserData(int userId, Module module, string userName, string email, string FCMId, string deviceId)
        {
            var password = GeneratePassword(true, true, true, false, false, 16);
            Users user = new Users();
            user.UserId = userId;
            user.ModuleId = (int)module;
            user.UserName = userName;
            user.Email = email;
            user.Password = password;
            user.AuthorizeTokenKey = GenerateAuthorizeTokenKey();
            user.CreatedDate = DateTime.Now;
            user.Active = true;
            user.Deleted = false;
            user.FCMId = FCMId;
            _context.Users.Add(user);
            _context.SaveChanges();
            SaveDeviceAuthorizationData(user, deviceId);
            return user;
        }

        /// <summary>
        /// Save All Mapping Data
        /// </summary>
        public string SaveShoppingCart(int assignToId, AddToCartModel model, bool isRemove = false)
        {
            #region Save Mapping Data

            int mappingId;
            var retrivedMappingData = _context.StandardMediumBoardMapping.Where(x => x.UserId == assignToId && x.Active == true
                && x.BoardId == model.BoardId && x.MediumId == model.MediumId && x.StandardId == model.StandardId).FirstOrDefault();

            if (retrivedMappingData == null)
            {
                if (isRemove)
                    return "Subject is not in the cart";

                StandardMediumBoardMapping standardMediumBoardMapping = new StandardMediumBoardMapping();
                standardMediumBoardMapping.UserId = assignToId;
                standardMediumBoardMapping.BoardId = model.BoardId;
                standardMediumBoardMapping.MediumId = model.MediumId;
                standardMediumBoardMapping.StandardId = model.StandardId;
                standardMediumBoardMapping.Active = true;
                _context.StandardMediumBoardMapping.Add(standardMediumBoardMapping);
                _context.SaveChanges();
                mappingId = standardMediumBoardMapping.Id;
            }
            else
            {
                mappingId = retrivedMappingData.Id;
            }

            if (isRemove)
            {
                var subjectMappingData = _context.ShoppingCartSubjects.Where(x => x.SMBId == mappingId && x.SubjectId == model.SubjectId);
                if (subjectMappingData.Any())
                {
                    var subMap = subjectMappingData.FirstOrDefault();
                    _context.ShoppingCartSubjects.Remove(subMap);
                    _context.SaveChanges();

                    var levelCart = _context.ShoppingCartSubjects.Where(x => x.UserId == assignToId && x.LevelId > subMap.LevelId).ToList();
                    foreach (var item in levelCart)
                    {
                        int levId = item.LevelId - 1;
                        item.LevelId = levId;
                        _context.ShoppingCartSubjects.Update(item);
                        _context.SaveChanges();
                    }
                    // Update the LevelIds
                    return "Subject Removed Successfully";
                }
                else
                {
                    return "Subject is not in the cart";
                }
            }
            else
            {
                var shoppingCartData = _context.ShoppingCartSubjects.Where(x => x.SMBId == mappingId && x.SubjectId == model.SubjectId);
                if (!shoppingCartData.Any())
                {
                    var cartItems = _context.ShoppingCartSubjects.Where(x => x.UserId == assignToId);
                    var orderCartItems = from order in _context.Order
                                         join oi in _context.OrderSubjects on order.Id equals oi.OrderId
                                         where order.UserId == assignToId
                                         select new
                                         {
                                             id = oi.Id
                                         };
                    var listCount = orderCartItems.ToList().Count();

                    var levelId = cartItems.Count() + listCount + 1;
                    ShoppingCartSubjects shoppingCartItem = new ShoppingCartSubjects();
                    shoppingCartItem.SMBId = mappingId;
                    shoppingCartItem.UserId = assignToId;
                    shoppingCartItem.SubjectId = model.SubjectId;
                    shoppingCartItem.LevelId = levelId;
                    _context.ShoppingCartSubjects.Add(shoppingCartItem);
                    _context.SaveChanges();
                    return "Subject Added Successfully";
                }
                else
                {
                    return "Subject is already in the cart";
                }
            }

            #endregion
        }

        /// <summary>
        /// Save Course Mapping Data
        /// </summary>
        public void CourseCategoryMappingData(int moduleId, int assignToId, MappingRequestModel mappingData)
        {
            // Save Course Category Mapping
            if (!string.IsNullOrEmpty(mappingData.CourseCategoryIds))
            {
                var existingCourseCategoryIds = _context.CourseCategoryMapping.Where(x => x.ModuleId == moduleId && x.AssignToId == assignToId).ToList();
                var stringCourseCategory = mappingData.CourseCategoryIds.Split(",").ToList();

                //delete CourseCategories
                foreach (var existingCourseCategoryId in existingCourseCategoryIds)
                {
                    if (!stringCourseCategory.Contains(existingCourseCategoryId.CourseCategoryId.ToString()))
                    {
                        _context.CourseCategoryMapping.Remove(existingCourseCategoryId);
                        _context.SaveChanges();
                    }
                }

                // Add CourseCategories
                foreach (var courseCategoryId in stringCourseCategory)
                {
                    if (!_context.CourseCategoryMapping.Any(x => x.ModuleId == moduleId && x.AssignToId == assignToId && x.CourseCategoryId == int.Parse(courseCategoryId)))
                    {
                        CourseCategoryMapping courseCategoryMapping = new CourseCategoryMapping();
                        courseCategoryMapping.CourseCategoryId = int.Parse(courseCategoryId);
                        courseCategoryMapping.ModuleId = moduleId;
                        courseCategoryMapping.AssignToId = assignToId;
                        _context.CourseCategoryMapping.Add(courseCategoryMapping);
                        _context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Save Subject SpecialityMapping
        /// </summary>
        public void SubjectSpecialityMappingData(int moduleId, int assignToId, MappingRequestModel mappingData)
        {
            //Save Subject Speciality Mapping
            if (!string.IsNullOrEmpty(mappingData.SubjectSpecialityIds))
            {
                var existingSubjectSpecialityIds = _context.SubjectSpecialityMapping.Where(x => x.ModuleId == moduleId && x.AssignToId == assignToId).ToList();
                var stringSubjectSpeciality = mappingData.SubjectSpecialityIds.Split(",");

                //delete SubjectSpeciality
                foreach (var existingSubjectSpecialityId in existingSubjectSpecialityIds)
                {
                    if (!stringSubjectSpeciality.Contains(existingSubjectSpecialityId.SubjectSpecialityId.ToString()))
                    {
                        _context.SubjectSpecialityMapping.Remove(existingSubjectSpecialityId);
                        _context.SaveChanges();
                    }
                }

                //Add SubjectSpeciality
                foreach (var subjectSpecialityId in stringSubjectSpeciality)
                {
                    if (!_context.SubjectSpecialityMapping.Any(x => x.ModuleId == moduleId && x.AssignToId == assignToId && x.SubjectSpecialityId == int.Parse(subjectSpecialityId)))
                    {
                        SubjectSpecialityMapping subjectSpecialityMapping = new SubjectSpecialityMapping();
                        subjectSpecialityMapping.SubjectSpecialityId = int.Parse(subjectSpecialityId);
                        subjectSpecialityMapping.ModuleId = moduleId;
                        subjectSpecialityMapping.AssignToId = assignToId;
                        _context.SubjectSpecialityMapping.Add(subjectSpecialityMapping);
                        _context.SaveChanges();
                    }
                }
            }
        }

        #endregion

        #region Student

        /// <summary>
        /// Save the Student Record
        /// </summary>
        public (int studentId, string UniqueNo) SaveStudent(Student studentData, List<IFormFile> files)
        {
            Student stud = new Student();
            stud.FirstName = studentData.FirstName;
            stud.LastName = studentData.LastName;
            stud.Address = studentData.Address;
            stud.Email = studentData.Email;
            stud.Gender = studentData.Gender;
            stud.DOB = studentData.DOB;
            stud.StateId = studentData.StateId;
            stud.CityId = studentData.StateId;
            stud.ContactNo = studentData.ContactNo;
            stud.Pincode = studentData.Pincode;
            stud.BoardId = studentData.BoardId;
            stud.MediumId = studentData.MediumId;
            stud.StandardId = studentData.StandardId;
            stud.RegistrationNo = studentData.RegistrationNo;
            if (files?.Count > 0)
                stud.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Student);
            stud.ReferCode = studentData.ReferCode;
            var previousUnique = _context.Student.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            stud.UniqueNo = GenerateUniqueNo(previousUnique, studentData.FirstName, studentData.LastName);
            stud.RegistrationFromTypeId = studentData.RegistrationFromTypeId;
            stud.RegistrationByTypeId = studentData.RegistrationByTypeId;
            stud.CreatedDate = DateTime.Now;
            stud.CreatedBy = 0;
            stud.Active = true;
            stud.Deleted = false;
            _context.Student.Add(stud);
            _context.SaveChanges();
            return (stud.Id, stud.UniqueNo);
        }

        /// <summary>
        /// Update the Student Record
        /// </summary>
        public int UpdateStudent(Student studentData, Student stud, List<IFormFile> files)
        {
            stud.FirstName = studentData.FirstName;
            stud.LastName = studentData.LastName;
            stud.Address = studentData.Address;
            stud.Email = studentData.Email;
            stud.Gender = studentData.Gender;
            stud.DOB = studentData.DOB;
            stud.StateId = studentData.StateId;
            stud.CityId = studentData.StateId;
            stud.ContactNo = studentData.ContactNo;
            stud.Pincode = studentData.Pincode;
            stud.BoardId = studentData.BoardId;
            stud.MediumId = studentData.MediumId;
            stud.StandardId = studentData.StandardId;
            stud.RegistrationNo = studentData.RegistrationNo;
            if (files?.Count > 0)
                stud.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Student);
            stud.ReferCode = studentData.ReferCode;
            stud.UpdatedDate = DateTime.Now;
            stud.UpdatedBy = 0;
            _context.Student.Update(stud);
            _context.SaveChanges();
            return stud.Id;
        }

        #endregion

        #region Classes

        /// <summary>
        /// Save the Classes Record
        /// </summary>
        public (int classId, string UniqueNo) SaveClasses(Classes classesData, List<IFormFile> files, List<IFormFile> video)
        {
            Classes classes = new Classes();
            classes.Name = classesData.Name;
            classes.Email = classesData.Email;
            classes.ContactNo = classesData.ContactNo;
            classes.AlternateContact = classesData.AlternateContact;
            classes.RegistrationNo = classesData.RegistrationNo;
            if (files?.Count > 0)
            {
                //classes.LogoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
                classes.ClassPhotoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
            }
            if (video?.Count > 0)
                classes.IntroductionURL = _fileService.SaveFile(video, ClassBookConstant.VideoPath_Classes);
            classes.EstablishmentDate = classesData.EstablishmentDate;
            classes.RegistrationNo = classesData.RegistrationNo;
            classes.Address = classesData.Address;
            classes.StateId = classesData.StateId;
            classes.CityId = classesData.StateId;
            classes.Pincode = classesData.Pincode;
            classes.ApproveStatus = classesData.ApproveStatus;
            classes.ApprovalDate = classesData.ApprovalDate;
            classes.TeachingExperience = classesData.TeachingExperience;
            classes.Description = classesData.Description;
            classes.ReferCode = classesData.ReferCode;
            var previousUnique = _context.Classes.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            classes.UniqueNo = GenerateUniqueNo(previousUnique, classesData.Name, classesData.Email);
            classes.RegistrationFromTypeId = classesData.RegistrationFromTypeId;
            classes.RegistrationByTypeId = classesData.RegistrationByTypeId;
            classes.CreatedDate = DateTime.Now;
            classes.CreatedBy = 0;
            classes.Active = true;
            classes.Deleted = false;
            _context.Classes.Add(classes);
            _context.SaveChanges();
            return (classes.Id, classes.UniqueNo);
        }

        /// <summary>
        /// Update the Classes Record
        /// </summary>
        public int UpdateClasses(Classes classesData, Classes classes, List<IFormFile> files)
        {
            classes.Name = classesData.Name;
            classes.Email = classesData.Email;
            classes.ContactNo = classesData.ContactNo;
            classes.AlternateContact = classesData.AlternateContact;
            classes.RegistrationNo = classesData.RegistrationNo;
            if (files?.Count > 0)
            {
                classes.LogoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
                classes.ClassPhotoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
            }
            classes.EstablishmentDate = classesData.EstablishmentDate;
            classes.RegistrationNo = classesData.RegistrationNo;
            classes.Address = classesData.Address;
            classes.StateId = classesData.StateId;
            classes.CityId = classesData.StateId;
            classes.Pincode = classesData.Pincode;
            classes.ApproveStatus = classesData.ApproveStatus;
            classes.ApprovalDate = classesData.ApprovalDate;
            classes.TeachingExperience = classesData.TeachingExperience;
            classes.Description = classesData.Description;
            classes.ReferCode = classesData.ReferCode;
            classes.UpdatedDate = DateTime.Now;
            classes.UpdatedBy = 0;
            _context.Classes.Update(classes);
            _context.SaveChanges();
            return classes.Id;
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<ListingModel> GetModuleDataByModuleId(int ModuleId)
        {
            IList<ListingModel> listingModels = new List<ListingModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetModuleDataByModuleId.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = ModuleId;
                var reader = cmd.ExecuteReader();
                var hostName = _httpContextAccessor.HttpContext.Request.Host.Value;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ListingModel ISP = new ListingModel()
                        {
                            Id = reader.GetValue<int>("Id"),
                            Title = reader.GetValue<string>("Name"),
                            Image = reader.GetValue<string>("PhotoUrl") == null ? string.Empty : ClassBookConstant.WebSite_HostURL.ToString() + "/" + reader.GetValue<string>("PhotoUrl")?.Replace("\\", "/"),
                            Rating = reader.GetValue<string>("Rating"),
                            TotalBoard = reader.GetValue<int>("BoardCount"),
                            TotalStandard = reader.GetValue<int>("StandardCount"),
                            TotalSubject = reader.GetValue<int>("SubjectCount")
                        };
                        listingModels.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return listingModels;
            }
        }


        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<CartDetailModel> GetCartDetailByUserId(int UserId, int moduleId)
        {
            IList<CartDetailModel> cartDetailModels = new List<CartDetailModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetCartDetailByUserId.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = UserId;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = moduleId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CartDetailModel cartDetailModel = new CartDetailModel()
                        {
                            Board = reader.GetValue<string>("BoardName"),
                            Medium = reader.GetValue<string>("MediumName"),
                            Standard = reader.GetValue<string>("StandardsName"),
                            Subject = reader.GetValue<string>("SubjectName"),
                            Amount = reader.GetValue<decimal>("Amount")
                        };
                        cartDetailModels.Add(cartDetailModel);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return cartDetailModels;
            }
        }


        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<BoardMediumStandardModel> GetDetailById(int Id, int moduleId)
        {
            IList<BoardMediumStandardModel> boardMediumStandardModel = new List<BoardMediumStandardModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetDetailById.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = moduleId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        BoardMediumStandardModel ISP = new BoardMediumStandardModel()
                        {
                            Board = reader.GetValue<string>("BoardName"),
                            BoardId = reader.GetValue<int>("BoardId"),
                            Medium = reader.GetValue<string>("MediumName"),
                            MediumId = reader.GetValue<int>("MediumId"),
                            Standard = reader.GetValue<string>("StandardsName"),
                            StandardsId = reader.GetValue<int>("StandardsId"),
                        };
                        boardMediumStandardModel.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return boardMediumStandardModel;
            }
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<SubjectDetails> GetSubjects(SubjectRequestDetails subjectRequestDetails)
        {
            IList<SubjectDetails> subjectDetails = new List<SubjectDetails>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetSubjects.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = subjectRequestDetails.ModuleId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = subjectRequestDetails.Id;
                cmd.Parameters.Add("@BoardId", SqlDbType.Int).Value = subjectRequestDetails.BoardId;
                cmd.Parameters.Add("@MediumId", SqlDbType.Int).Value = subjectRequestDetails.MediumId;
                cmd.Parameters.Add("@StandardId", SqlDbType.Int).Value = subjectRequestDetails.StandardId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SubjectDetails ISP = new SubjectDetails()
                        {
                            Id = reader.GetValue<int>("Id"),
                            Name = reader.GetValue<string>("Name")
                        };
                        subjectDetails.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return subjectDetails;
            }
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public bool OrderPaid(int UserId, int ModuleId, string PaymentType)
        {
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_OrderPaid.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = ModuleId;
                cmd.Parameters.Add("@PaymentType", SqlDbType.VarChar).Value = PaymentType;
                var reader = cmd.ExecuteNonQuery();
                //close connection
                connection.Close();
                return true;
            }
        }

        #endregion

        #region Teacher

        /// <summary>
        /// Save the Teacher Record
        /// </summary>
        public (int teacherId, string UniqueNo) SaveTeacher(Teacher teacherData, List<IFormFile> files, List<IFormFile> video)
        {
            Teacher teachers = new Teacher();
            teachers.FirstName = teacherData.FirstName;
            teachers.LastName = teacherData.LastName;
            teachers.Email = teacherData.Email;
            teachers.ContactNo = teacherData.ContactNo;
            teachers.AlternateContact = teacherData.AlternateContact;
            teachers.Gender = teacherData.Gender;
            if (files?.Count > 0)
                teachers.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Teacher);
            if (video?.Count > 0)
                teachers.IntroductionURL = _fileService.SaveFile(video, ClassBookConstant.VideoPath_Teacher);
            teachers.DOB = teacherData.DOB;
            teachers.Address = teacherData.Address;
            teachers.StateId = teacherData.StateId;
            teachers.CityId = teacherData.StateId;
            teachers.Pincode = teacherData.Pincode;
            teachers.ApproveStatus = teacherData.ApproveStatus;
            teachers.ApprovalDate = teacherData.ApprovalDate;
            teachers.TeachingExperience = teacherData.TeachingExperience;
            teachers.Description = teacherData.Description;
            teachers.ReferCode = teacherData.ReferCode;
            var previousUnique = _context.Classes.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            teachers.UniqueNo = GenerateUniqueNo(previousUnique, teacherData.FirstName, teacherData.LastName);
            teachers.RegistrationFromTypeId = teacherData.RegistrationFromTypeId;
            teachers.RegistrationByTypeId = teacherData.RegistrationByTypeId;
            teachers.CreatedDate = DateTime.Now;
            teachers.CreatedBy = 0;
            teachers.Active = true;
            teachers.Deleted = false;
            _context.Teacher.Add(teachers);
            _context.SaveChanges();
            return (teachers.Id, teachers.UniqueNo);
        }

        /// <summary>
        /// Update the Teacher Record
        /// </summary>
        public int UpdateTeachers(Teacher teacherData, Teacher teachers, List<IFormFile> files)
        {
            teachers.FirstName = teacherData.FirstName;
            teachers.LastName = teacherData.LastName;
            teachers.Email = teacherData.Email;
            teachers.ContactNo = teacherData.ContactNo;
            teachers.AlternateContact = teacherData.AlternateContact;
            teachers.Gender = teacherData.Gender;
            if (files?.Count > 0)
            {
                teachers.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
            }
            teachers.DOB = teacherData.DOB;
            teachers.Address = teacherData.Address;
            teachers.StateId = teacherData.StateId;
            teachers.CityId = teacherData.StateId;
            teachers.Pincode = teacherData.Pincode;
            teachers.ApproveStatus = teacherData.ApproveStatus;
            teachers.ApprovalDate = teacherData.ApprovalDate;
            teachers.TeachingExperience = teacherData.TeachingExperience;
            teachers.Description = teacherData.Description;
            teachers.ReferCode = teacherData.ReferCode;
            teachers.UpdatedDate = DateTime.Now;
            teachers.UpdatedBy = 0;
            _context.Teacher.Update(teachers);
            _context.SaveChanges();
            return teachers.Id;
        }

        #endregion

        #region CareerExpert

        /// <summary>
        /// Save the CareerExpert Record
        /// </summary>
        public (int classId, string UniqueNo) SaveCareerExpert(CareerExpert CareerExpertData, List<IFormFile> files)
        {
            CareerExpert CareerExpert = new CareerExpert();
            CareerExpert.FirstName = CareerExpertData.FirstName;
            CareerExpert.LastName = CareerExpertData.LastName;
            CareerExpert.Email = CareerExpertData.Email;
            CareerExpert.ContactNo = CareerExpertData.ContactNo;
            CareerExpert.AlternateContact = CareerExpertData.AlternateContact;
            CareerExpert.Gender = CareerExpertData.Gender;
            if (files?.Count > 0)
            {
                CareerExpert.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_CareerExpert);
            }
            CareerExpert.DOB = CareerExpertData.DOB;
            CareerExpert.Address = CareerExpertData.Address;
            CareerExpert.StateId = CareerExpertData.StateId;
            CareerExpert.CityId = CareerExpertData.StateId;
            CareerExpert.Pincode = CareerExpertData.Pincode;
            CareerExpert.ApproveStatus = CareerExpertData.ApproveStatus;
            CareerExpert.ApprovalDate = CareerExpertData.ApprovalDate;
            CareerExpert.TeachingExperience = CareerExpertData.TeachingExperience;
            CareerExpert.Description = CareerExpertData.Description;
            CareerExpert.ReferCode = CareerExpertData.ReferCode;
            var previousUnique = _context.CareerExpert.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            CareerExpert.UniqueNo = GenerateUniqueNo(previousUnique, CareerExpertData.FirstName, CareerExpertData.Email);
            CareerExpert.RegistrationFromTypeId = CareerExpertData.RegistrationFromTypeId;
            CareerExpert.RegistrationByTypeId = CareerExpertData.RegistrationByTypeId;
            CareerExpert.CreatedDate = DateTime.Now;
            CareerExpert.CreatedBy = 0;
            CareerExpert.Active = true;
            CareerExpert.Deleted = false;
            _context.CareerExpert.Add(CareerExpert);
            _context.SaveChanges();
            return (CareerExpert.Id, CareerExpert.UniqueNo);
        }

        /// <summary>
        /// Update the CareerExpert Record
        /// </summary>
        public int UpdateCareerExpert(CareerExpert CareerExpertData, CareerExpert CareerExpert, List<IFormFile> files)
        {
            CareerExpert.FirstName = CareerExpertData.FirstName;
            CareerExpert.LastName = CareerExpertData.LastName;
            CareerExpert.Email = CareerExpertData.Email;
            CareerExpert.ContactNo = CareerExpertData.ContactNo;
            CareerExpert.AlternateContact = CareerExpertData.AlternateContact;
            CareerExpert.Gender = CareerExpertData.Gender;
            if (files?.Count > 0)
            {
                CareerExpert.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_CareerExpert);
            }
            CareerExpert.DOB = CareerExpertData.DOB;
            CareerExpert.Address = CareerExpertData.Address;
            CareerExpert.StateId = CareerExpertData.StateId;
            CareerExpert.CityId = CareerExpertData.StateId;
            CareerExpert.Pincode = CareerExpertData.Pincode;
            CareerExpert.ApproveStatus = CareerExpertData.ApproveStatus;
            CareerExpert.ApprovalDate = CareerExpertData.ApprovalDate;
            CareerExpert.TeachingExperience = CareerExpertData.TeachingExperience;
            CareerExpert.Description = CareerExpertData.Description;
            CareerExpert.ReferCode = CareerExpertData.ReferCode;
            CareerExpert.UpdatedDate = DateTime.Now;
            CareerExpert.UpdatedBy = 0;
            _context.CareerExpert.Update(CareerExpert);
            _context.SaveChanges();
            return CareerExpert.Id;
        }

        #endregion

        #region School

        /// <summary>
        /// Save the School Record
        /// </summary>
        public (int classId, string UniqueNo) SaveSchool(School schoolData, List<IFormFile> files)
        {
            School School = new School();
            School.Name = schoolData.Name;
            School.Email = schoolData.Email;
            School.ContactNo = schoolData.ContactNo;
            School.AlternateContact = schoolData.AlternateContact;
            School.RegistrationNo = schoolData.RegistrationNo;
            if (files?.Count > 0)
            {
                School.SchoolPhotoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_School);
            }
            School.EstablishmentDate = schoolData.EstablishmentDate;
            School.RegistrationNo = schoolData.RegistrationNo;
            School.Address = schoolData.Address;
            School.StateId = schoolData.StateId;
            School.CityId = schoolData.StateId;
            School.Pincode = schoolData.Pincode;
            School.ApproveStatus = schoolData.ApproveStatus;
            School.ApprovalDate = schoolData.ApprovalDate;
            School.TeachingExperience = schoolData.TeachingExperience;
            School.Description = schoolData.Description;
            var previousUnique = _context.School.OrderByDescending(x => x.Id).Select(x => x.UniqueNo).FirstOrDefault();
            School.UniqueNo = GenerateUniqueNo(previousUnique, schoolData.Name, schoolData.Email);
            School.RegistrationFromTypeId = schoolData.RegistrationFromTypeId;
            School.RegistrationByTypeId = schoolData.RegistrationByTypeId;
            School.CreatedDate = DateTime.Now;
            School.CreatedBy = 0;
            School.Active = true;
            School.Deleted = false;
            _context.School.Add(School);
            _context.SaveChanges();
            return (School.Id, School.UniqueNo);
        }

        /// <summary>
        /// Update the School Record
        /// </summary>
        public int UpdateSchool(School schoolData, School School, List<IFormFile> files)
        {
            School.Name = schoolData.Name;
            School.Email = schoolData.Email;
            School.ContactNo = schoolData.ContactNo;
            School.AlternateContact = schoolData.AlternateContact;
            School.RegistrationNo = schoolData.RegistrationNo;
            if (files?.Count > 0)
            {
                School.SchoolPhotoUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_School);
            }
            School.EstablishmentDate = schoolData.EstablishmentDate;
            School.RegistrationNo = schoolData.RegistrationNo;
            School.Address = schoolData.Address;
            School.StateId = schoolData.StateId;
            School.CityId = schoolData.StateId;
            School.Pincode = schoolData.Pincode;
            School.ApproveStatus = schoolData.ApproveStatus;
            School.ApprovalDate = schoolData.ApprovalDate;
            School.TeachingExperience = schoolData.TeachingExperience;
            School.Description = schoolData.Description;
            School.UpdatedDate = DateTime.Now;
            School.UpdatedBy = 0;
            _context.School.Update(School);
            _context.SaveChanges();
            return School.Id;
        }

        #endregion
    }
}
