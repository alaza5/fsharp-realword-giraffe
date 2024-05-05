namespace InternalSecurity

open BCrypt.Net
open System
open Microsoft.IdentityModel.Tokens
open System.Text
open System.Security.Claims
open System.IdentityModel.Tokens.Jwt


module Hashing =
  let hashWorkFactor = 11

  let hashPassword (password: string) =
    BCrypt.HashPassword(inputKey = password, workFactor = hashWorkFactor)

  let verifyPassword (password: string) (realPassword: string) =
    BCrypt.Verify(password, realPassword)

module JwtHelper =
  let private secret = "spadR2dre#u-ruBrE@TepA&*Uf@UspadR2dre#u-ruBrE@TepA&*Uf@U"

  let secretByteArray = Encoding.UTF8.GetBytes(secret)

  let issuer = "giraffe"
  let audience = "fsharp"


  let generateToken email =
    let claim1 = Claim(JwtRegisteredClaimNames.Sub, email)
    let claim2 = Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

    let now = DateTime.UtcNow
    let from = now
    let until = now.AddHours 1.0

    let securityKey = SymmetricSecurityKey(secretByteArray)

    let signingCredentials =
      SigningCredentials(key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)

    let token =
      JwtSecurityToken(
        issuer = issuer,
        audience = audience,
        claims = [| claim1; claim2 |],
        notBefore = from,
        expires = until,
        signingCredentials = signingCredentials
      )

    let tokenResult = JwtSecurityTokenHandler().WriteToken(token)

    tokenResult