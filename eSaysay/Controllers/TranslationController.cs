using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class TranslationController : Controller
{
    private readonly TranslationService _translationService = new TranslationService();

    [HttpGet]
    public async Task<IActionResult> Translate(string text, string fromLang, string toLang)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Text cannot be empty.");

        string translatedText = await _translationService.TranslateText(text, fromLang, toLang);
        return Json(new { translatedText });
    }
}
