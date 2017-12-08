dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add "Update database"
dotnet ef database update