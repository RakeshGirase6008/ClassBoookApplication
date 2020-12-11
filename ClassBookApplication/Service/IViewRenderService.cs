using System.Threading.Tasks;

namespace ClassBookApplication.Service
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
