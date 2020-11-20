using ClassBookApplication.DataContext;
using ClassBookApplication.Factory;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageCategoryView")]
    public class HomePageCategoryView : ViewComponent
    {
        #region Fields

        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public HomePageCategoryView(ClassBookService classBookService)
        {
            this._classBookService = classBookService;
        }

        #endregion

        #region Method
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courseCategory = await _classBookService.GetCategories();
            return View(courseCategory);
        }
        #endregion
    }
}