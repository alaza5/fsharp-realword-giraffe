namespace Models

open System

module DatabaseModels =

  // TODO try to rename it to something that make more sence
  // it's called userS becasue it's how Dapper fsharp worked
  [<CLIMutable>]
  type users =
    { id: Guid
      email: string
      username: string
      password: string
      bio: string option
      image: string option
      created_at: DateTime
      updated_at: DateTime }

  [<CLIMutable>]
  type articles =
    { id: Guid
      author_id: Guid
      slug: string
      title: string
      description: string
      body: string
      created_at: DateTime
      updated_at: DateTime }

  type articles_tags = { article_id: Guid; tag_id: Guid }

  type tags = { id: Guid; name: string }


  type ArticleUserTags =
    { article: articles
      user: users
      tags: string array }