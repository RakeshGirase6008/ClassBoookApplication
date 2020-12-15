using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassBookApplication.Factory
{
    public class ClassBookModelFactory
    {
        #region Fields

        private readonly ClassBookManagementContext _context;

        #endregion

        #region Ctor

        public ClassBookModelFactory(ClassBookManagementContext context)
        {
            this._context = context;
        }

        #endregion

        #region ClassBook

        public object PrepareUserDetail(Users user)
        {
            return new
            {
                UserId = user.EntityId,
                Email = user.Email,
                AuthorizeTokenKey = user.AuthorizeTokenKey
            };
        }
        public string PrepareURL(string ImageUrl)
        {
            if (!string.IsNullOrEmpty(ImageUrl))
                return ClassBookConstant.WebSite_HostURL.ToString() + "/" + ImageUrl.Replace("\\", "/");
            return string.Empty;
        }
        public List<Testimonial> PrepareTestimonial(IList<Testimonial> testimonial)
        {
            List<Testimonial> testimonials = new List<Testimonial>();
            foreach (var test in testimonial)
            {
                testimonials.Add(new Testimonial()
                {
                    ClientName = test.ClientName,
                    Position = test.Position,
                    Rating = test.Rating,
                    PhotoUrl = PrepareURL(test.PhotoUrl),
                    Descrption = test.Descrption
                });
            }
            return testimonials;

        }

        #endregion

        #region Common

        /// <summary>
        /// Prepare the DropDownlist for Gender by Enum
        /// </summary>
        public List<SelectListItem> PrepareGenderDropDown()
        {
            Array values = Enum.GetValues(typeof(Gender));
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var i in values)
            {
                items.Add(new SelectListItem
                {
                    Text = Enum.GetName(typeof(Gender), i),
                    Value = ((int)i).ToString()
                });
            }
            return items;
        }

        /// <summary>
        /// Prepare the DropDownlist for States
        /// </summary>
        public List<SelectListItem> PrepareStateDropDown()
        {
            var stateList = _context.States.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var state in stateList)
            {
                model.Add(new SelectListItem()
                {
                    Text = state.Name,
                    Value = state.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Cities
        /// </summary>
        public List<SelectListItem> PrepareCityDropDown()
        {
            var cityList = _context.City.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var city in cityList)
            {
                model.Add(new SelectListItem()
                {
                    Text = city.Name,
                    Value = city.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Cities by StateId
        /// </summary>
        public List<SelectListItem> PrepareCityDropDown(int stateId)
        {
            var cityList = _context.City.Where(x => x.Active == true && x.StateId == stateId).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var city in cityList)
            {
                model.Add(new SelectListItem()
                {
                    Text = city.Name,
                    Value = city.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Pincode
        /// </summary>
        public List<SelectListItem> PreparePincodeDropDown()
        {
            var pincodeList = _context.Pincode.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var pincode in pincodeList)
            {
                model.Add(new SelectListItem()
                {
                    Text = pincode.Name,
                    Value = pincode.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Pincode
        /// </summary>
        public List<SelectListItem> PreparePincodeDropDown(int cityId)
        {
            var pincodeList = _context.Pincode.Where(x => x.Active == true && x.CityId == cityId).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var pincode in pincodeList)
            {
                model.Add(new SelectListItem()
                {
                    Text = pincode.Name,
                    Value = pincode.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Board
        /// </summary>
        public List<SelectListItem> PrepareBoardDropDown()
        {
            var boardList = _context.Board.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var board in boardList)
            {
                model.Add(new SelectListItem()
                {
                    Text = board.Name,
                    Value = board.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Medium
        /// </summary>
        public List<SelectListItem> PrepareMediumDropDown()
        {
            var mediumList = _context.Medium.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var medium in mediumList)
            {
                model.Add(new SelectListItem()
                {
                    Text = medium.Name,
                    Value = medium.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Standard
        /// </summary>
        public List<SelectListItem> PrepareStandardDropDown()
        {
            var standardsList = _context.Standards.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var standard in standardsList)
            {
                model.Add(new SelectListItem()
                {
                    Text = standard.Name,
                    Value = standard.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Teacher
        /// </summary>
        public List<SelectListItem> PrepareTeacherDropDown()
        {
            var teacherList = _context.Teacher.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var teacher in teacherList)
            {
                model.Add(new SelectListItem()
                {
                    Text = teacher.FirstName + "" + teacher.LastName,
                    Value = teacher.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Classes
        /// </summary>
        public List<SelectListItem> PrepareClassesDropDown()
        {
            var classesList = _context.Classes.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var classes in classesList)
            {
                model.Add(new SelectListItem()
                {
                    Text = classes.Name,
                    Value = classes.Id.ToString()
                });
            }
            return model;
        }

        /// <summary>
        /// Prepare the DropDownlist for Course Category
        /// </summary>
        public List<SelectListItem> PrepareCourseCategoryDropDown()
        {
            var courseCategoryList = _context.CourseCategory.Where(x => x.Active == true).ToList();
            List<SelectListItem> model = new List<SelectListItem>();
            foreach (var courses in courseCategoryList)
            {
                model.Add(new SelectListItem()
                {
                    Text = courses.Name,
                    Value = courses.Id.ToString()
                });
            }
            return model;
        }

        #endregion
    }
}
