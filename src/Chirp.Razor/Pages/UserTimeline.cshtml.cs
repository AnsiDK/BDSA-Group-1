using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }
    public int CurrentPage { get; set; }
    public string Author { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author, int page = 1)
    {
        if (page < 1) page = 1;
        
        Author = author;
        CurrentPage = page;
        Cheeps = _service.GetCheepsFromAuthor(author, page);
        return Page();
    }
}
