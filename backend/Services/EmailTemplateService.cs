namespace backend.Services;

public interface IEmailTemplateService
{
    string Render(string templateName, Dictionary<string, string> variables);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatePath;

    public EmailTemplateService()
    {
        _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        
        // In development, the templates might be in the project folder, not the output folder
        if (!Directory.Exists(_templatePath))
        {
            _templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        }
    }

    public string Render(string templateName, Dictionary<string, string> variables)
    {
        var filePath = Path.Combine(_templatePath, $"{templateName}.html");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Email template '{templateName}' not found at {filePath}");
        }

        var content = File.ReadAllText(filePath);

        foreach (var variable in variables)
        {
            content = content.Replace($"{{{{{variable.Key}}}}}", variable.Value);
        }

        return content;
    }
}
