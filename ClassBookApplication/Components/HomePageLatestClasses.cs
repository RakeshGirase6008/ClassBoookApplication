using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageLatestClasses")]
    public class HomePageLatestClasses : ViewComponent
    {
        public HomePageLatestClasses()
        {

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}
