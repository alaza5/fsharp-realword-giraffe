namespace Models

open System

module DatabaseModels =

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