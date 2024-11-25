using System.Text.Json;
using System.Diagnostics;
using Tesseract;
using Tweetinvi.Core.Extensions;
using Tweetinvi;
using Tweetinvi.Core.Web;
using TweetImage2Text;
using System.Configuration;

const string CHECK_TEXT_IN_IMAGE = "COMUNICADO OFICIAL";

string folderPath = ConfigurationManager.AppSettings["DownloadFolder"] ?? string.Empty;
int waitTimer;

if (int.TryParse(ConfigurationManager.AppSettings["WaitTimer"] ?? string.Empty, out waitTimer) is false)
{
    waitTimer = 900;
}

string consumerKey = ConfigurationManager.AppSettings["ConsumerKey"] ?? string.Empty;
string consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"] ?? string.Empty;
string accessToken = ConfigurationManager.AppSettings["AccessToken"] ?? string.Empty;
string accessSecret = ConfigurationManager.AppSettings["AccessSecret"] ?? string.Empty;
string bearerToken = ConfigurationManager.AppSettings["BearerToken"] ?? string.Empty;

HttpClient clientXGet = new HttpClient();
clientXGet.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
HttpClient clientDownload = new HttpClient();

TwitterClient clientXPost = new TwitterClient(consumerKey, consumerSecret, accessToken, accessSecret);

Console.CursorVisible = false;

CancellationTokenSource cts = new CancellationTokenSource();
Task currentTask;


try
{
    if (Directory.Exists(folderPath) is false)
    {
        Directory.CreateDirectory(folderPath);
    }
}
catch (Exception ex)
{
    throw new Exception(ex.Message);
}

