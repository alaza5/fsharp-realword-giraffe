namespace Models

open Giraffe

[<CLIMutable>]
type UpdateUserRequest =
  { user:
      {| email: string option
         username: string option
         password: string option
         image: string option
         bio: string option |} }

  member this.HasErrors() =
    if this.user.email |> Option.exists (fun email -> email.Length < 1) then
      Some "Invalid email address."
    else if
      this.user.username |> Option.exists (fun username -> username.Length < 1)
    then
      Some "Username is too short."
    else if
      this.user.password |> Option.exists (fun password -> password.Length < 1)
    then
      Some "Password is too short."
    else
      None

  interface IModelValidation<UpdateUserRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this