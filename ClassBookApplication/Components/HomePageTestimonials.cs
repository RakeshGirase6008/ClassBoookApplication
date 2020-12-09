using ClassBookApplication.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageTestimonials")]
    public class HomePageTestimonials : ViewComponent
    {
        #region Fields

        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public HomePageTestimonials(ClassBookService classBookService)
        {
            this._classBookService = classBookService;
        }

        #endregion
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var getAllClasses = await _classBookService.GetAllTestimonials11();
            return View(getAllClasses);
        }
    }
}
