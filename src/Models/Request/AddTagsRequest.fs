namespace Models

open Giraffe

[<CLIMutable>]
type AddTagsRequest =
  { tags: string list }

  member this.HasErrors() =
    if this.tags.IsEmpty then
      Some "Tags list cannot be empty."
    elif this.tags |> List.exists (fun tag -> tag.Length < 1) then
      Some "Tags cannot be empty."
    else
      None

  interface IModelValidation<AddTagsRequest> with
    member this.Validate() =
      match this.HasErrors() with
      | Some msg -> Error(RequestErrors.badRequest (text msg))
      | None -> Ok this