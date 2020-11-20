using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Domain.Teacher;
using ClassBookApplication.Extension;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ClassBookApplication.Service
{
    public class ClassBookService
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ChannelPartnerManagementContext _channelPartnerManagementContext;
        private readonly FileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClassBookModelFactory _classBookModelFactory;

        #endregion

        #region Ctor

        public ClassBookService(ClassBookManagementContext context,
            ChannelPartnerManagementContext channelPartnerManagementContext,
            FileService fileService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._channelPartnerManagementContext = channelPartnerManagementContext;
            this._fileService = fileService;
            this._configuration = configuration;
            this._env = env;
            this._httpContextAccessor = httpContextAccessor;
            this._classBookModelFactory = classBookModelFactory;
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
            user.EntityId = userId;
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
        public string SaveShoppingCartClassTeacher(Users user, AddToCartModelClassTeacher model, bool isRemove = false)
        {

            #region Save Mapping Data
            decimal DistanceLearningAmountSubject = 3000;
            decimal DistanceLearningAmountCourse = 5000;

            int mappingId;
            if (model.CourseId == 0)
            {
                var retrivedMappingData = _context.StandardMediumBoardMapping.Where(x => x.EntityId == user.EntityId && x.ModuleId == user.ModuleId
                    && x.BoardId == model.BoardId && x.MediumId == model.MediumId && x.StandardId == model.StandardId).FirstOrDefault();
                if (retrivedMappingData == null)
                {
                    if (isRemove)
                        return "Subject is not in the cart";

                    StandardMediumBoardMapping standardMediumBoardMapping = new StandardMediumBoardMapping();
                    standardMediumBoardMapping.EntityId = user.EntityId;
                    standardMediumBoardMapping.ModuleId = user.ModuleId;
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
                    var subjectMappingData = _context.SubjectMapping.Where(x => x.SMBId == mappingId && x.SubjectId == model.SubjectId);
                    if (subjectMappingData.Any())
                    {
                        var subMap = subjectMappingData.FirstOrDefault();
                        _context.SubjectMapping.Remove(subMap);
                        _context.SaveChanges();

                        var shoppingCartItemsData = _context.ShoppingCartItems.Where(x => x.EntityId == user.Id && x.ModuleId == user.ModuleId && x.MappingId == mappingId
                            && x.TypeOfMapping == ClassBookConstant.Mapping_Subject.ToString());
                        if (shoppingCartItemsData.Any())
                        {
                            _context.ShoppingCartItems.Remove(shoppingCartItemsData.FirstOrDefault());
                            _context.SaveChanges();
                        }
                        return "Subject Removed Successfully";
                    }
                    else
                    {
                        return "Subject is not in the cart";
                    }
                }
                else
                {
                    var subjectMappingsData = _context.SubjectMapping.Where(x => x.SMBId == mappingId && x.SubjectId == model.SubjectId);
                    if (!subjectMappingsData.Any())
                    {
                        SubjectMapping subjectMapping = new SubjectMapping();
                        subjectMapping.SMBId = mappingId;
                        subjectMapping.SubjectId = model.SubjectId;
                        subjectMapping.DistanceFees = 0;
                        subjectMapping.PhysicalFees = 0;
                        subjectMapping.Active = false;
                        _context.SubjectMapping.Add(subjectMapping);
                        _context.SaveChanges();

                        ShoppingCartItems shoppingCartItems = new ShoppingCartItems();
                        shoppingCartItems.MappingId = subjectMapping.Id;
                        shoppingCartItems.TypeOfMapping = ClassBookConstant.Mapping_Subject.ToString();
                        shoppingCartItems.Type = ClassBookConstant.LearningType_Distance.ToString();
                        shoppingCartItems.EntityId = user.EntityId;
                        shoppingCartItems.ModuleId = user.ModuleId;
                        shoppingCartItems.ActualAmount = 0;
                        shoppingCartItems.OurAmount = DistanceLearningAmountSubject;
                        _context.ShoppingCartItems.Add(shoppingCartItems);
                        _context.SaveChanges();
                        return "Subject Added Successfully";
                    }
                    else
                    {
                        return "Subject is already in the cart";
                    }
                }
            }
            else
            {
                var retrivedMappingData = _context.CourseMapping.Where(x => x.EntityId == user.EntityId && x.ModuleId == user.ModuleId
                                                && x.CourseId == model.CourseId).FirstOrDefault();
                if (isRemove)
                {
                    if (retrivedMappingData != null)
                    {
                        var courseMap = retrivedMappingData;
                        _context.CourseMapping.Remove(retrivedMappingData);
                        _context.SaveChanges();

                        var shoppingCartItemsData = _context.ShoppingCartItems.Where(x => x.EntityId == user.Id && x.ModuleId == user.ModuleId && x.MappingId == retrivedMappingData.Id
                            && x.TypeOfMapping == ClassBookConstant.Mapping_Course.ToString());
                        if (shoppingCartItemsData.Any())
                        {
                            _context.ShoppingCartItems.Remove(shoppingCartItemsData.FirstOrDefault());
                            _context.SaveChanges();
                        }
                        return "Course Removed Successfully";
                    }
                    else
                    {
                        return "Course is not in the cart";
                    }
                }
                else
                {

                    if (retrivedMappingData == null)
                    {
                        if (isRemove)
                            return "Course is not in the cart";

                        CourseMapping courseMapping = new CourseMapping();
                        courseMapping.EntityId = user.Id;
                        courseMapping.ModuleId = user.ModuleId;
                        courseMapping.CourseId = model.CourseId;
                        courseMapping.DistanceFees = 0;
                        courseMapping.PhysicalFees = 0;
                        courseMapping.Active = false;
                        _context.CourseMapping.Add(courseMapping);
                        _context.SaveChanges();

                        ShoppingCartItems shoppingCartItems = new ShoppingCartItems();
                        shoppingCartItems.MappingId = courseMapping.Id;
                        shoppingCartItems.TypeOfMapping = ClassBookConstant.Mapping_Course.ToString();
                        shoppingCartItems.Type = ClassBookConstant.LearningType_Distance.ToString();
                        shoppingCartItems.EntityId = user.Id;
                        shoppingCartItems.ModuleId = user.ModuleId;
                        shoppingCartItems.ActualAmount = 0;
                        shoppingCartItems.OurAmount = DistanceLearningAmountCourse;
                        _context.ShoppingCartItems.Add(shoppingCartItems);

                        return "Course added successfully to Cart";
                    }
                    else
                    {
                        return "Course is already in the Cart";
                    }
                }

            }
            #endregion
        }

        public string GetReferCode(int UserId, int ModuleId)
        {
            string referCode = string.Empty;
            if (ModuleId == (int)Module.Student)
                referCode = _context.Student.Where(x => x.Id == UserId).FirstOrDefault().ReferCode;
            else if (ModuleId == (int)Module.Classes)
                referCode = _context.Classes.Where(x => x.Id == UserId).FirstOrDefault().ReferCode;
            else if (ModuleId == (int)Module.Teacher)
                referCode = _context.Teacher.Where(x => x.Id == UserId).FirstOrDefault().ReferCode;
            else if (ModuleId == (int)Module.School)
                referCode = _context.School.Where(x => x.Id == UserId).FirstOrDefault().ReferCode;
            else if (ModuleId == (int)Module.CareerExpert)
                referCode = _context.CareerExpert.Where(x => x.Id == UserId).FirstOrDefault().ReferCode;
            return referCode;
        }

        /// <summary>
        /// Save All Mapping Data
        /// </summary>
        public string SaveShoppingCart(Users user, AddToCartModel model)
        {
            #region Save Mapping Data

            decimal DistanceLearningAmountSubject = 3000;
            decimal DistanceLearningAmountCourse = 5000;
            decimal DistanceLearningAmountExpertise = 5000;

            decimal PhysicalLearningAmountSubject = 2500;
            decimal PhysicalLearningAmountCourse = 5000;
            decimal PhysicalLearningAmountExpertise = 5000;

            var shoppingCartItemsData = _context.ShoppingCartItems.Where(x => x.EntityId == user.Id && x.ModuleId == user.ModuleId && x.MappingId == model.MappingId
                && x.TypeOfMapping == model.TypeOfMapping).FirstOrDefault();

            if (shoppingCartItemsData == null)
            {
                ShoppingCartItems shoppingCartItems = new ShoppingCartItems();
                shoppingCartItems.MappingId = model.MappingId;
                shoppingCartItems.TypeOfMapping = model.TypeOfMapping;
                shoppingCartItems.Type = model.Type;
                shoppingCartItems.EntityId = user.Id;
                shoppingCartItems.ModuleId = user.ModuleId;
                if (shoppingCartItems.Type == ClassBookConstant.LearningType_Distance.ToString())
                {
                    if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Subject.ToString())
                    {
                        var mappingData = _context.SubjectMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.DistanceFees;
                        shoppingCartItems.OurAmount = DistanceLearningAmountSubject;
                    }
                    else if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Course.ToString())
                    {
                        var mappingData = _context.CourseMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.DistanceFees;
                        shoppingCartItems.OurAmount = DistanceLearningAmountCourse;
                    }
                    else if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Expertise.ToString())
                    {
                        var mappingData = _context.ExpertiseMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.DistanceFees;
                        shoppingCartItems.OurAmount = DistanceLearningAmountExpertise;
                    }
                    else
                    {
                        return "Wrong type of Mapping";
                    }
                }
                else if (shoppingCartItems.Type == ClassBookConstant.LearningType_Physical.ToString())
                {
                    if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Subject.ToString())
                    {
                        var mappingData = _context.SubjectMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.PhysicalFees;
                        List<int> listSmbIds = new List<int>();
                        var query = from shop in _context.ShoppingCartItems
                                    join sub in _context.SubjectMapping on shop.MappingId equals sub.Id
                                    where shop.EntityId == user.Id && shop.ModuleId == user.ModuleId && shop.TypeOfMapping == "Subject" && shop.Type == "Physical"
                                    select sub.SMBId;
                        listSmbIds = query.ToList();

                        if (!listSmbIds.Contains(mappingData.SMBId))
                            shoppingCartItems.OurAmount = PhysicalLearningAmountSubject;
                        else
                            shoppingCartItems.ActualAmount = 0;
                    }
                    else if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Course.ToString())
                    {
                        var mappingData = _context.CourseMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.PhysicalFees;
                        shoppingCartItems.OurAmount = PhysicalLearningAmountCourse;
                    }
                    else if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Expertise.ToString())
                    {
                        var mappingData = _context.ExpertiseMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        shoppingCartItems.ActualAmount = mappingData.PhysicalFees;
                        shoppingCartItems.OurAmount = PhysicalLearningAmountExpertise;
                    }
                    else
                    {
                        return "Wrong type of Mapping";
                    }
                }
                else
                {
                    return "Wrong type pass";
                }
                _context.ShoppingCartItems.Add(shoppingCartItems);
                _context.SaveChanges();
                return model.TypeOfMapping + " successflly added to Cart";
            }
            else
            {
                return model.TypeOfMapping + " already Added to Cart";
            }

            #endregion
        }

        public string RemoveShoppingCart(Users user, AddToCartModel model)
        {
            #region Save Mapping Data

            decimal PhysicalLearningAmountSubject = 2500;
            var shoppingCartItems = _context.ShoppingCartItems.Where(x => x.EntityId == user.Id && x.ModuleId == user.ModuleId && x.MappingId == model.MappingId
                && x.TypeOfMapping == model.TypeOfMapping).FirstOrDefault();

            if (shoppingCartItems != null)
            {
                _context.ShoppingCartItems.Remove(shoppingCartItems);
                _context.SaveChanges();
                if (shoppingCartItems.TypeOfMapping == ClassBookConstant.Mapping_Subject.ToString())
                {
                    if (shoppingCartItems.OurAmount == PhysicalLearningAmountSubject)
                    {
                        var mappingData = _context.SubjectMapping.Where(x => x.Id == model.MappingId).FirstOrDefault();
                        List<int> listSmbIds = new List<int>();
                        var query = from shop in _context.ShoppingCartItems
                                    join sub in _context.SubjectMapping on shop.MappingId equals sub.Id
                                    where shop.EntityId == user.Id && shop.ModuleId == user.ModuleId && shop.TypeOfMapping == "Subject" && shop.Type == "Physical" && sub.SMBId == mappingData.SMBId
                                    select shop;
                        var shopping = query.FirstOrDefault();
                        if (shopping != null)
                        {
                            shopping.OurAmount = PhysicalLearningAmountSubject;
                            _context.ShoppingCartItems.Update(shopping);
                            _context.SaveChanges();
                        }
                    }
                }
                return model.TypeOfMapping + " successflly removed to Cart";
            }
            else
            {
                return model.TypeOfMapping + " not in the cart";
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
        /// Get All Module Data by Module Id
        /// </summary>
        public CartCompleteDetail GetCartDetailByUserId(int UserId, int moduleId)
        {
            CartCompleteDetail cartCompleteDetail = new CartCompleteDetail();
            List<CartDetailModel> cartDetailModels = new List<CartDetailModel>();
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
                cmd.Parameters.Add("@ClassBookHandlingAmount", SqlDbType.Decimal);
                cmd.Parameters["@ClassBookHandlingAmount"].Direction = ParameterDirection.Output;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CartDetailModel cartDetailModel = new CartDetailModel()
                        {
                            MappingId = reader.GetValue<int>("MappingId"),
                            ProviderType = reader.GetValue<string>("ProviderType"),
                            LearningType = reader.GetValue<string>("LearningType"),
                            ActualFees = reader.GetValue<decimal>("ActualFees"),
                            ProviderName = reader.GetValue<string>("ProviderName"),
                            BoardName = reader.GetValue<string>("BoardName"),
                            MediumName = reader.GetValue<string>("MediumName"),
                            StandardsName = reader.GetValue<string>("StandardsName"),
                            EnityName = reader.GetValue<string>("EnityName"),
                            TypeOfMapping = reader.GetValue<string>("TypeOfMapping"),
                        };
                        cartDetailModels.Add(cartDetailModel);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                cartCompleteDetail.CartDetailModel = cartDetailModels;
                cartCompleteDetail.ClassBookHandlingAmount = decimal.Parse(cmd.Parameters["@ClassBookHandlingAmount"].Value.ToString());
                cartCompleteDetail.TotalPrice = cartCompleteDetail.CartDetailModel.Sum(x => x.ActualFees);
                cartCompleteDetail.GrandTotal = cartCompleteDetail.TotalPrice + cartCompleteDetail.ClassBookHandlingAmount;
                cartCompleteDetail.GST = (cartCompleteDetail.GrandTotal * 18) / 100;
                cartCompleteDetail.GrandTotal = cartCompleteDetail.GrandTotal + cartCompleteDetail.GST;
                cartCompleteDetail.InternetHandlingCharge = (cartCompleteDetail.GrandTotal * 2) / 100;
                //close connection
                connection.Close();
                return cartCompleteDetail;
            }
        }

        /// <summary>
        /// Get All SubScription Data
        /// </summary>
        public List<SubscriptionDetailModel> GetSubscriptionDetailByUserId(int UserId, int moduleId)
        {
            List<SubscriptionDetailModel> subscriptionDetailModels = new List<SubscriptionDetailModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetSubscrptionDetailByUserId.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = UserId;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = moduleId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SubscriptionDetailModel subscriptionDetailModel = new SubscriptionDetailModel()
                        {
                            ProviderType = reader.GetValue<string>("ProviderType"),
                            LearningType = reader.GetValue<string>("LearningType"),
                            PaidAmount = reader.GetValue<decimal>("PaidAmount"),
                            ProviderName = reader.GetValue<string>("ProviderName"),
                            BoardName = reader.GetValue<string>("BoardName"),
                            MediumName = reader.GetValue<string>("MediumName"),
                            StandardsName = reader.GetValue<string>("StandardsName"),
                            EnityName = reader.GetValue<string>("EnityName"),
                            TypeOfMapping = reader.GetValue<string>("TypeOfMapping"),
                            SubscriptionDate = reader.GetValue<string>("SubscriptionDate"),
                            ExpireDate = reader.GetValue<string>("ExpireDate"),
                        };
                        subscriptionDetailModels.Add(subscriptionDetailModel);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return subscriptionDetailModels;
            }
        }

        /// <summary>
        /// Get All Transcation Data
        /// </summary>
        public List<TranscationDetailModel> GetTranscationDetailByUserId(int UserId, int moduleId)
        {
            List<TranscationDetailModel> transcationDetailModel = new List<TranscationDetailModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetTranscationDetailByUserId.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = UserId;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = moduleId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TranscationDetailModel subscriptionDetailModel = new TranscationDetailModel()
                        {
                            TranscatioNo = reader.GetValue<int>("TranscatioNo"),
                            ProviderType = reader.GetValue<string>("ProviderType"),
                            LearningType = reader.GetValue<string>("LearningType"),
                            PaidAmount = reader.GetValue<decimal>("PaidAmount"),
                            ProviderName = reader.GetValue<string>("ProviderName"),
                            EnityName = reader.GetValue<string>("EnityName"),
                            TypeOfMapping = reader.GetValue<string>("TypeOfMapping"),
                            OrderDate = reader.GetValue<string>("OrderDate"),
                            ExpireDate = reader.GetValue<string>("ExpireDate"),
                        };
                        transcationDetailModel.Add(subscriptionDetailModel);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return transcationDetailModel;
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

        public IList<CourseDetailsList> GetCourses()
        {
            IList<CourseDetailsList> courseDetailsList = new List<CourseDetailsList>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetCourses.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CourseDetailsList ISP = new CourseDetailsList()
                        {
                            Type = reader.GetValue<string>("Type"),
                            CourseName = reader.GetValue<string>("CourseName"),
                            CourseProviderName = reader.GetValue<string>("CourseProviderName"),
                            ImageUrl = _classBookModelFactory.PrepareURL(reader.GetValue<string>("ImageUrl")),
                            Rating = reader.GetValue<int>("Rating"),
                            CategoryName = reader.GetValue<string>("CategoryName"),
                        };
                        courseDetailsList.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return courseDetailsList;
            }
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public IList<SubjectDetails> GetSubjects(Users singleUser, SubjectRequestDetails subjectRequestDetails)
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
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = singleUser == null ? 0 : singleUser.Id;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = subjectRequestDetails.ModuleId;
                cmd.Parameters.Add("@EntityId", SqlDbType.Int).Value = subjectRequestDetails.EntityId;
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
                            Name = reader.GetValue<string>("Name"),
                            InCart = reader.GetValue<bool>("InCart"),
                            DistanceFees = reader.GetValue<decimal>("DistanceFees"),
                            PhysicalFees = reader.GetValue<decimal>("PhysicalFees"),
                            SubjectMappingId = reader.GetValue<int>("SubjectMappingId"),
                            OrderCartItemId = reader.GetValue<int>("OrderCartItemId"),
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
        public IList<FavouriteDetailModel> GetFavourite(int UserId)
        {
            IList<FavouriteDetailModel> favouriteDetailModel = new List<FavouriteDetailModel>();
            SqlConnection connection = new SqlConnection(GetConnectionString());
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = ClassBookConstant.SP_ClassBook_GetFavourites.ToString();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FavouriteDetailModel ISP = new FavouriteDetailModel()
                        {
                            Name = reader.GetValue<string>("Name"),
                            EntityName = reader.GetValue<string>("EntityName"),

                        };
                        favouriteDetailModel.Add(ISP);
                    }
                };
                //close up the reader, we're done saving results
                reader.Close();
                //close connection
                connection.Close();
                return favouriteDetailModel;
            }
        }

        /// <summary>
        /// Get All Moduel Data by Module Id
        /// </summary>
        public bool OrderPaid(int UserId, int ModuleId, string PaymentType)
        {
            var referCode = GetReferCode(UserId, ModuleId);
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
                cmd.Parameters.Add("@ReferCode", SqlDbType.VarChar).Value = referCode;
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
                teachers.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Classes);
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

        /// <summary>
        /// Get IntoducerName
        /// </summary>
        public CareerExpertData GetIntoducerName(string referCode, int ModuleId)
        {
            var query = from careerExpert in _context.CareerExpert
                        where careerExpert.ReferCode == referCode && careerExpert.Active == true
                        select new CareerExpertData
                        {
                            Name = careerExpert.FirstName + " " + careerExpert.LastName,
                            UniqueId = careerExpert.UniqueNo
                        };
            return query.FirstOrDefault();

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

        #region SendRegister

        public IRestResponse RegisterMethod(CommonRegistrationModel model, string ApiName)
        {
            var secretKey = _channelPartnerManagementContext.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            var client = new RestClient(ClassBookConstant.ChannelPartnerWebSite_HostURL.ToString() + ApiName.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Secret_Key", secretKey.Value.ToString());
            request.AddHeader("AuthorizeTokenKey", "Default");
            request.AddParameter("data", model.Data);
            request.AddParameter("DeviceId", model.DeviceId);
            request.AddParameter("FCMId", model.FCMId);
            IRestResponse response = client.Execute(request);
            return response;
        }

        public IRestResponse GetCommonFromChannelPartner(string ApiName)
        {
            var secretKey = _channelPartnerManagementContext.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            var client = new RestClient(ClassBookConstant.ChannelPartnerWebSite_HostURL.ToString() + ApiName.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Secret_Key", secretKey.Value.ToString());
            request.AddHeader("AuthorizeTokenKey", _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"]);
            IRestResponse response = client.Execute(request);
            return response;
        }
        #endregion



        #region Website

        public Task<List<CourseCategoryModel>> GetItemsAsync()
        {
            return _context.CourseCategory.Where(x => x.Active == true).
                Select(x => new CourseCategoryModel
                {
                    Name = x.Name,
                    ImageUrl = _classBookModelFactory.PrepareURL(x.ImageUrl),
                    Count = 25,
                    CategoryUrl = x.Name.ToLower().Replace(" ", "-")
                }).ToListAsync();
        }
        #endregion
    }
}
