using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Atlantis_Portfolio_Console_Client;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "Atlantis Portfolio";
        Console.OutputEncoding = Encoding.UTF8;

        using var app = new App();

        await app.RunAsync();
    }
}

// ============================================================
// APP
// ============================================================

internal sealed class App : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;
    private readonly ApiClient _apiClient;

    private string _baseUrl = "https://localhost:7072";
    public App()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _tokenManager = new TokenManager();

        _apiClient = new ApiClient(
            _httpClient,
            _tokenManager,
            _baseUrl);
    }

    public async Task RunAsync()
    {
        while (true)
        {
            UI.Header("ATLANTIS PORTFOLIO");

            Console.WriteLine("Welcome to Atlantis Portfolio!");
            Console.WriteLine();
            Console.WriteLine("1. My Portfolio");
            Console.WriteLine("2. Projects");
            Console.WriteLine("3. Account");
            Console.WriteLine("4. Developer Tools");
            Console.WriteLine("0. Exit");

            var choice = UI.ReadChoice();

            switch (choice)
            {
                case "1":
                    await ProfileMenuAsync();
                    break;

                case "2":
                    await ProjectsMenuAsync();
                    break;

                case "3":
                    await AccountMenuAsync();
                    break;

                case "4":
                    await DeveloperToolsMenuAsync();
                    break;

                case "0":
                    UI.Success("Thank you for visiting Atlantis Portfolio!");
                    return;

                default:
                    UI.Error("Please choose a valid option.");
                    UI.Pause();
                    break;
            }
        }
    }

    // --------------------------------------------------------
    // PROFILE
    // --------------------------------------------------------

    private async Task ProfileMenuAsync()
    {
        while (true)
        {
            UI.Header("MY PORTFOLIO");

            Console.WriteLine("1. View My Portfolio Profile");
            Console.WriteLine("2. Find Profile by ID");
            Console.WriteLine("0. Back");

            switch (UI.ReadChoice())
            {
                case "1":
                    await SendAndDisplayAsync(
                        HttpMethod.Get,
                        "api/PortfolioProfile/Get/Profile");
                    break;

                case "2":
                    {
                        var id = UI.ReadPositiveInt("Profile ID");

                        if (id.HasValue)
                        {
                            await SendAndDisplayAsync(
                                HttpMethod.Get,
                                $"api/PortfolioProfile/Get/{id.Value}");
                        }

                        break;
                    }

                case "0":
                    return;

                default:
                    UI.Error("Please choose a valid option.");
                    UI.Pause();
                    break;
            }
        }
    }

    // --------------------------------------------------------
    // PROJECTS
    // --------------------------------------------------------

    private async Task ProjectsMenuAsync()
    {
        while (true)
        {
            UI.Header("PROJECTS");

            Console.WriteLine("1. Browse All Projects");
            Console.WriteLine("2. View Project Details");
            Console.WriteLine("3. Add a Project");
            Console.WriteLine("4. Edit a Project");
            Console.WriteLine("5. Delete a Project");
            Console.WriteLine("0. Back");

            switch (UI.ReadChoice())
            {
                case "1":
                    await SendAndDisplayAsync(
                        HttpMethod.Get,
                        "api/Project/Get/All");
                    break;

                case "2":
                    {
                        var id = UI.ReadPositiveInt("Project ID");

                        if (id.HasValue)
                        {
                            await SendAndDisplayAsync(
                                HttpMethod.Get,
                                $"api/Project/Get/{id.Value}");
                        }

                        break;
                    }

                case "3":
                    await AddProjectAsync();
                    break;

                case "4":
                    await UpdateProjectAsync();
                    break;

                case "5":
                    await DeleteProjectAsync();
                    break;

                case "0":
                    return;

                default:
                    UI.Error("Please choose a valid option.");
                    UI.Pause();
                    break;
            }
        }
    }

    private async Task AddProjectAsync()
    {
        if (!RequireLogin())
            return;

        UI.Header("ADD A PROJECT");

        var project = ReadProjectForm();

        if (project is null)
            return;

        if (!UI.Confirm("Add this project?"))
            return;

        await SendAndDisplayAsync(
            HttpMethod.Post,
            "api/Project/Add",
            project);
    }

    private async Task UpdateProjectAsync()
    {
        if (!RequireLogin())
            return;

        UI.Header("EDIT A PROJECT");

        var id = UI.ReadPositiveInt("Project ID");

        if (!id.HasValue)
            return;

        Console.WriteLine();
        Console.WriteLine(
            "Enter the new values for the project.");

        Console.WriteLine(
            "Optional URLs can be left empty.");

        var project = ReadProjectForm();

        if (project is null)
            return;

        if (!UI.Confirm($"Update project #{id.Value}?"))
            return;

        await SendAndDisplayAsync(
            HttpMethod.Put,
            $"api/Project/Update/{id.Value}",
            project);
    }

    private async Task DeleteProjectAsync()
    {
        if (!RequireLogin())
            return;

        UI.Header("DELETE A PROJECT");

        var id = UI.ReadPositiveInt("Project ID");

        if (!id.HasValue)
            return;

        UI.Warning(
            "This action may permanently remove the project.");

        if (!UI.Confirm(
                $"Are you sure you want to delete project #{id.Value}?"))
        {
            UI.Info("Deletion cancelled.");
            UI.Pause();
            return;
        }

        await SendAndDisplayAsync(
            HttpMethod.Delete,
            $"api/Project/Delete/{id.Value}");
    }

    private static object? ReadProjectForm()
    {
        Console.WriteLine();
        Console.WriteLine("Enter Project Information");
        Console.WriteLine("-------------------------");

        var title = UI.ReadRequired("Title");

        if (title is null)
            return null;

        var description = UI.ReadRequired("Description");

        if (description is null)
            return null;

        var imageUrl = UI.ReadOptional("Image URL");
        var gitHubUrl = UI.ReadOptional("GitHub URL");
        var liveUrl = UI.ReadOptional("Live Demo URL");

        return new
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            GitHubUrl = gitHubUrl,
            LiveUrl = liveUrl
        };
    }

    // --------------------------------------------------------
    // ACCOUNT
    // --------------------------------------------------------

    private async Task AccountMenuAsync()
    {
        while (true)
        {
            UI.Header("ACCOUNT");

            Console.WriteLine(
                $"Status: {(_tokenManager.IsLoggedIn ? "Signed In" : "Guest")}");

            Console.WriteLine();

            if (_tokenManager.IsLoggedIn)
            {
                Console.WriteLine("1. Sign Out");
            }
            else
            {
                Console.WriteLine("1. Sign In");
            }

            Console.WriteLine("0. Back");

            switch (UI.ReadChoice())
            {
                case "1":
                    if (_tokenManager.IsLoggedIn)
                        await LogoutAsync();
                    else
                        await LoginAsync();

                    break;

                case "0":
                    return;

                default:
                    UI.Error("Please choose a valid option.");
                    UI.Pause();
                    break;
            }
        }
    }

    private async Task LoginAsync()
    {
        UI.Header("SIGN IN");

        var email = UI.ReadRequired("Email");

        if (email is null)
            return;

        var password = UI.ReadPassword();

        if (password is null)
            return;

        var result = await _apiClient.SendAsync(
            HttpMethod.Post,
            "api/Auth/Login",
            new
            {
                Email = email,
                Password = password
            });

        DisplayResult(result);

        if (result.IsSuccess)
        {
            _tokenManager.CaptureTokens(result.Content);

            if (_tokenManager.IsLoggedIn)
            {
                UI.Success("You are now signed in.");
            }
            else
            {
                UI.Warning(
                    "The server returned a successful response, " +
                    "but no access token was found.");
            }
        }

        UI.Pause();
    }

    private async Task LogoutAsync()
    {
        UI.Header("SIGN OUT");

        if (!UI.Confirm("Are you sure you want to sign out?"))
        {
            UI.Info("Sign out cancelled.");
            UI.Pause();
            return;
        }

        var refreshToken = _tokenManager.RefreshToken;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var result = await _apiClient.SendAsync(
                HttpMethod.Post,
                "api/Auth/Logout",
                new
                {
                    RefreshToken = refreshToken
                });

            DisplayResult(result);
        }

        // Clear local credentials even if the server is unavailable.
        _tokenManager.Clear();

        UI.Info("Local session cleared.");
        UI.Pause();
    }

    // --------------------------------------------------------
    // DEVELOPER TOOLS
    // --------------------------------------------------------

    private async Task DeveloperToolsMenuAsync()
    {
        while (true)
        {
            UI.Header("DEVELOPER TOOLS");

            Console.WriteLine("1. Send Custom API Request");
            Console.WriteLine("2. Change API Base URL");
            Console.WriteLine("3. Clear Local Tokens");
            Console.WriteLine("0. Back");

            switch (UI.ReadChoice())
            {
                case "1":
                    await CustomRequestAsync();
                    break;

                case "2":
                    ChangeBaseUrl();
                    break;

                case "3":
                    if (UI.Confirm("Clear locally stored tokens?"))
                    {
                        _tokenManager.Clear();
                        UI.Success("Local tokens cleared.");
                    }

                    UI.Pause();
                    break;

                case "0":
                    return;

                default:
                    UI.Error("Please choose a valid option.");
                    UI.Pause();
                    break;
            }
        }
    }

    private async Task CustomRequestAsync()
    {
        UI.Header("CUSTOM API REQUEST");

        Console.WriteLine("HTTP Method");
        Console.WriteLine("1. GET");
        Console.WriteLine("2. POST");
        Console.WriteLine("3. PUT");
        Console.WriteLine("4. DELETE");

        var method = UI.ReadChoice();

        HttpMethod httpMethod;

        switch (method)
        {
            case "1":
                httpMethod = HttpMethod.Get;
                break;

            case "2":
                httpMethod = HttpMethod.Post;
                break;

            case "3":
                httpMethod = HttpMethod.Put;
                break;

            case "4":
                httpMethod = HttpMethod.Delete;
                break;

            default:
                UI.Error("Invalid HTTP method.");
                UI.Pause();
                return;
        }

        Console.WriteLine();
        Console.WriteLine(
            "Enter the endpoint relative to the API base URL.");

        Console.WriteLine(
            "Example: api/Project/Get/All");

        var endpoint = UI.ReadRequired("Endpoint");

        if (endpoint is null)
            return;

        object? body = null;

        if (httpMethod == HttpMethod.Post ||
            httpMethod == HttpMethod.Put)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Enter a JSON request body, or leave empty for no body.");

            var json = UI.ReadOptional("JSON body");

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    body = JsonNode.Parse(json);
                }
                catch (JsonException)
                {
                    UI.Error("Invalid JSON.");
                    UI.Pause();
                    return;
                }
            }
        }

        if (httpMethod == HttpMethod.Delete &&
            !UI.Confirm("Send this DELETE request?"))
        {
            UI.Info("Request cancelled.");
            UI.Pause();
            return;
        }

        await SendAndDisplayAsync(
            httpMethod,
            endpoint,
            body);
    }

    private void ChangeBaseUrl()
    {
        UI.Header("API CONFIGURATION");

        Console.WriteLine($"Current URL: {_baseUrl}");

        var input = UI.ReadRequired("New API Base URL");

        if (input is null)
            return;

        if (!Uri.TryCreate(input, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != Uri.UriSchemeHttp))
        {
            UI.Error("Please enter a valid HTTP or HTTPS URL.");
            UI.Pause();
            return;
        }

        _baseUrl = uri.ToString().TrimEnd('/');

        _apiClient.BaseUrl = _baseUrl;

        UI.Success("API Base URL updated.");
        UI.Pause();
    }

    // --------------------------------------------------------
    // SHARED HELPERS
    // --------------------------------------------------------

    private bool RequireLogin()
    {
        if (_tokenManager.IsLoggedIn)
            return true;

        UI.Warning(
            "This action requires authentication.");

        UI.Info(
            "Please sign in from the Account menu first.");

        UI.Pause();

        return false;
    }

    private async Task SendAndDisplayAsync(
        HttpMethod method,
        string endpoint,
        object? body = null)
    {
        UI.Header("REQUEST RESULT");

        var result = await _apiClient.SendAsync(
            method,
            endpoint,
            body);

        DisplayResult(result);

        UI.Pause();
    }

    private static void DisplayResult(ApiResult result)
    {
        Console.WriteLine();

        UI.Line();

        Console.WriteLine(
            $"Status: {(int)result.StatusCode} {result.StatusCode}");

        if (result.IsSuccess)
        {
            UI.Success("Request completed successfully.");
        }
        else
        {
            UI.Error("The request could not be completed.");
        }

        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(result.Content))
        {
            UI.Info("The server returned no response body.");
        }
        else
        {
            Console.WriteLine("Response");
            UI.Line();

            Console.WriteLine(
                UI.FormatJson(result.Content));
        }

        UI.Line();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

