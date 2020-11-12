using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageCourses")]
    public class HomePageCourses : ViewComponent
    {
        public HomePageCourses()
        {

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}