while(true)
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine("Tweet Image to Text");
    Console.WriteLine("1. Iniciar buscando el usuario");
    Console.WriteLine("2. Iniciar con la configuración guardada");
    Console.WriteLine("3. Configuración");
    Console.WriteLine("\n0. Salir");

    var consoleKey = Console.ReadKey(true);

    switch (consoleKey.Key)
    {
        case ConsoleKey.D1:
            {
                if(CheckAppConfig() is false)
                {
                    break;
                }

                Console.Clear();
                Console.WriteLine("Tweet Image to Text");

                clientXPost = new TwitterClient(consumerKey, consumerSecret, accessToken, accessSecret);

                while (true)
                {
                    if(Console.GetCursorPosition() != (0, 0))
                    {
                        Console.Clear();
                        Console.WriteLine("Tweet Image to Text");
                    }

                    Console.Write("Ingresar Username: ");

                    var cursorPosition = Console.GetCursorPosition();

                    Console.WriteLine("\n\n0. Volver al inicio");
                    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);

                    string username = Console.ReadLine() ?? string.Empty;
                    username = username.Replace("@", string.Empty);

                    if(string.IsNullOrEmpty(username))
                    {
                        continue;
                    }
                    else if(username.Equals("0"))
                    {
                        break;
                    }

                    currentTask = Task.Run(() => StartFromUsername(username, cts.Token));

                    await PressToCancel(currentTask);

                    break;
                }
                
                break;
            }
        case ConsoleKey.D2:
            {
                if (CheckAppConfig(true) is false)
                {
                    break;
                }

                Console.Clear();
                Console.WriteLine("Tweet Image to Text");

                clientXPost = new TwitterClient(consumerKey, consumerSecret, accessToken, accessSecret);

                currentTask = Task.Run(() => StartFromConfig(cts.Token));

                await PressToCancel(currentTask);

                break;
            }
        case ConsoleKey.D3:
            {
                Console.Clear();

                bool breakWhile = false;

                while(breakWhile is false)
                {
                    Console.WriteLine("Tweet Image to Text");
                    Console.WriteLine("1. Cambiar el nombre del directorio de almacenamiento");
                    Console.WriteLine("2. Cambiar el tiempo de espera entre consultas");
                    Console.WriteLine($"3. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserId"]) ? "Ingresar" : "Cambiar")} User ID");
                    Console.WriteLine($"4. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserLastTweet"]) ? "Ingresar" : "Cambiar")} User Last Tweet ID");
                    Console.WriteLine($"5. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConsumerKey"]) ? "Ingresar" : "Cambiar")} Consumer Key");
                    Console.WriteLine($"6. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConsumerSecret"]) ? "Ingresar" : "Cambiar")} Consumer Secret");
                    Console.WriteLine($"7. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessToken"]) ? "Ingresar" : "Cambiar")} Access Token");
                    Console.WriteLine($"8. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessSecret"]) ? "Ingresar" : "Cambiar")} Access Secret");
                    Console.WriteLine($"9. {(string.IsNullOrEmpty(ConfigurationManager.AppSettings["BearerToken"]) ? "Ingresar" : "Cambiar")} Bearer Token");
                    Console.WriteLine("\n0. Salir");

                    var key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.D1:
                            {
                                ConfigOptionMenu("DownloadFolder", "directorio", "Cambiar", "\\",
                                    true, "letras y/o números", true);

                                folderPath = ConfigurationManager.AppSettings["DownloadFolder"] ?? string.Empty;

                                break;
                            }
                        case ConsoleKey.D2:
                            {
                                ConfigOptionMenu("WaitTimer", "tiempo de espera (segundos)", "Cambiar", "",
                                    true, "números");

                                if (int.TryParse(ConfigurationManager.AppSettings["WaitTimer"] ?? string.Empty, out waitTimer) is false)
                                {
                                    waitTimer = 900;
                                }

                                break;
                            }
                        case ConsoleKey.D3:
                            {
                                ConfigOptionMenu("UserId", "User ID", string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserId"]) ? "Ingresar" : "Cambiar", "",
                                    true, "números");

                                break;
                            }
                        case ConsoleKey.D4:
                            {
                                ConfigOptionMenu("UserLastTweet", "User Last Tweet", string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserLastTweet"]) ? "Ingresar" : "Cambiar", "",
                                    true, "números");

                                break;
                            }
                        case ConsoleKey.D5:
                            {
                                ConfigOptionMenu("ConsumerKey", "Consumer Key", string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConsumerKey"]) ? "Ingresar" : "Cambiar");

                                consumerKey = ConfigurationManager.AppSettings["ConsumerKey"] ?? string.Empty;

                                break;
                            }
                        case ConsoleKey.D6:
                            {
                                ConfigOptionMenu("ConsumerSecret", "Consumer Secret", string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConsumerSecret"]) ? "Ingresar" : "Cambiar");

                                consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"] ?? string.Empty;

                                break;
                            }
                        case ConsoleKey.D7:
                            {
                                ConfigOptionMenu("AccessToken", "Access Token", string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessToken"]) ? "Ingresar" : "Cambiar");

                                accessToken = ConfigurationManager.AppSettings["AccessToken"] ?? string.Empty;

                                break;
                            }
                        case ConsoleKey.D8:
                            {
                                ConfigOptionMenu("AccessSecret", "Access Secret", string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessSecret"]) ? "Ingresar" : "Cambiar");

                                accessSecret = ConfigurationManager.AppSettings["AccessSecret"] ?? string.Empty;

                                break;
                            }
                        case ConsoleKey.D9:
                            {
                                ConfigOptionMenu("BearerToken", "Bearer Token", string.IsNullOrEmpty(ConfigurationManager.AppSettings["BearerToken"]) ? "Ingresar" : "Cambiar");

                                bearerToken = ConfigurationManager.AppSettings["BearerToken"] ?? string.Empty;

                                clientXGet.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                                break;
                            }
                        case ConsoleKey.D0:
                            {
                                breakWhile = true;

                                Console.Clear();

                                break;
                            }
                    }

                    Console.SetCursorPosition(0, 0);
                }

                break;
            }
        case ConsoleKey.D0:
            {
                Environment.Exit(0);
                break;
            }
        default:
            {
                break;
            }
    }
}

