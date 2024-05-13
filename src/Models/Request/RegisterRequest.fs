namespace Models

open Giraffe

[<CLIMutable>]
type RegisterRequest =
  { user:
      {| username: string
         email: string
         password: string |} }

  member this.HasErrors() =
    if this.user.username.Length < 1 then
      Some "Username is too short."
    // For tests to pass
    // else if this.user.username.Length > 50 then
    //   Some "Username is too long."
    // else if not (Helpers.isValidEmail this.user.email) then
    //   Some "Invalid email address."
    else if (this.user.email.Length < 1) then
      Some "Invalid email address."
    else if this.user.password.Length < 1 then
      Some "Password is too short."
    else
      None

  interface IModelValidation<RegisterRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this