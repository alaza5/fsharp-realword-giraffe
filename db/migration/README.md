To run migration:

  `liquibase --changeLogFile=changelog.xml update`


to login to db

  psql -U myuser -h localhost -p 5432 -d fsharp_giraffe_database -W
  
  password: mypassword
