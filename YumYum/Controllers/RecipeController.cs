using Microsoft.AspNetCore.Mvc;

namespace YumYum.Controllers
{
	public class RecipeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
