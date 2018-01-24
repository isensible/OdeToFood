# ASP.NET Core Fundamentals - course notes

# Building your first ASP.NET Core Application

## dotnet command line
* `dotnet --version`
    Print the version
* `dotnet --help`
    Print list of available `dotnet` commands
* `dotnet new web`
    Creates an empty ASP.NET Core web application with the name of parent folder
* `dotnet restore`
* `dotnet build`
* `dotnet run`

## Create an empty ASP.NET Core web project from command line
1. `cd C:\code`
1. `mkdir OdeToFood`
1. `cd OdeToFood`
1. `dotnet new web`

This creates the following project structure:

* `OdeToFood.csproj`
* `Program.cs`
* `Startup.cs`
* `wwwroot\`
* `obj\`

## Create an empty ASP.NET Core web project from Rider or Visual Studio
Creating an empty project with Rider will create the above structure nested
below a `OdeToCode\OdeToCode\` with a `OdeToCode\OdeToCode.sln` in which to
manage the empty project.

## OdeToFood.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>      
    <!-- Each possible target has a moniker (short name).
    Could target full .NET Framework with moniker `net46` -->
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Special folder -->
    <Folder Include="wwwroot\" />
  </ItemGroup>

  <ItemGroup>
    <!-- `PackageReference` brings in a nuget package. 
    `Microsoft.AspNetCore.All` is a "meta-package" because it brings in
    additional packages. -->
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.5" />
  </ItemGroup>

</Project>
```

No listing of source files in `csproj` file. Included files are taken from files existing in the project folder and below.

## Program.cs
```cs
...
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
...
namespace OdeToFood
{
    public class Program
    {
        // entry point to the app
        // ASP.NET Core apps are structured as console apps
        // `dotnet run` looks for this method
        // VS puts this behind IIS Express which acts as reverse proxy
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        // `WebHost.CreateDefaultBuilder(args)` spins up a web-server.
        // Builder configured to use `Startup` class
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
```

## Startup.cs
```cs
namespace OdeToFood
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Responds to all requests with "Hello World!" content.
            app.Run(async (context) => { await context.Response.WriteAsync("Hello World!"); });
        }
    }
}
```

# Adding Configuration

## WebHost with CreateDefaultBuilder

1. Will use Kestrel web server
2. ISS integration if app is running behind ISS. Pass through Windows credentials; important for intranet applications.
3. Logging for console output.
4. `IConfiguration` service made available. By default configuration data comes from the following sources in this order:
    1. JSON file (appsettings.json)
    2. User secrets
    3. Environment variables
    4. Command line arguments

## IConfiguration service

Create a file named `appsettings.json` to root of project.
```json
{
  "Greeting": "Hello!!"
}
```
Modify `Startup.cs` to pull in `IConfiguration` as a dependency
and then use the `Greeting` configuration value as the new default
reponse to all requests.

```cs
public class Startup
{
    public void Configure(
        IApplicationBuilder app, 
        IHostingEnvironment env,
        IConfiguration configuration)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Run(async (context) =>
        {
            var greeting = configuration["Greeting"];
            await context.Response.WriteAsync(greeting);
        });
    }
}
```

With later sources overriding prior sources, one can use `appsettings.json` in `Development` mode, then when deploying to `Production` use `Environment Variables` or `Command line arguments` to override secrets such as connection strings.

# Dependency injection

## Registering services

In `Startup.cs` services must be registered in the `ConfigureServices(IServiceCollection services)` method.

There are many built-in services that can be registered here:
* `services.AddAntiforgery`
* `services.AddAuthentication`

### Registering a custom service

* `services.AddSingleton` - a single instance for the entire app
* `services.AddTransient` - a new instance for each dependency
* `services.AddScoped` - a single instance for each request

For each of these methods there are many overloads that afford further control over the construction and lifecycle of the registered services.

---
# Middleware

Each piece of middleware is an object with a specific function, such as logging, authorization, or routing a request.

## UseStartup(Startup)
`IApplicationBuilder.UseStartup(Startup)` will call in order the following methods of the `Startup` class:
1. `Startup.ConfigureServices(IServiceCollection services)`
2. `Configure(...)`

