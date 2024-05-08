namespace ModelsMappers


open Models
open InternalSecurity

module ResponseToDbMappers =
  open System

  type RegisterRequest with

    member this.toDbModel =
      let user: DatabaseModels.users =
        { id = Guid.NewGuid()
          email = this.email
          username = this.username
          password = Hashing.hashPassword this.password
          bio = None
          image = None
          created_at = DateTime.UtcNow
          updated_at = DateTime.UtcNow }

      user

  type CreateArticleRequest with

    member this.toDbModel(user: DatabaseModels.users) =
      let generatedSlug =
        this.title.Trim().ToLower().Replace(@"\s+", "-")
        + "-"
        + Guid.NewGuid().ToString() // sadge no pipe

      let article: DatabaseModels.articles =
        { id = Guid.NewGuid()
          author_id = user.id
          slug = generatedSlug
          title = this.title
          description = this.description
          body = this.body
          created_at = DateTime.UtcNow
          updated_at = DateTime.UtcNow }

      article