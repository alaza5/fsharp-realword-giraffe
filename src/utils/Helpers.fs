namespace Helpers


module Helpers =

  let stringToInt (x: string) =
    match System.Int64.TryParse x with
    | true, num -> Some num
    | _ -> None