`Configure(...)` is where middleware components are registered.

Be careful with the order you register middleware.

## Using custom middleware
```cs
public void Configure(
    IApplicationBuilder app, 
    IHostingEnvironment env,
    IGreeter greeter,
    ILogger<Startup> logger)
{
    app.Use(next =>
    {
        return async context =>
        {
            logger.LogInformation("Request incoming");
            if (context.Request.Path.StartsWithSegments("/mym"))
            {
                await context.Response.WriteAsync("Hit!!");
                logger.LogInformation("Request handled");
            }
            else
            {
                await next(context);
                logger.LogInformation("Request outgoing");
            }
        };
    });

    app.UseWelcomePage(new WelcomePageOptions
    {
        Path = "/wp"
    });
    
    app.Run(async (context) =>
    {
        var greeting = greeter.GetMessageOfTheDay();
        await context.Response.WriteAsync(greeting);
    });
}
```

In the above code the `Use(next => {});` call will execute the delegate once per request. The code first logs a request is "incoming" before deciding if to respond to the request ("handled") or to pass the request ("outgoing") onto the next piece of middleware in the pipeline.

The `UseWelcomePage(...)` is an example of a built-in piece of middleware that will display the ASP.NET Core Welcome page. If no `Path` is specified then the WelcomePage component will respond to the request with the HTML of the Welcome Page. If the `Path` is specified the WelcomePage will displayed if the `Path` matches the route of the request, otherwise the request will be passed onto the the next piece of middleware.

## Showing Exceptions

When debugging use the following code to enable the ASP.NET exception page.
```cs
public void Configure(
    IApplicationBuilder app, 
    IHostingEnvironment env,
    IGreeter greeter,
    ILogger<Startup> logger)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    ...
}
```
### IHostingEnvironment service
Find out about the runtime environment:
* `env.IsProduction`
* `env.IsDevelopment`
* `env.IsStaging`
* `env.IsEnvironment(string environmentName)` - e.g IsEnvironment("QA")

The name of the environment is taken from:
* `env.EnvironmentName`

To set the `EnvironmentName` set the environment variable `ASPNETCORE_ENVIRONMENT`.

From VS check `launchSetting.json` to set profiles for executing the application.

Can use `appsetting.json` as base settings for environment specific settings by creating `appsettings.Development.json`

## Serving files

### Static files
`app.UseStaticFiles()` configures the app to allow access to static files, such as JS, images and HTML files. It checks the URL of the incoming request, and if it matches a file in the `wwwroot\` then it will respond by serving that file.

By default, this middleware will only serve files in `wwwroot\`. This makes it easy for developers to place all files for serving into this folder and avoid exposing to the Internet source code, settings, or application secrets.

For example, adding a HTML file `index.html` under the project's `wwwroot\` folder, you would expect to access the file with `http:www.odetofood.com/index.html`. With `app.UseStaticFiles` in place the URL will work as expected. Requests for any other URL will fall-through to the `app.Run(...)` middleware.

### Default files
`app.UseDefaultFiles()`

Look at request, if for a directory, then look for a default file. `index.html` is a default file, so adding `app.UseDefaultFiles()` will allow access to `index.html`.

Must come **before** `app.UseStaticFiles()` so `app.UseDefaultFiles` can properly setup the environment for static files to work. 

```cs
public void Configure(
    IApplicationBuilder app, 
    IHostingEnvironment env,
    IGreeter greeter,
    ILogger<Startup> logger)
{
    // ...snip...
    app.UseDefaultFiles();
    app.UseStaticFiles();
    // ...snip...
}
```

### File server
`app.UseFileServer()`

Installs `app.UseDefaultFiles()` and `app.UseStaticFiles()`

Options can enable things such as directory browsing.

## Setting up ASP.NET MVC

1. Add the package dependency
2. Add the MVC services
3. Add the MVC middleware

### Add the package dependency
The `Microsoft.ApsNetCore.All` package, is a meta-package that brings in all the ASP.NET Core packages including ASP.NET Core MVC. This package is already included in the empty project template, so there are no further dependencies to add.

From an empty project's `csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.5" />
</ItemGroup>
```
### Add services and middleware

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGreeter, Greeter>();
        // Add MVC services to route requests through the MVC framework
        services.AddMvc();
    }

    public void Configure(
        IApplicationBuilder app, 
        IHostingEnvironment env,
        IGreeter greeter,
        ILogger<Startup> logger)
    {
        /// ...snip...

        // serve a file from wwwroot only if URL matches exactly
        app.UseStaticFiles();

        // configure MVC with default routing
        app.UseMvcWithDefaultRoute();

        // will only reach this code if the MVC middleware doesn't pick up the
        // request route
        app.Run(async (context) =>
        {
            var greeting = greeter.GetMessageOfTheDay();
            await context.Response.WriteAsync(greeting);
        });
    }
}
```

### Creating a very basic HomeController

1. Create a `Controllers` folder
2. In `Controllers`, add an empty class called `HomeController`
3. Add public method `string Index()`

```cs
namespace OdeToFood.Controllers
{
    public class HomeController
    {
        public string Index()
        {
            return "Hello from HomeController";
        }
    }
}
```
Note: no *need* to inherit from the ASP.NET Core `Contoller` class.

At this point we are ready to begin using the ASP.NET MVC framework.

# Controllers

1. Controller accepts a request
2. Builds a Model
3. Returns a model (API) or render a View (HTML)

## Routing

ASP.NET uses "convention based routing". Uses templates that define how to map URLs to actions on controllers.

```cs
routeBuilder.MapRoute("Default",
    "{controller=Home}/{action=Index}/{id}");
```

## Conventional routes

`app.UseMvc();` doesn't know how to map routes to actions, so use
the overload to specify how routes are mapped to actions.

### Setting default route mapping
```cs
            app.UseMvc(ConfigureRoutes);

            // 
            app.Run(async (context) =>
            {
                var greeting = greeter.GetMessageOfTheDay();
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Not found");
            });
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            // To map: /Home/Index
            //routeBuilder.MapRoute("Default",
            //    "{controller=Home}/{action=Index}");
            
            // To map: /admin/Home/Index
            //routeBuilder.MapRoute("Default",
            //    "admin/{controller}/{action}");
            
            // To map: /Home/Index/4
            //routeBuilder.MapRoute("Default",
            //    "{controller}/{action}/{id}");
            
            // Use "{id?}" to map with optional query string: /Home/Index/4 
            routeBuilder.MapRoute("Default", 
                "{controller}/{action}/{id?}");
        }

```

To set defaults for the `/home/index/?` action:
```cs
            // To set defaults add defaults to the route config
            routeBuilder.MapRoute("Default", 
                "{controller=Home}/{action=Index}/{id?}");
```

## Attribute based routing using RouteAttribute
Create a route specific to an action or controller, avoiding the configured routing.

* Contoller level
* Action level

```cs
// Attribute route with [controller] token to take name from class
// so this picks up "/about"
[Route("[controller]")]
public class AboutController
{
    [Route("")]
    public string Phone()
    {
        return "0458 109311";
    }

    // [action] token takes method name
    [Route("[action]")]
    public string Address()
    {
        return "2c Brentham Street, Leederville, WA 6007";
    }

    [Route("email")]
    public string Email()
    {
        return "ed.james@hotmail.co.uk";
    }
}
```

## Controller and IActionResult
```cs
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Content("Hello from HomeController");
    }
}
```

Or to return a model
```cs
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var model = new Restaurant
        {
            Id = 1,
            Name = "Cindy's"
        };
        
        // ObjectResult uses content negotiation
        return new ObjectResult(model);
    }
}
```

## Rendering views

`ViewResult` carries the name of the `Razor` View object, and the model.

The following is `Views\Home\Index.cshtml` showing the model restaurant name.

