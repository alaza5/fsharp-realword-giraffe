namespace Models

[<CLIMutable>]
type UserResponse =
  { email: string
    token: string
    username: string
    bio: string
    image: string }

[<CLIMutable>]
type AuthorResponse =
  { username: string
    bio: string
    image: string
    following: bool }

[<CLIMutable>]
type ProfileResponse =
  { username: string
    bio: string
    image: string
    following: bool }

[<CLIMutable>]
type ArticleResponse =
  { slug: string
    title: string
    description: string
    body: string
    tagList: string list
    createdAt: string
    updatedAt: string
    favorited: bool
    favoritesCount: int
    author: AuthorResponse }

[<CLIMutable>]
type ArticlesResponse =
  { articles: Article list
    articlesCount: int }

[<CLIMutable>]
type CommentResponse =
  { id: int
    createdAt: string
    updatedAt: string
    body: string
    author: AuthorResponse }

[<CLIMutable>]
type TagsResponse = { tags: string list }



[<CLIMutable>]
type ErrorBody = { body: string list }

[<CLIMutable>]
type ErrorResponse = { errors: ErrorBody }