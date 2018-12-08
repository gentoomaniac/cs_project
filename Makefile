test:
	dotnet restore project.csproj
	dotnet test project.csproj --logger:"console;verbosity=normal"