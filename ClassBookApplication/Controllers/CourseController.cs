using ClassBookApplication.DataContext;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using JW;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Controllers
{
    public class CourseController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly ClassBookService _classBookService;
        private readonly IViewRenderService _viewRenderService;

        #endregion

        #region Ctor

        public CourseController(ClassBookModelFactory classBookModelFactory,
            ClassBookService classBookService,
            IViewRenderService viewRenderService)
        {
            _classBookModelFactory = classBookModelFactory;
            _classBookService = classBookService;
            _viewRenderService = viewRenderService;
        }

        #endregion

        #region Method

        public IActionResult AllCourseList()
        {
            int count = 0;
            CoursesListModel model = new CoursesListModel();
            model.States = _classBookModelFactory.PrepareStateDropDown();
            model.CourseCategory = _classBookModelFactory.PrepareCourseCategoryDropDown();
            model.TeacherList = _classBookModelFactory.PrepareTeacherDropDown();
            model.ClassesList = _classBookModelFactory.PrepareClassesDropDown();
            FilterParameter filterParameterModel = new FilterParameter();
            model.CoursesModel = _classBookService.AllCoursesListModel(filterParameterModel, out count, 1);
            model.Pager = new Pager(count, 1);
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AllCourseList(FilterParameter model)
        {
            int count;
            CoursesListModel coursesListModel = new CoursesListModel();
            coursesListModel.CoursesModel = _classBookService.AllCoursesListModel(model, out count, model.PageIndex);
            coursesListModel.Pager = new Pager(count, model.PageIndex);
            var result = await _viewRenderService.RenderToStringAsync("_CoursesListPartialView", coursesListModel);
            return Content(result);
        }

        #endregion
    }
}
