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

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public HomePageCategoryView(ClassBookManagementContext context, ClassBookService classBookService)
        {
            this._context = context;
            this._classBookService = classBookService;
        }

        #endregion

        #region Method
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courseCategory = await _classBookService.GetItemsAsync();
            return View(courseCategory);
        }
        #endregion
    }
}