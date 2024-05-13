namespace Models

open Giraffe

[<CLIMutable>]
type CreateArticleRequest =
  { article:
      {| title: string
         description: string
         body: string
         tagList: string list option |} }

  member this.HasErrors() =
    if this.article.title.Length < 1 then
      Some "Title is too short."
    elif this.article.description.Length < 1 then
      Some "Description is too short."
    elif this.article.body.Length < 1 then
      Some "Body is too short."
    else
      None

  interface IModelValidation<CreateArticleRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this