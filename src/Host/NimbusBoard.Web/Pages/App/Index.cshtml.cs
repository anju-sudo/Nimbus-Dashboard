using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nimbus_Board.Pages.App;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/App/Dashboard");
}
