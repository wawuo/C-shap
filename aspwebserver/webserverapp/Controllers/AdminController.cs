using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class AdminController : Controller
{
    private readonly IConfiguration _config;

    public AdminController(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult Index()
    {
        ViewBag.Port = _config["Port"];
        return View();
    }

    [HttpPost]
    public IActionResult UpdatePort(string port)
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var configJson = File.ReadAllText(appSettingsPath);

        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(configJson);

        jsonObj["Port"] = port;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

        File.WriteAllText(appSettingsPath, output);

        ViewBag.Message = "Port updated successfully!";
        ViewBag.Port = port;
        return View("Index");
    }
}