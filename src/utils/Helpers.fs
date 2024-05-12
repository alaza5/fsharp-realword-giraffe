namespace Helpers


module Helpers =
  open System

  let stringToInt (x: string) =
    match System.Int64.TryParse x with
    | true, num -> Some num
    | _ -> None

  let generateSlug (title: string) =
    let lowerCaseTitle = title.Trim().ToLower().Replace(@"\s+", "-")
    let guid = Guid.NewGuid().ToString() // sadge no pipe
    lowerCaseTitle + "-" + guid