```
@model OdeToFood.Models.Restaurant 

<!DOCTYPE html>

<html>
<head>
    <title>Ode to Food</title>
</head>
<body>
<div>
    <h1>@Model.Name</h1>
    <div>This is the Index.cshtml</div>
</div>
</body>
</html>
```

## Displaying a collection

```cs
    public class HomeController : Controller
    {
        private readonly IRestaurantData _restaurantData;

        public HomeController(IRestaurantData restaurantData)
        {
            _restaurantData = restaurantData;
        }
        
        public IActionResult Index()
        {
            var model = _restaurantData.GetAll();
            
            return View(model);
        }
    }
```

```
@model IEnumerable<OdeToFood.Models.Restaurant> 

<!DOCTYPE html>

<html>
<head>
    <title>Ode to Food</title>
</head>
<body>
<div>
    <table>
        @foreach (var restaurant in Model)
        {
            <tr>
                <td>@restaurant.Id</td>
                <td>@restaurant.Name</td>
            </tr>
        }
     </table>
</div>
</body>
</html>
```

# Models and ViewModels

## Entity Model
Maps to the database

* Restaurant
    * Name
    * Address
    * CuisineType

## View Model
Maps to the View

* RestaurantEdit
    * Name
    * Address
    * CuisineType
    * List of CuisineType for editing CuisineType property

## Adding Tag Helpers with _ViewImports.cshtml
`_ViewImports.cshtml` is a special Razor view that doesn't render anything, but exists to import dependencies to assist with the building of Razor views.

It provides instructions to the Razor engine.

Here follows an example of `_ViewImports.cshtml` to bring into play all of ASP.NET Core MVC tag helpers:
```
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

## Viewing Details

Three ways to link to another Action:
1. Using hand-coded `<a>` tag
2. Using `@Html.ActionLink("Text", "Action", anon-obj-with-action-params)`
3. Using <a> tag with tag-helpers, such as `asp-action="Details"` and `asp-route-xyz=@restaurant.Xyz`

```html
<table>
    @foreach (var restaurant in Model.Restaurants)
    {
        <tr>
            <td>@restaurant.Id</td>
            <td>@restaurant.Name</td>
            <td>
                <a href="/home/details/@restaurant.Id">Details</a>
            </td>
            <td>
                @Html.ActionLink("Go", "Details", new { id = @restaurant.Id })
            </td>
            <td>
                <a asp-action="Details" asp-route-id="@restaurant.Id">More</a>
            </td>
        </tr>
    }
</table>
```
The best linking method is to use Tag Helpers:

```
<a asp-action="Details" asp-route-id="@restaurant.Id">More</a>
```
* `asp-action="Details"` - sets the name of the action the link should invoke
* `asp-route-foo="bar"` - sets to "bar" the `foo` parameter of the action

Another example linking to an action on a different controller:
```
<a asp-action="Index" asp-controller="Home">Home</a>
```
* `asp-controller="Home"` - sets the controller on which to invoke the action

# Create a new Restaurant

Using `<form>` element

Every input control in form requires a name so ASP.NET MVC can map the names to the input parameters of the `POST` action on the controller.

## CSRF - Cross Site Request Forgery
The `__RequestVerificationToken` hidden input is added by ASP.NET Core to prevent CSRF. Helps to ensure form user submits is the form created by our application.
```html
<input name="__RequestVerificationToken" type="hidden" value="CfDJ8FSUGXur3DNEqDLk9wYoxlH7ov9X5TrUgII3l03-qq6LxgU98E3siMcgarAVLleW9BxXOpr5Zn-zOZjL4en-NWvx2KcPZ3hvcvibqisv97tUA703AzKe0etnI5fa_KeBjv_td7wY_rQ-WwMZMpqDQvA">
```

# Layout

`Views\Shared\_Layout.cshtml` - default view layout HTML.

`Views\_ViewStart.cshtml` - This is run before any other view code. Use this to set the default layout view for all views.

# Razor Pages

Can accept Requests directly.

# Partial Views

`@Html.Partial("_Summary", restaurant)`

Can't get its own Model. Must use parent Model



# View Components

Independent view components.
Render anywhere on the page.  
Build own Model.


















