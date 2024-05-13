namespace Repository

open Models
open System
open Npgsql.FSharp
open System.Threading.Tasks


module UsersRepository =

  let createAndGetUser (data: DbTables.UsersTable) : Task<DbTables.UsersTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"INSERT INTO users
          (id, email, username, password, bio, image, created_at, updated_at)
        VALUES
          (@id, @email, @username, @password, @bio, @image, @created_at, @updated_at)
        RETURNING *"
    |> Sql.parameters
      [ "id", Sql.uuid data.id
        "email", Sql.string data.email
        "username", Sql.string data.username
        "bio", Sql.stringOrNone data.bio
        "password", Sql.string data.password
        "image", Sql.stringOrNone data.image
        "created_at", Sql.date data.created_at
        "updated_at", Sql.date data.updated_at ]
    |> Sql.executeRowAsync Common.readUser


  //TODO maybe extract getting user by different props (email/id/itd)
  let getUserByEmail (email: string) : Task<DbTables.UsersTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users 
        WHERE email = @email
        LIMIT 1
       "
    |> Sql.parameters [ "@email", Sql.string email ]
    |> Sql.executeRowAsync Common.readUser

  let getUserById (userId: Guid) : Task<DbTables.UsersTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users
        WHERE id = @id
        LIMIT 1
       "
    |> Sql.parameters [ "id", Sql.uuid userId ]
    |> Sql.executeRowAsync Common.readUser


  let getUserByUsername (username: string) : Task<DbTables.UsersTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users 
        WHERE username = @username
        LIMIT 1
       "
    |> Sql.parameters [ "@username", Sql.string username ]
    |> Sql.executeRowAsync Common.readUser


  let updateAndReturnUser
    (currentUserEmail: string)
    (data: DbTables.UsersTable)
    : Task<DbTables.UsersTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"UPDATE users 
          SET 
            id = @id, 
            email = @Email, 
            username = @username, 
            password = @password, 
            bio = @bio, 
            image = @image, 
            created_at = @created_at, 
            updated_at = @updated_at 
          WHERE 
            email = @currentUserEmail
          RETURNING *
        "
    |> Sql.parameters
      [ "id", Sql.uuid data.id
        "Email", Sql.string data.email
        "username", Sql.string data.username
        "password", Sql.string data.password
        "bio", Sql.stringOrNone data.bio
        "image", Sql.stringOrNone data.image
        "created_at", Sql.date data.created_at
        "updated_at", Sql.date data.updated_at
        "currentUserEmail", Sql.string currentUserEmail ]
    |> Sql.executeRowAsync Common.readUser



  // FOLLOWS

  let getFollowingUsers (userId: Guid) : Task<DbTables.UsersTable list> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT u.*
        FROM follows f
        LEFT JOIN users u ON u.id = f.following_id
        where f.user_id = @userId
        "
    |> Sql.parameters [ "@userId", Sql.uuid userId ]
    |> Sql.executeAsync Common.readUser

  let addFollow (userId: Guid) (followingId: Guid) =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO follows
        (user_id, following_id)
      VALUES 
        (@userId, @followingId)
      ON CONFLICT (user_id, following_id)
      DO NOTHING
      "
    |> Sql.parameters
      [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
    |> Sql.executeNonQueryAsync

  let removeFollow (userId: Guid) (followingId: Guid) =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE FROM follows
      WHERE 
        user_id = @userId
      AND 
        following_id = @followingId
      "
    |> Sql.parameters
      [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
    |> Sql.executeNonQueryAsync



// let isFollowing (userId: Guid) (followingId: Guid) : Task<Count> =
//   Common.connectionString
//   |> Sql.connect
//   |> Sql.query
//     @"
//       SELECT COUNT(*)
//       FROM follows
//       WHERE user_id = @userId
//       AND following_id = @followingId
//       "
//   |> Sql.parameters [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
//   |> Sql.executeRowAsync (fun read -> { count = read.int64 "count" })


// let getUserWithFollowers (username: string) : Task<DbModels.UserWithFollowerList> =
//   Common.connectionString
//   |> Sql.connect
//   |> Sql.query
//     @"
//     SELECT
//       row_to_json(u.*) AS user,
//       (
//         SELECT json_agg(fu.*)
//         FROM follows f
//         JOIN users fu ON f.user_id = fu.id
//         WHERE f.following_id = u.id
//       ) AS followers
//     FROM users u
//     WHERE u.username = @username
//     "
//   |> Sql.parameters [ "@username", Sql.string username ]
//   |> Sql.executeRowAsync (fun read ->
//     { user = read.fieldValue<DbTables.UsersTable> "user"
//       followers = read.fieldValue<DbTables.UsersTable list> "followers" })