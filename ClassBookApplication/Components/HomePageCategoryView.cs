using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageCategoryView")]
    public class HomePageCategoryView : ViewComponent
    {
        public HomePageCategoryView()
        {

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}