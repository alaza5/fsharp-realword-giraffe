# ![RealWorld Example App](logo.png)

> ### [YOUR_FRAMEWORK] codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld) spec and API.


### [Demo](https://demo.realworld.io/)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)


This codebase was created to demonstrate a fully fledged fullstack application built with **[YOUR_FRAMEWORK]** including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the **[YOUR_FRAMEWORK]** community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.


# How it works
Used libraries
  - Npgsql (db)
  - Microsoft.FSharpLu.Json (deserialization)
  - BCrypt (password hashing)
  - Microsoft.AspNetCore.Authentication.JwtBearer (JWT)
  
> Describe the general architecture of your app here

# Getting started

1. docker compose up
2. liquibase --changeLogFile=changelog.xml update
3. dotnet run


Things to do:
- fix response mappers (currently all over the place)
- add unit tests
- add some logging
- do some DI?
- persist db connection? (DI?)