// ============================================================
// TOKEN MANAGER
// ============================================================

internal sealed class TokenManager
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(AccessToken);

    public void CaptureTokens(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            using var document = JsonDocument.Parse(json);

            var root = document.RootElement;

            AccessToken = FindString(
                root,
                "AccessToken",
                "accessToken");

            RefreshToken = FindString(
                root,
                "RefreshToken",
                "refreshToken");
        }
        catch (JsonException)
        {
            // The response was not JSON.
            Clear();
        }
    }

    private static string? FindString(
        JsonElement element,
        params string[] names)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                foreach (var name in names)
                {
                    if (string.Equals(
                            property.Name,
                            name,
                            StringComparison.OrdinalIgnoreCase) &&
                        property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }
                }

                var nested = FindString(
                    property.Value,
                    names);

                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindString(item, names);

                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }

        return null;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
    }
}

// ============================================================
// API RESULT
// ============================================================

internal sealed class ApiResult
{
    public HttpStatusCode StatusCode { get; init; }

    public string Content { get; init; } = string.Empty;

    public bool IsSuccess { get; init; }
}

// ============================================================
// API CLIENT
// ============================================================

internal sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;

    public string BaseUrl { get; set; }

    public ApiClient(
        HttpClient httpClient,
        TokenManager tokenManager,
        string baseUrl)
    {
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<ApiResult> SendAsync(
        HttpMethod method,
        string endpoint,
        object? body = null)
    {
        try
        {
            var normalizedEndpoint = endpoint.TrimStart('/');

            var url = $"{BaseUrl}/{normalizedEndpoint}";

            using var request = new HttpRequestMessage(
                method,
                url);

            if (!string.IsNullOrWhiteSpace(
                    _tokenManager.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _tokenManager.AccessToken);
            }

            if (body is not null)
            {
                var json = body is JsonNode jsonNode
                    ? jsonNode.ToJsonString()
                    : JsonSerializer.Serialize(body);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");
            }

            using var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return new ApiResult
            {
                StatusCode = response.StatusCode,
                Content = content,
                IsSuccess = response.IsSuccessStatusCode
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiResult
            {
                StatusCode = 0,
                Content =
                    "The request timed out. " +
                    "Please check whether the API is running.",
                IsSuccess = false
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResult
            {
                StatusCode = 0,
                Content =
                    "Could not connect to the API.\n" +
                    "Check that the backend is running and that " +
                    "the Base URL is correct.\n\n" +
                    $"Details: {ex.Message}",
                IsSuccess = false
            };
        }
        catch (Exception ex)
        {
            return new ApiResult
            {
                StatusCode = 0,
                Content = $"Unexpected error: {ex.Message}",
                IsSuccess = false
            };
        }
    }
}

// ============================================================
// CONSOLE UI
// ============================================================

internal static class UI
{
    public static void Header(string title)
    {
        Console.Clear();

        Console.WriteLine(
            "==================================================");

        Console.WriteLine(
            $"  {title}");

        Console.WriteLine(
            "==================================================");

        Console.WriteLine();
    }

    public static void Line()
    {
        Console.WriteLine(
            "--------------------------------------------------");
    }

    public static string ReadChoice()
    {
        Console.WriteLine();

        Console.Write("Choose an option: ");

        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string? ReadRequired(string label)
    {
        Console.Write($"{label}: ");

        var value = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            Error($"{label} is required.");
            Pause();

            return null;
        }

        return value;
    }

    public static string? ReadOptional(string label)
    {
        Console.Write($"{label} (optional): ");

        var value = Console.ReadLine()?.Trim();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    public static string? ReadPassword()
    {
        Console.Write("Password: ");

        var password = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;

                    Console.Write("\b \b");
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);

                Console.Write('*');
            }
        }

        if (password.Length == 0)
        {
            Error("Password is required.");
            Pause();

            return null;
        }

        return password.ToString();
    }

    public static int? ReadPositiveInt(string label)
    {
        Console.Write($"{label}: ");

        var input = Console.ReadLine();

        if (int.TryParse(input, out var value) && value > 0)
            return value;

        Error("Please enter a valid positive number.");
        Pause();

        return null;
    }

    public static bool Confirm(string message)
    {
        Console.WriteLine();
        Warning(message);

        Console.Write("Type Y to confirm: ");

        var input = Console.ReadLine()?.Trim();

        return string.Equals(
            input,
            "Y",
            StringComparison.OrdinalIgnoreCase);
    }

    public static void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {message}");
        Console.ResetColor();
    }

    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to continue...");

        Console.ReadKey(intercept: true);

        Console.WriteLine();
    }

    public static string FormatJson(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);

            return JsonSerializer.Serialize(
                document.RootElement,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
        catch (JsonException)
        {
            // Some error responses may be plain text.
            return content;
        }
    }
}