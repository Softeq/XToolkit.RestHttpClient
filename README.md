
![alt text](https://github.com/Softeq/Softeq.XToolkit.RestHttpClient/blob/master/clientServer.png?raw=true)

[![Build status](https://dev.azure.com/SofteqDevelopment/XToolkit.RestHttpClient/_apis/build/status/Master%20build)](https://dev.azure.com/SofteqDevelopment/XToolkit.RestHttpClient/_build/latest?definitionId=56)

An easy to use library providing some advanced API to use HTTP client and some extensions for your mobile projects.

## Table of Contents

- [Getting Started](#getting-started)
- [Components](#components)
    - [Executor](#executor)
    - [Mapper](#mapper)
    - [Uri Builder](#uri-builder)
    - [Json Converter](#json-converter)
    - [Http Client](#http-client)
    - [Message Hub](#message-hub)
- [License](#license)
- [Credits](#credits)

## Getting Started

If you want to work with library you can add it to your project as a NuGet package:

```
Install-Package Softeq.XToolkit.DefaultAuthorization
Install-Package Softeq.XToolkit.HttpClient
```

Also you can manually download NuGets:

Package | &nbsp;
--------|-------
Softeq.XToolkit.HttpClient | [![Softeq.XToolkit.HttpClient](https://img.shields.io/nuget/v/Softeq.XToolkit.HttpClient.svg)](https://www.nuget.org/packages/Softeq.XToolkit.HttpClient/)
Softeq.XToolkit.DefaultAuthorization | [![Softeq.XToolkit.DefaultAuthorization](https://img.shields.io/nuget/v/Softeq.XToolkit.DefaultAuthorization.svg)](https://www.nuget.org/packages/Softeq.XToolkit.DefaultAuthorization/)

## Components

The library supports a bunch of components out of the box:

- `Executor`: is a special wrapper which can be used to wrap async action and execute it multiple times if it fails;
- `Mapper` : is helper which register your own mapping and then can be used to map one object to another. Also, support collections mapping;
- `UriBuilder` : is helper which can be used to build your API URLs;
- `JsonConverter` : basic JSON converter. This is a wrapper on NewtonSoft Json Converter;
- `HttpClient` : is base implementation of HTTP client for every mobile project has. We trying to encapsulate base logic to one HTTP client to cover the most popular cases of usings.

### Executor

You can register class as a singleton and then use it like the following example:

Usage:

```csharp
var allowAttempts = 10;

await Executor.ExecuteWithRetryAsync(async executionContext =>
{
    //make your http request call, if it fail executor handle it
}, allowAttempts);
```

Also support next methods:

- `Task ExecuteSilentlyAsync(Func<Task> asyncAction, Action<Exception> exceptionCallback = null)`
- `void InBackgroundThread(Func<Task> asyncAction);`
- `void InBackgroundThread(Action action);`
- `Task ExecuteActionAsync(OncePerIntervalAction oncePerIntervalAction);`

### Mapper

Helps to map data models to application models.

Usage:

```csharp
// Data transfer model
public class SettingsData
{
    public string MyCustomToken { get; set; }
}

// our application model
public class SettingsModel
{
    public string AccessToken { get; set; }
}

// Method that map data transfer model to application model
public SettingsModel Map(SettingsData data)
{
    return data == null ? null : new SettingsModel{AccessToken = data.MyCustomToken}
}

// create and register mapping
var mapper = new Mapper();
mapper.RegisterMapping<SettingsData, SettingsModel>(Map);

// create dto(or recieve it from http request)
var data = new SettingsData { MyCustomToken = "test" };

// get application model
var settingsModel = mapper.Map<SettingsModel>(data);
```
### Uri Builder

This class helped to build up and manage your api uri:

```csharp
[DataContract]
public class ItemQueryParams
{
    [DataMember(Name = "itemId")]
    public string ItemId { get; set; }
}

public Uri SaveItem(ItemQueryParams queryParams)
{
    return _uriMaker.BuildUp(queryParams, baseUrl, "/item");
}

public Uri RemoveItem(string itemId)
{
    return _uriMaker.Combine(baseUrl, "/item", itemId);
}
```

```csharp
var baseUrl = "https://example.com";

// https://example.com/item/myItemId
var removeItemUri = RemoveItem("myItemId");

// https://example.com/item?itemId=myItemId
var removeItemUri = SaveItem(new ItemQueryParams { ItemId = "myItemId" });
```

### Json Converter

Current implementation is acceptable for most mobile clients. Also used to serialize data. You can register it as a singleton and then reuse in an application.

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

Http client used to make HTTP requests. Http client has few configuration properties.

To create HttpClient you need to provide `HttpServiceGateConfig`:

```csharp
var httpConfig = new HttpServiceGateConfig
{
    // use it to define when request should be canceled, default value is 10 seconds
    DeadRequestTimeoutInMilliseconds = (int)TimeSpan.FromSeconds(100).TotalMilliseconds
};

var httpServiceGate = new HttpServiceGate(httpConfig);
```

Then you are ready to make http requests:

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

As you can see, we can configure the http request. First of all, you can specify Uri, then you should set request type(Get, Post, Put and etc), also you can send data with requests, specify content type and headers.

Full example with an http request:

```csharp
var httpConfig = new HttpServiceGateConfig
{
    DeadRequestTimeoutInMilliseconds = (int)TimeSpan.FromSeconds(100).TotalMilliseconds
};
var httpServiceGate = new HttpServiceGate(httpConfig);

var executionResult = new ExecutionResult<string>();

await Executor.ExecuteWithRetryAsync(
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

### Message Hub

This is type based message hub.

```csharp
public class MyMessage
{
    public string Name { get; } = "Kain";
}

public class MyClass : IMessageHandler<MyMessage>
{
    void MyClass(IMessageRoot messageRoot)
    {
       messageRoot.Subscribe<MyMessage>(this);
    }

    public void Handle(MyMessage message)
    {
        Console.WriteLine(message.Name);
    }
}

IMessageRoot messageRoot = new TypeBasedMessageRoot();
var myClass = new MyClass(messageRoot);
messageRoot.Raise(new MyMessage());
```
also supported async version.


## License
Library is made available under the [MIT License](http://www.opensource.org/licenses/mit-license.php).

## Credits
We're open to suggestions, feel free to open an issue. Pull requests are also welcome!
