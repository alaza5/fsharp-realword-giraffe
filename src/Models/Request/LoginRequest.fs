namespace Models


[<CLIMutable>]
type LoginRequest =
  { user: {| email: string; password: string |} }