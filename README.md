# LuanVan
1. edit connection string in appsettings.json, Data/ApplicationDbContext.cs and Areas/Admin/Controller/ReportController.cs
"Data Source=<<server-name>>;Initial Catalog=<<database-name>>;TrustServerCertificate=True; Integrated Security=True"
  
2. run command after to create a database
$ dotnet ef database update