bool ConfigOptionMenu(string configKey, string text, string prefix, string suffix = "", bool hasCondition = false, 
    string conditionText = "", bool isLetterOrDigit = false)
{
    string error = string.Empty;

    while (true)
    {
        Console.Clear();
        Console.WriteLine("Tweet Image to Text");
        if(hasCondition)
        {
            Console.WriteLine($"{text} tiene que tener al menos un carácter y solo se aceptan {conditionText}.");
        }
        
        Console.Write($"{prefix} {text}: ");

        var cursorPosition = Console.GetCursorPosition();

        if (hasCondition && string.IsNullOrEmpty(error) is false)
        {
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top + 1);
            Console.Write($"Error: {error} tiene caracteres inválidos.");
        }

        Console.WriteLine("\n\n0. Volver al inicio");
        Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);

        string inputText = Console.ReadLine() ?? string.Empty;

        if (inputText.Equals("0"))
        {
            return false;
        }

        if(hasCondition)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                error = inputText;

                continue;
            }
            else
            {
                if ((isLetterOrDigit && inputText.All(char.IsLetterOrDigit) is false) || 
                    (isLetterOrDigit is false && inputText.All(char.IsDigit) is false))
                {
                    error = inputText;

                    continue;
                }
            }
        }

        Console.Clear();
        Console.WriteLine("Tweet Image to Text");
        if(string.IsNullOrEmpty(ConfigurationManager.AppSettings[configKey]))
        {
            Console.WriteLine($"Estas seguro que quieres {prefix.ToLower()} {inputText} a {text}");
        }
        else
        {
            Console.WriteLine($"Estas seguro que quieres {prefix.ToLower()} {ConfigurationManager.AppSettings[configKey]} por {inputText + suffix}");
        }
        Console.WriteLine("1. Si");
        Console.WriteLine("2. No");

        var confirmKey = Console.ReadKey(true);

        while (confirmKey.Key is not ConsoleKey.D1 && confirmKey.Key is not ConsoleKey.D2)
        {
            confirmKey = Console.ReadKey(true);
        }

        if (confirmKey.Key is ConsoleKey.D1)
        {
            UpdateConfigData(configKey, inputText + suffix);

            Console.Clear();

            return true;
        }
        else if (confirmKey.Key is ConsoleKey.D2)
        {
            continue;
        }

        break;
    }

    Console.Clear();

    return false;
}

async Task StartFromUsername(string username, CancellationToken cancellationToken)
{
    var user = await GetIdAndLatestTweet(username, cancellationToken);

    if (user is not null)
    {
        await GetTweet(user);

        if (string.IsNullOrEmpty(user.LastTweetId) is false && user.Urls.Any())
        {
            await TryPostTweetUrls(user.LastTweetId, user.Urls);
        }

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(waitTimer));

            await GetTweetSinceId(user, cancellationToken);
        }        
    }
}

async Task StartFromConfig(CancellationToken cancellationToken)
{
    var user = new User() { 
        Id = ConfigurationManager.AppSettings["UserId"], 
        LastTweetId = ConfigurationManager.AppSettings["UserLastTweet"] 
    };
    
    while (true)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await GetTweetSinceId(user, cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(waitTimer));
    }
}

//json: {"data":{"username":"USERNAME","name":"NAME","id":"USER_ID","most_recent_tweet_id":"TWEET_ID"}}
async Task<User?> GetIdAndLatestTweet(string username, CancellationToken cancellationToken)
{
    string apiUrl = $"https://api.twitter.com/2/users/by/username/{username}?user.fields=most_recent_tweet_id";
   
    var response = await clientXGet.GetAsync(apiUrl);

    if (cancellationToken.IsCancellationRequested)
    {
        return null;
    }

    if (response.IsSuccessStatusCode)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if(jsonResponse.Contains("Could not find user with username"))
        {
            Console.WriteLine("\nNo se encontró el usuario");
            Console.WriteLine("Presiona cualquier tecla para volver al inicio");

            return null;
        }

        using JsonDocument document = JsonDocument.Parse(jsonResponse);
        JsonElement root = document.RootElement;
        JsonElement dataArray = root.GetProperty("data");

        var id = dataArray.GetProperty("id").GetString() ?? null;
        var lastTweet = dataArray.GetProperty("most_recent_tweet_id").GetString() ?? null;

        UpdateConfigData("UserId", id, true);
        UpdateConfigData("UserLastTweet", lastTweet, true);

        return new User { Id = id, LastTweetId = lastTweet };
    }

    throw new Exception(
        "Error when getting user id and last tweet: " + Environment.NewLine + response.Content
    );
}

