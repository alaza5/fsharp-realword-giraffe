namespace Models

open System

[<CLIMutable>]
type UserResponse =
  { user:
      {| email: string
         token: string
         username: string
         bio: string option
         image: string option |} }

[<CLIMutable>]
type AuthorResponse =
  { username: string
    bio: string
    image: string
    following: bool }

[<CLIMutable>]
type ProfileResponse =
  { profile:
      {| username: string
         bio: string
         image: string
         following: bool |} }

[<CLIMutable>]
type SingularArticleResponse =
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
type ArticleResponse = { article: SingularArticleResponse }

[<CLIMutable>]
type ArticlesResponse =
  { articles: SingularArticleResponse list
    articlesCount: int }

[<CLIMutable>]
type SingularCommentResponse =
  { id: Guid
    createdAt: string
    updatedAt: string
    body: string
    author: AuthorResponse }

[<CLIMutable>]
type CommentResponse = { comment: SingularCommentResponse }

[<CLIMutable>]
type CommentsResponse =
  { comments: SingularCommentResponse list }

[<CLIMutable>]
type TagsResponse = { tags: string list }

[<CLIMutable>]
type ErrorBody = { body: string list }

[<CLIMutable>]
type ErrorResponse = { errors: ErrorBody }