# Contribuer à l’API Tunetalk

## Conventions

- Utiliser les namespaces et types C# en anglais, les descriptions produit en français.
- Conserver le sens de dépendance `Api -> Application -> Domain`; Infrastructure implémente les ports définis par Application.
- Toute règle métier va dans Domain/Application, jamais dans un contrôleur ou un hub.
- Tout secret vient de User Secrets en local ou de la configuration sécurisée de l’environnement.
- Ajouter ou mettre à jour un test pour toute règle métier modifiée.

## Commits

Utiliser [Conventional Commits](https://www.conventionalcommits.org/) :

```text
feat(auth): issue access tokens on registration
fix(chat): reject messages outside the joined channel
docs(api): explain radio provider rights
test(domain): cover chat message validation
ci(api): publish container tags
```

Types usuels : `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.

## Vérifications avant push

```powershell
dotnet tool restore
dotnet format whitespace .\TunetalkApi.Api.slnx --verify-no-changes
dotnet test .\TunetalkApi.Api.slnx --configuration Release
dotnet publish .\TunetalkApi.Api\TunetalkApi.Api.csproj --configuration Release
```

Les pull requests déclenchent le workflow `API CI`. Un tag `api-v*` validé publie l’image sur GHCR. Le déploiement runtime dépend du fournisseur et doit fournir des secrets, TLS et stockage persistant.
