using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }
    public int CurrentPage { get; set; }

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(int page = 1)
    {
        if (page < 1) page = 1;
        
        CurrentPage = page;
        Cheeps = _service.GetCheeps(page);
        return Page();
    }
}
