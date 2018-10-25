An easy to use library providing some advanced api to use http client for your mobile projects.

## Table of Contents

- [Getting Started](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#getting-started)
  - [Components](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#Components)
    - [Executor](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#executor)
    - [Mapper](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#mapper)
    - [Uri Builder](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#UriBuilder)
    - [Json Converter](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#JsonConverter)
    - [Http Client](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#HttpClient)
- [License](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#license)
- [Credits](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient#credits)

## Getting Started

If you want to work with library you can add it to your project as a nuget package.

```
Install-Package Softeq.XToolkit.RestHttpClient
```

#### Components

Library supports a bunch of Components out of the box:

- `Executor`: is special wrapper wich can be used to wrap async action and execute it multiply times if it fails.
- 'Mapper' : is helper which register your own mapping and then can be used to map one object to another. Also support Collection mapping.
- 'UriBuilder' : is helper which can be used to build your api url's
- 'Json Converter' : basic json converter. This is wrpapper on NewtonSoft Json Converter.
- 'HttpClient' : is base implementation of http client which every mobile project has. We trying to incapsulate base logic to one http client to cover most popular cases of usings.

### Executor
You can register class as singlton and then use it as following example:

Usage:

```csharp
var _executor = new Executor();
var allowAttempts = 10;
 await _executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                  //make your http request call, if it fail executor handle it
                }, allowAttempts);
```

Also support next methods:

`Task ExecuteSilentlyAsync(Func<Task> asyncAction, Action<Exception> exceptionCallback = null)`

`void InBackgroundThread(Func<Task> asyncAction);`

`void InBackgroundThread(Action action);`

`Task ExecuteActionAsync(OncePerIntervalAction oncePerIntervalAction);`

#### Mapper
Helps to map data models to application models.

Usage:

```csharp

//Data transfer model
public class SettingsData
{
    public string MyCustomToken{get;set;}
}

//our application model
public class SettingsModel
{
    public string AccessToken{get;set;}
}

//Method that map data transfer model to application model
public SettingsModel Map(SettingsData data)
{
  return data==null?null:
  new SettingsModel()
  {
    AccessToken=data.MyCustomToken
  }
}

//create and register mapping
var mapper = new Mapper();
mapper.RegisterMapping<SettingsData, SettingsModel>(Map)

//create dto(or recieve it from http request)
var data = new SettingsData{MyCustomToken = "test"}

//get application model
var settingsModel = mapper.Map<SettingsModel>(data);
```
### Uri Builder

This class helped to build up and manage your api uri

```csharp
  [DataContract]
	public class ItemQueryParams
	{
		[DataMember(Name = "itemId")]
		public string ItemId { get; set; }
	}
  
		public Uri SaveItem(ItemQueryParams queryParams)
		{
			return _uriMaker.BuildUp(queryParams, baseUrl,"/item" );
		}

    public Uri RemoveItem(string itemId)
		{
			return _uriMaker.Combine(baseUrl,"/item",itemId );
		}
    
    baseUrl = https://example.com;
    
    //https://example.com/item/myItemId
    var removeItemUri = RemoveItem("myItemId");
    
    //https://example.com/item?itemId=myItemId
    var removeItemUri = SaveItem(new ItemQueryParams{ItemId="myItemId"});
```

### Json Converter

Current implementation acceptable for most mobile clients. Also used to serialize data. You can register it as singlton and then reuse in application.

```csharp
public static class JsonConverter
    {
        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static bool TryDeserialize<T>(string jsonString, out T result)
        {
            try
            {
                result = Deserialize<T>(jsonString);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        public static string Serialize(object obj, bool shouldIgnoreNullValue = false)
        {
            return JsonConvert.SerializeObject(
                obj,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = shouldIgnoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include
                });
        }
    }
```

### Http Client

Http client used to make http requests. Http client has few configuration properties.

To create HttpClient you need to provide

```csharp
    public class HttpServiceGateConfig
    {
        public int MaxHttpLoadThreads { get; set; } = 5;
        public int HighPriorityHttpLoadThreads { get; set; } = 2;
        public int HttpRequestPerPriorityLimit { get; set; } = 350;
        public int DeadRequestTimeoutInMilliseconds { get; set; } = 10000; // use it to define when request should be canceled, default value is 10 seconds
        public WebProxy Proxy { get; set; } = null; //use it to configure proxy to handle requests(via charles on OS X as example)
    }
```

To create http client use next line

```csharp
var httpConfig = new HttpServiceGateConfig
            {
                Proxy = new WebProxy("10.55.1.191", 8888),
                DeadRequestTimeoutInMilliseconds = (int)TimeSpan.FromSeconds(100).TotalMilliseconds
            };
var httpServiceGate = new HttpServiceGate(httpConfig);

```

Then you are ready to make http requests.

```csharp
var request = new HttpRequest()
              .SetUri(new Uri("example.com"))
              .SetMethod(HttpMethods.Post)
              .WithData(JsonConverter.Serialize(new { Id = "MyId" }));

var response = await httpServiceGate.ExecuteApiCallAsync(HttpRequestPriority.Normal, request);

if (response.IsSuccessful)
{
    //parse data
}
```

As you can see, we can configure http request. First of all you can specify Uri, then you should set request type(Get, Post, Put and etc), also you can send data with requests, specify content type and headers.

Full example with http request:

```csharp
 var httpConfig = new HttpServiceGateConfig
            {
                Proxy = new WebProxy("10.55.1.191", 8888),
                DeadRequestTimeoutInMilliseconds = (int)TimeSpan.FromSeconds(100).TotalMilliseconds
            };
var httpServiceGate = new HttpServiceGate(httpConfig);

var executionResult = new ExecutionResult<string>();

            await _executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("example.com"))
                    .SetMethod(HttpMethods.Post)
                    .WithData(JsonConverter.Serialize(new { Id = "MyId" }));

                    var response = await executionResult.ExecuteApiCallAsync(HttpRequestPriority.High, request);

                    if (response.IsSuccessful)
                    {
                        executionResult.Report(response.Content, ExecutionStatus.Completed);
                    }
                });

            if (executionResult.Status == ExecutionStatus.NotCompleted)
            {
                executionResult.Report(null, ExecutionStatus.Failed);
            }

            return executionResult;
```

## License
Library is made available under the [MIT License](http://www.opensource.org/licenses/mit-license.php).

## Credits
We're open to suggestions, feel free to open an issue. Pull requests are also welcome!

