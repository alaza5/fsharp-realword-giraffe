namespace Models

[<CLIMutable>]
type LoginRequest = { email: string; password: string }

[<CLIMutable>]
type RegisterRequest =
  { username: string
    email: string
    password: string }

[<CLIMutable>]
type UpdateUserRequest =
  { email: string option
    username: string option
    password: string option
    image: string option
    bio: string option }

[<CLIMutable>]
type CreateArticleRequest =
  { title: string
    description: string
    body: string
    tagList: string list option }


[<CLIMutable>]
type UpdateArticleRequest =
  { title: string option
    description: string option
    body: string option }

[<CLIMutable>]
type AddArticleCommentRequest = { body: string }

[<CLIMutable>]
type AddTagsRequest = { tags: string list }

[<CLIMutable>]
type GetTagIdsRequest = { tags: string list }