//json: {"data":{"id":"TWEET ID","attachments":{"media_keys":["MEDIA KEY"]},"edit_history_tweet_ids":["TWEET ID"],"text":"TWEET TEXT"},"includes":{"media":[{"media_key":"MEDIA KEY","type":"TYPE","url":"URL"}]}}
async Task GetTweet(User user)
{
    string apiUrl = $"https://api.twitter.com/2/tweets/{user.LastTweetId}?expansions=attachments.media_keys&media.fields=url";

    var response = await clientXGet.GetAsync(apiUrl);

    if (response.IsSuccessStatusCode)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if(jsonResponse.Contains("\"includes\":{\"media\":"))
        {
            using JsonDocument document = JsonDocument.Parse(jsonResponse);
            JsonElement root = document.RootElement;
            JsonElement includesArray = root.GetProperty("includes");
            JsonElement mediaArray = includesArray.GetProperty("media");

            user.Urls.Clear();

            foreach (JsonElement tweet in mediaArray.EnumerateArray())
            {
                string? type = tweet.GetProperty("type").GetString();

                if (string.IsNullOrEmpty(type) || type.Equals("photo") is false)
                {
                    continue;
                }

                string? url = tweet.GetProperty("url").GetString();

                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                user.Urls.Add(url);
            }
        }

        return;
    }

    throw new Exception(
        "Error when getting tweet: " + Environment.NewLine + response.Content
    );
}

//json: {"data":[{"id":"TWEET ID","attachments":{"media_keys":["MEDIA KEY"]},"text":"TWEET TEXT","edit_history_tweet_ids":["TWEET ID"]},{"id":"TWEET ID","text":"RT @USERNAME: TWEET TEXT","edit_history_tweet_ids":["TWEET ID"]}],"includes":{"media":[{"media_key":"MEDIA KEY","type":"TYPE","url":"URL"}]},"meta":{"result_count":NUMBER,"newest_id":"TWEET ID","oldest_id":"TWEET ID"}}
async Task GetTweetSinceId(User user, CancellationToken cancellationToken)
{
    if (user is null)
    {
        return;
    }

    string apiUrl = $"https://api.twitter.com/2/users/{user.Id}/tweets?since_id={user.LastTweetId}&expansions=attachments.media_keys&media.fields=url";

    var response = await clientXGet.GetAsync(apiUrl);

    if (cancellationToken.IsCancellationRequested)
    {
        return;
    }

    if (response.IsSuccessStatusCode)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();

        using JsonDocument document = JsonDocument.Parse(jsonResponse);
        JsonElement root = document.RootElement;

        JsonElement metaArray;
        if (document.RootElement.TryGetProperty("meta", out metaArray))
        {
            JsonElement resultElement;

            if(metaArray.TryGetProperty("result_count", out resultElement))
            {
                if(resultElement.GetInt32() == 0)
                {
                    return;
                }
            }
        }

        JsonElement dataArray;

        if (root.TryGetProperty("data", out dataArray))
        {
            JsonElement includesArray;
            JsonElement mediaArray;

            if (root.TryGetProperty("includes", out includesArray) &&
            includesArray.TryGetProperty("media", out mediaArray))
            {
                int totalCount = dataArray.EnumerateArray().Count();
                int currentIndex = 0;

                foreach (JsonElement tweetData in dataArray.EnumerateArray().Reverse())
                {
                    currentIndex++;

                    JsonElement tweetText;
                    string text = string.Empty;

                    if (tweetData.TryGetProperty("text", out tweetText))
                    {
                        text = tweetText.GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(text))
                        {
                            throw new Exception
                                (
                                $@"tweetText return null or empty from:
                                            tweetData: {tweetData.GetString()}
                                            tweetText: {tweetText.GetString()}"
                                );
                        }
                        else if (text.Contains("RT @"))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        throw new Exception
                            (
                            $@"text property not found in tweetData:
                                        tweetData: {tweetData.GetString()}"
                            );
                    }

                    JsonElement tweetId;
                    string id = string.Empty;

                    if (tweetData.TryGetProperty("id", out tweetId))
                    {
                        id = tweetId.GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(id))
                        {
                            throw new Exception
                                (
                                $@"tweetId return null or empty from:
                                            tweetData: {tweetData.GetString()}
                                            tweetId: {tweetId.GetString()}"
                                );
                        }
                    }
                    else
                    {
                        throw new Exception
                            (
                            $@"id property not found in tweetData:
                                        tweetData: {tweetData.GetString()}"
                            );
                    }

                    if (currentIndex == totalCount)
                    {
                        user.LastTweetId = id;
                        UpdateConfigData("UserLastTweet", id, true);
                    }

                    JsonElement attachmentsArray;

                    if (tweetData.TryGetProperty("attachments", out attachmentsArray))
                    {
                        JsonElement media_keysArray;

                        if (attachmentsArray.TryGetProperty("media_keys", out media_keysArray))
                        {
                            List<string> urls = new List<string>();

                            foreach (JsonElement media_key in media_keysArray.EnumerateArray())
                            {
                                foreach (JsonElement media in mediaArray.EnumerateArray())
                                {
                                    string key = media_key.GetString() ?? string.Empty;

                                    if (key.Equals(media.GetProperty("media_key").GetString()))
                                    {
                                        string type = media.GetProperty("type").GetString() ?? "";

                                        if (type.Equals("photo"))
                                        {
                                            urls.Add(media.GetProperty("url").GetString() ?? "");
                                        }
                                    }
                                }
                            }

                            await TryPostTweetUrls(id, urls);
                        }
                    }
                }
            }
            else
            {
                foreach (JsonElement tweetData in dataArray.EnumerateArray())
                {
                    JsonElement tweetId;

                    if (tweetData.TryGetProperty("id", out tweetId))
                    {
                        string id = tweetId.GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(id))
                        {
                            throw new Exception
                                (
                                $@"tweetId return null or empty from:
                                            tweetData: {tweetData.GetString()}
                                            tweetId: {tweetId.GetString()}"
                                );
                        }

                        user.LastTweetId = id;
                        UpdateConfigData("UserLastTweet", id, true);

                        break;
                    }
                }
            }
        }

        return;
    }

    throw new Exception(
            "Error getting tweet since: " + Environment.NewLine + response.Content
        );
}

