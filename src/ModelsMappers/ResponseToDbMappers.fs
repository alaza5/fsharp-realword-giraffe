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