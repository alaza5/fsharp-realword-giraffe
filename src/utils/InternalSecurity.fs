namespace InternalSecurity

open BCrypt.Net

module InternalSecurity =
  let workFactor = 11

  let hashPassword (password: string) =
    BCrypt.HashPassword(inputKey = password, workFactor = workFactor)