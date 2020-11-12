using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageTestimonials")]
    public class HomePageTestimonials : ViewComponent
    {
        public HomePageTestimonials()
        {

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}
