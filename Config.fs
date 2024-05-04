namespace Config

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Controller

module Config =
  open System.Data
  open Microsoft.Extensions.Configuration
  open Newtonsoft.Json
  open Microsoft.FSharpLu.Json

  let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(
      ex,
      "An unhandled exception has occurred while executing the request."
    )

    clearResponse >=> setStatusCode 500 >=> text ex.Message

  let configureCors (builder: CorsPolicyBuilder) =
    builder
      .WithOrigins("http://localhost:5000", "https://localhost:5001")
      .AllowAnyMethod()
      .AllowAnyHeader()
    |> ignore

  let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    let isDevelepment = env.IsDevelopment()
    printfn $">> ENV: {env.EnvironmentName}"

    let builder =
      match isDevelepment with
      | true -> app.UseDeveloperExceptionPage()
      | false -> app.UseGiraffeErrorHandler(errorHandler).UseHttpsRedirection()

    let messageController = Controller.root

    builder
      .UseCors(configureCors)
      .UseStaticFiles()
      .UseGiraffe(messageController)

  let getSerializer =
    let customSettings =
      JsonSerializerSettings(
        ContractResolver =
          Compact.Strict.RequireNonOptionalPropertiesContractResolver()
      )

    // if something breaks with serialization maybe should be (true,true)?
    customSettings.Converters.Add(CompactUnionJsonConverter(true))
    NewtonsoftJson.Serializer(customSettings)

  let configureServices (services: IServiceCollection) : unit =

    services
      .AddCors()
      .AddGiraffe()
      .AddTransient<IDbConnection>(fun serviceProvider ->
        let settings = serviceProvider.GetService<IConfiguration>()
        let conn = settings.["DbConnection"]
        new Npgsql.NpgsqlConnection(conn))
      .AddRouting()
      .AddSingleton<Json.ISerializer>(getSerializer)
    |> ignore

  let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore