using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Domain.Teacher;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
        private readonly LogsService _logsService;

        #endregion

        #region Ctor

        public ClassBookService(ClassBookManagementContext context,
            FileService fileService,
            LogsService logsService)
        {
            this._context = context;
            this._fileService = fileService;
            this._logsService = logsService;
        }

        #endregion

        #region Common

        /// <summary>
        /// Generate Random Token Key
        /// </summary>
        public string GenerateAuthorizeTokenKey()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
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
            string body = string.Empty;
            var target = Path.Combine(Directory.GetCurrentDirectory() + "\\", "Content\\HtmlTemplates\\" + mypageName + ".html");
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
            var emailBody = CreateBody(ToEmailId, GeneratedPassword, string.Empty, "ActivateMyAccount");
            title = title.ToString() + " Register";
            SendEmail(ToEmailId, emailBody, title);
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
        public Users SaveUserData(int userId, Module module, string userName, string email)
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
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        /// <summary>
        /// Save All Mapping Data
        /// </summary>
        public void SaveMappingData(int moduleId, int assignToId, MappingRequestModel mappingData)
        {
            if (mappingData != null)
            {
                CourseCategoryMappingData(moduleId, assignToId, mappingData);
                SubjectSpecialityMappingData(moduleId, assignToId, mappingData);
                StandardMediumBoardMappingData(moduleId, assignToId, mappingData);
            }
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

        /// <summary>
        /// Save Board Meduim Standard Mapping
        /// </summary>
        public void StandardMediumBoardMappingData(int moduleId, int assignToId, MappingRequestModel mappingData)
        {
            List<int> InsertedBoardIds = new List<int>();
            List<int> InsertedMediumIds = new List<int>();

            #region SavingBoardMapping

            if (!string.IsNullOrEmpty(mappingData.BoardIds))
            {
                var existingBoardIds = _context.BoardMapping.Where(x => x.ModuleId == moduleId && x.AssignToId == assignToId).ToList();
                var stringBoard = mappingData.BoardIds.Split(",");

                //Deactiving the Board Mapping
                foreach (var existingBoardId in existingBoardIds)
                {
                    if (!stringBoard.Contains(existingBoardId.BoardId.ToString()))
                    {
                        existingBoardId.Active = false;
                        _context.BoardMapping.Update(existingBoardId);
                        _context.SaveChanges();

                        //// Remove Unwanted Records
                        var removeStandardBoardMappings = _context.StandardMediumBoardMapping.Where(x => x.BoardMappingId == existingBoardId.Id).ToList();
                        _context.StandardMediumBoardMapping.RemoveRange(removeStandardBoardMappings);
                        _context.SaveChanges();

                    }
                }

                foreach (var boardId in stringBoard)
                {
                    var existingBoardMappingRecord = _context.BoardMapping.Where(x => x.ModuleId == moduleId && x.AssignToId == assignToId && x.BoardId == int.Parse(boardId)).FirstOrDefault();
                    if (existingBoardMappingRecord == null)
                    {
                        BoardMapping boardMapping = new BoardMapping();
                        boardMapping.BoardId = int.Parse(boardId);
                        boardMapping.ModuleId = moduleId;
                        boardMapping.AssignToId = assignToId;
                        boardMapping.Active = true;
                        _context.BoardMapping.Add(boardMapping);
                        _context.SaveChanges();
                        InsertedBoardIds.Add(boardMapping.Id);
                    }
                    else
                    {
                        InsertedBoardIds.Add(existingBoardMappingRecord.Id);
                    }
                }
            }

            #endregion

            #region SavingStandardMapping

            if (!string.IsNullOrEmpty(mappingData.MediumIds) && !string.IsNullOrEmpty(mappingData.StandardIds))
            {
                int mediumIndex = 0;
                int standardIndex = 0;
                var stringMedium = mappingData.MediumIds.Split("#");
                var stringStandard = mappingData.StandardIds.Split("$");

                foreach (var mediumId in stringMedium)
                {
                    var stringActualMedium = mediumId.Split(",");
                    foreach (var mdmId in stringActualMedium)
                    {
                        var stringActualStandard = stringStandard[mediumIndex].Split(",");
                        foreach (var stdId in stringActualStandard)
                        {
                            //delete MediumStandardId
                            //var existingMediumStandardIds = _context.StandardMediumBoardMapping.Where(x => x.MediumId == int.Parse(mdmId) && x.BoardMappingId == InsertedBoardIds[standardIndex]).ToList();
                            var existingMediumStandardIds = _context.StandardMediumBoardMapping.Where(x => x.MediumId == int.Parse(mdmId) && x.BoardMappingId == InsertedBoardIds[standardIndex]).ToList();
                            foreach (var existingMediumStandardId in existingMediumStandardIds)
                            {
                                if (!stringActualStandard.Contains(existingMediumStandardId.StandardId.ToString()))
                                {
                                    _context.StandardMediumBoardMapping.Remove(existingMediumStandardId);
                                    _context.SaveChanges();
                                }
                            }

                            // Insert the records for Standard Medium Mapping
                            if (!_context.StandardMediumBoardMapping.Any(x => x.BoardMappingId == InsertedBoardIds[standardIndex] && x.MediumId == int.Parse(mdmId) && x.StandardId == int.Parse(stdId)))
                            {
                                StandardMediumBoardMapping standardMediumBoardMapping = new StandardMediumBoardMapping();
                                standardMediumBoardMapping.BoardMappingId = InsertedBoardIds[standardIndex];
                                standardMediumBoardMapping.MediumId = int.Parse(mdmId);
                                standardMediumBoardMapping.StandardId = int.Parse(stdId);
                                standardMediumBoardMapping.Active = true;
                                _context.StandardMediumBoardMapping.Add(standardMediumBoardMapping);
                                _context.SaveChanges();
                            }
                        }
                        mediumIndex = mediumIndex + 1;
                    }
                    standardIndex = standardIndex + 1;
                }
            }

            #endregion
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
        public (int classId, string UniqueNo) SaveClasses(Classes classesData, List<IFormFile> files)
        {
            Classes classes = new Classes();
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

        #endregion

        #region Teacher

        /// <summary>
        /// Save the Teacher Record
        /// </summary>
        public (int teacherId, string UniqueNo) SaveTeacher(Teacher teacherData, List<IFormFile> files)
        {
            Teacher teachers = new Teacher();
            teachers.FirstName = teacherData.FirstName;
            teachers.LastName = teacherData.LastName;
            teachers.Email = teacherData.Email;
            teachers.ContactNo = teacherData.ContactNo;
            teachers.AlternateContact = teacherData.AlternateContact;
            teachers.Gender = teacherData.Gender;
            if (files?.Count > 0)
            {
                teachers.ProfilePictureUrl = _fileService.SaveFile(files, ClassBookConstant.ImagePath_Teacher);
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
