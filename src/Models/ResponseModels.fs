namespace Models

open System

[<CLIMutable>]
type UserResponse =
  { email: string
    token: string
    username: string
    bio: string option
    image: string option }

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
    createdAt: DateTime
    updatedAt: DateTime
    favorited: bool
    favoritesCount: int
    author: AuthorResponse }

[<CLIMutable>]
type ArticlesResponse =
  { articles: ArticleResponse list
    articlesCount: int }

[<CLIMutable>]
type CommentResponse =
  { id: int
    createdAt: DateTime
    updatedAt: DateTime
    body: string
    author: AuthorResponse }

[<CLIMutable>]
type TagsResponse = { tags: string list }

[<CLIMutable>]
type ErrorBody = { body: string list }

[<CLIMutable>]
type ErrorResponse = { errors: ErrorBody }