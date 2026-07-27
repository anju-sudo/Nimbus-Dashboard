# Packaging

Deployment and packaging notes for Nimbus Board.

## Local run

```bash
dotnet run --project src/Host/NimbusBoard.Web/NimbusBoard.Web.csproj
```

## Publish

```bash
dotnet publish src/Host/NimbusBoard.Web/NimbusBoard.Web.csproj -c Release -o ./artifacts/web
```

## Notes

- Umbraco media and SQLite databases live under the host project's `umbraco/Data` (gitignored).
- Keep secrets in `appsettings.Development.json` (gitignored); use environment variables or user secrets in shared environments.
- CI builds and tests the full solution via `.github/workflows/build.yml`.
