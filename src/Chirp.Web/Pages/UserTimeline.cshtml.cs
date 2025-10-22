using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage { get; set; }
    public string Author { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author, [FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        
        Author = author;
        CurrentPage = page;
        Cheeps = _service.GetCheepsFromAuthor(author, page);
        return Page();
    }
}