async Task TryPostTweetUrls(string tweetId, List<string> urls)
{
    string text = string.Empty;

    foreach (string url in urls)
    {
        Console.WriteLine("Se encontró un tweet con imagen");
        Console.WriteLine($"Tweet ID: {tweetId}");

        var textOutput = await ProcessUrl(url);

        if (text.Contains(CHECK_TEXT_IN_IMAGE) || textOutput.Contains(CHECK_TEXT_IN_IMAGE))
        {
            text += textOutput + Environment.NewLine;
        }
    }

    if (text.IsEmpty() is false)
    {
        await PostTweetImageText(tweetId, text);
    }
}

async Task PostTweetImageText(string tweetId, string tweetText)
{
    var poster = new TweetsV2Poster(clientXPost);

    ITwitterResult result = await poster.PostTweet(
        new TweetV2PostRequest
        {
            Text = tweetText,
            QuoteTweetId = tweetId
        }
    );

    if (result.Response.IsSuccessStatusCode == false)
    {
        throw new Exception(
            "Error when posting tweet: " + Environment.NewLine + result.Content
        );
    }
}

async Task<string> ProcessUrl(string url)
{
    if (string.IsNullOrEmpty(url) is false)
    {
        Console.WriteLine($"Extracted URL: {url}");

        string[] parts = url.Split('/');
        string fileName = parts[parts.Length - 1];

        if(fileName.Contains(".jpg") is false)
        {
            return string.Empty;
        }

        await DownloadImage(url, fileName);

        return UseOcrOnFile(fileName);
    }

    return string.Empty;
}

string UseOcrOnFile(string file)
{
    try
    {
        using (var engine = new TesseractEngine(@"./tessdata", "spa", EngineMode.Default))
        {
            using (var img = Pix.LoadFromFile(folderPath + file))
            {
                using (var page = engine.Process(img))
                {
                    return page.GetText();
                }
            }
        }
    }
    catch (Exception e)
    {
        Trace.TraceError(e.ToString());
        Console.WriteLine("Unexpected Error: " + e.Message);
        Console.WriteLine("Details: ");
        Console.WriteLine(e.ToString());
    }

    return string.Empty;
}

