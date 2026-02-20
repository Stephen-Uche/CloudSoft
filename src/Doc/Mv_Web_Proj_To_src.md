# Move Web Project to `/src` Folder

1. From the project root, create the `src` folder:

```bash
mkdir -p src
```

2. Move all project files/folders into `src`, while keeping root-only items (`.git`, `.gitignore`, `ReadMe.md`, and `src`) at the root:

```bash
for item in * .*; do
  case "$item" in
    .|..|.git|.gitignore|ReadMe.md|src) continue ;;
  esac
  mv "$item" src/
done
```

3. Remove the old solution file that was moved into `src`:

```bash
rm -f src/CloudSoft.sln
```

4. Create a new solution file at the repository root:

```bash
dotnet new sln
```

5. Add the moved web project to the new solution:

```bash
dotnet sln add src/CloudSoft.csproj
```

6. Run and verify the project from its new location:

```bash
dotnet run --project src/CloudSoft.csproj
dotnet build src/CloudSoft.csproj --tl:off
```

7. Remove root build artifacts so the root remains clean:

```bash
rm -rf bin obj
```

Result: the web project is located in `src/`, and the root solution references `src/CloudSoft.csproj`.
