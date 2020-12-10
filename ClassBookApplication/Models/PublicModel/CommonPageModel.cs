using JW;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassBookApplication.Models.PublicModel
{
    public class CommonPageModel : PageModel
    {
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int MaxPages { get; set; }
        public Pager Pager { get; set; }
    }
}
