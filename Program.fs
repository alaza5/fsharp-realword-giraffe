module giraf.App

open System.IO
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Config

[<EntryPoint>]
let main args =
  let contentRoot = Directory.GetCurrentDirectory()
  let webRoot = Path.Combine(contentRoot, "WebRoot")


  Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(fun webHostBuilder ->
      webHostBuilder
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .Configure(Config.configureApp)
        .ConfigureServices(Config.configureServices)
        .ConfigureLogging(Config.configureLogging)
      |> ignore)
    .Build()
    .Run()

  0