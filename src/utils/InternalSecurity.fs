namespace InternalSecurity

open BCrypt.Net

module InternalSecurity =
  let workFactor = 20

  let hashPassword (password: string) =
    BCrypt.HashPassword(inputKey = password, workFactor = workFactor)