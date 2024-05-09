namespace DapperExtensions

open System
open System.Data
open Dapper

module DapperExtensions =

  // could probably pass it as HOF?
  type IDbConnection with

    member this.QuerySingleAsyncResult<'T>(query, ?param) =
      task {
        try
          let! result = this.QuerySingleAsync<'T>(query, param)
          return Ok result
        with ex ->
          return Error ex.Message
      }

    member this.QueryAsyncResult<'T>(query, ?param) =
      task {
        try
          let! result = this.QueryAsync<'T>(query, param)
          return Ok result
        with ex ->
          return Error ex.Message
      }