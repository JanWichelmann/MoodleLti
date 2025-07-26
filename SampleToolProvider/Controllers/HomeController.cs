using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodleLti;

namespace SampleToolProvider.Controllers
{
    [Authorize]
    [Route("")]
    [Route("home")]
    public class HomeController : Controller
    {
        public static string ControllerName { get; } = "Home";

        private readonly IMoodleGradebook _moodleGradebook;

        public HomeController(IMoodleGradebook moodleGradebook)
        {
            _moodleGradebook = moodleGradebook;
        }

        [HttpGet]
        public async Task<IActionResult> RenderAsync()
        {
            // Load gradebook
            ViewData["columns"] = await _moodleGradebook.GetColumnsAsync();

            return View("/Views/Home.cshtml");
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> DeleteColumnAsync(int id)
        {
            await _moodleGradebook.DeleteColumnAsync(id);

            return await RenderAsync();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateColumnAsync(string title, string tag, float maxScore)
        {
            await _moodleGradebook.CreateColumnAsync(title, maxScore, tag);

            return await RenderAsync();
        }
    }
}