async Task DownloadImage(string url, string fileName)
{
    if (File.Exists(folderPath + fileName))
    {
        return;
    }

    try
    {
        var response = await clientDownload.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var fileStream = new FileStream(folderPath + fileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        await stream.CopyToAsync(fileStream);
        fileStream.Dispose();
    }
    catch (Exception e)
    {
        Trace.TraceError(e.ToString());
        Console.WriteLine("Unexpected Error: " + e.Message);
        Console.WriteLine("Details: ");
        Console.WriteLine(e.ToString());
    }
}

void UpdateConfigData(string key, string? value, bool checkForNullOrEmpty = false)
{
    if (checkForNullOrEmpty && string.IsNullOrEmpty(value))
    {
        throw new Exception(
        $"Trying to save a null or empty {key}"
        );
    }

    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    config.AppSettings.Settings[key].Value = value;
    config.Save(ConfigurationSaveMode.Modified);
    ConfigurationManager.RefreshSection("appSettings");
}

async Task PressToCancel(Task task)
{
    Console.Clear();
    Console.WriteLine("Tweet Image to Text");
    Console.WriteLine("Programa en ejecución...");
    Console.WriteLine("\n0. Volver al inicio");

    while (Console.ReadKey(true).Key != ConsoleKey.D0) 
    {
        if(currentTask.IsCompleted)
        {
            break;
        }
    }

    cts.Cancel();
    try
    {
        await Task.WhenAny(currentTask, Task.Delay(1000));
    }
    finally
    {
        cts = new CancellationTokenSource();
    }

    Console.Clear();
}

bool CheckAppConfig(bool checkUser = false)
{
    if (string.IsNullOrEmpty(consumerKey))
    {
        if(UpdateAppConfig("ConsumerKey", "Consumer Key") is false)
        {
            return false;
        }

        consumerKey = ConfigurationManager.AppSettings["ConsumerKey"] ?? string.Empty;
    }
    if (string.IsNullOrEmpty(consumerSecret))
    {
        if (UpdateAppConfig("ConsumerSecret", "Consumer Secret") is false)
        {
            return false;
        }

        consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"] ?? string.Empty;
    }
    if (string.IsNullOrEmpty(accessToken))
    {
        if (UpdateAppConfig("AccessToken", "Access Token") is false)
        {
            return false;
        }

        accessToken = ConfigurationManager.AppSettings["AccessToken"] ?? string.Empty;
    }
    if (string.IsNullOrEmpty(accessSecret))
    {
        if (UpdateAppConfig("AccessSecret", "Access Secret") is false)
        {
            return false;
        }

        accessSecret = ConfigurationManager.AppSettings["AccessSecret"] ?? string.Empty;
    }

    if (string.IsNullOrEmpty(bearerToken))
    {
        if (UpdateAppConfig("BearerToken", "Bearer Token") is false)
        {
            return false;
        }

        bearerToken = ConfigurationManager.AppSettings["BearerToken"] ?? string.Empty;

        clientXGet.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
    }

    if (checkUser)
    {
        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserId"]))
        {
            if (UpdateAppConfig("UserId", "User Id", true, "números") is false)
            {
                return false;
            }
        }
        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["UserLastTweet"]))
        {
            if (UpdateAppConfig("UserLastTweet", "User Last Tweet", true, "números") is false)
            {
                return false;
            }
        }
    }

    return true;
}

bool UpdateAppConfig(string configKey, string text, bool hasCondition = false,
    string conditionText = "", bool isLetterOrDigit = false)
{
    Console.Clear();
    Console.WriteLine("Tweet Image to Text");
    Console.WriteLine($"No se encontró {text}");
    Console.WriteLine($"Ingresar {text}\n");

    Console.WriteLine("1. Si");
    Console.WriteLine("2. No");

    var confirmKey = Console.ReadKey(true);

    while (confirmKey.Key is not ConsoleKey.D1 && confirmKey.Key is not ConsoleKey.D2)
    {
        confirmKey = Console.ReadKey(true);
    }

    if(confirmKey.Key is ConsoleKey.D1)
    {
        return ConfigOptionMenu(configKey, text, "Ingresar", "", hasCondition, conditionText, isLetterOrDigit);
    }

    Console.Clear();

    return false;
}

class User
{
    public string? Id { get; set; }
    public string? LastTweetId { get; set; }
    public List<string> Urls { get; set; } = new List<string>();
}