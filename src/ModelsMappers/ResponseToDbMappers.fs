namespace ModelsMappers



module ResponseToDbMappers =
  open Models
  open Utils
  open System

  type RegisterRequest with

    member this.toDbModel =
      let user: DbTables.UsersTable =
        { id = Guid.NewGuid()
          email = this.user.email
          username = this.user.username
          password = Hashing.hashPassword this.user.password
          bio = None
          image = None
          created_at = DateTime.UtcNow
          updated_at = DateTime.UtcNow }

      user

  type CreateArticleRequest with

    member this.toDbModel(user: DbTables.UsersTable) =
      let generatedSlug = Helpers.generateSlug this.article.title

      let article: DbTables.ArticlesTable =
        { id = Guid.NewGuid()
          author_id = user.id
          slug = generatedSlug
          title = this.article.title
          description = this.article.description
          body = this.article.body
          created_at = DateTime.UtcNow
          updated_at = DateTime.UtcNow }

      article