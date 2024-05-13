namespace Models

open Giraffe

[<CLIMutable>]
type AddArticleCommentRequest =
  { comment: {| body: string |} }

  member this.HasErrors() =
    if this.comment.body.Length < 1 then
      Some "Comment body is too short."
    else
      None

  interface IModelValidation<AddArticleCommentRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this