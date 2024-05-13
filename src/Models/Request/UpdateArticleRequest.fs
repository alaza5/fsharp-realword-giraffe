namespace Models

open Giraffe

[<CLIMutable>]
type UpdateArticleRequest =
  { article:
      {| title: string option
         description: string option
         body: string option |} }

  member this.HasErrors() =
    if this.article.title |> Option.exists (fun title -> title.Length < 1) then
      Some "Title is too short."
    else if
      this.article.description
      |> Option.exists (fun description -> description.Length < 1)
    then
      Some "Description is too short."
    else if
      this.article.body |> Option.exists (fun body -> body.Length < 1)
    then
      Some "Body is too short."
    else
      None

  interface IModelValidation<UpdateArticleRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this