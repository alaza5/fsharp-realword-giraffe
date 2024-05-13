namespace Utils


module Helpers =
  open System
  open Giraffe
  open System.Text.RegularExpressions
  open Microsoft.AspNetCore.Http

  let stringToInt (x: string) =
    match System.Int64.TryParse x with
    | true, num -> Some num
    | _ -> None

  let generateSlug (title: string) =
    let lowerCaseTitle = title.Trim().ToLower().Replace(@"\s+", "-")
    let guid = Guid.NewGuid().ToString() // sadge no pipe
    lowerCaseTitle + "-" + guid

  let isValidEmail (email: string) =
    let emailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"
    Regex.IsMatch(email, emailRegex)


  let DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ"