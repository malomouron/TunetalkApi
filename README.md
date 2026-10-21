# Tunetalk API

API ASP.NET Core 10 de Tunetalk : comptes, catalogue radio externe, chat persistant et salons SignalR.

## Choix produit : les stations radio

Le MVP utilise [Radio Browser](https://api.radio-browser.info/), un annuaire communautaire gratuit et open source. Il expose des stations existantes par tags/genres ; les stations et leurs flux sont exploités par leurs diffuseurs. Tunetalk ne relaie ni ne stocke l’audio : l’application Android lit directement le flux HTTPS fourni par la station. L’API ne transmet au mobile que les URL HTTPS.

Radio Browser est un **annuaire**, pas un fournisseur de musique et ne garantit pas les droits de diffusion ou de retransmission. Il ne fournit pas non plus une programmation musicale contractuelle. Avant toute exploitation commerciale, il faut obtenir les autorisations applicables dans les territoires visés et remplacer l’adaptateur par un fournisseur contractuel si des chaînes garanties sont requises.

Autres pistes étudiées :

- [Live365](https://www.live365.com/broadcaster/radio-broadcasting) fournit des outils de gestion et des offres avec licences musicales annoncées pour les États-Unis, le Canada et le Mexique, ainsi que des options de distribution et d’application personnalisée. C’est une solution de diffusion pour des stations exploitées par le client, pas un catalogue public générique de chaînes licenciées pour Tunetalk ; sa couverture annoncée ne règle pas les droits en France/UE.
- [Radio.co](https://www.radio.co/) sert à créer, programmer et gérer des stations, avec des lecteurs et applications de marque. C’est également une plateforme d’exploitation de station, pas un flux générique de chaînes musicales prêtes à intégrer.

L’interface `IRadioChannelCatalog` isole cette décision. Le remplacement du fournisseur ne change ni les écrans Android, ni l’API de chat.

## Démarrage local (PowerShell)

Prérequis : .NET 10 SDK. Depuis ce dossier :

```powershell
$random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 64
$random.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
$random.Dispose()
dotnet user-secrets set --project .\TunetalkApi.Api\TunetalkApi.Api.csproj "Jwt:SigningKey" $key
dotnet run --project .\TunetalkApi.Api\TunetalkApi.Api.csproj --launch-profile http
```

L’API écoute sur `http://localhost:5269`. Le client Android Emulator utilise `http://10.0.2.2:5269/`. Pour un appareil physique, lancez l’API sur l’interface LAN (`dotnet run --project .\TunetalkApi.Api\TunetalkApi.Api.csproj --urls http://0.0.0.0:5269`), ouvrez le port dans le pare-feu local et configurez `TUNETALK_API_BASE_URL` avec l’adresse LAN de la machine. L’environnement Development accepte HTTP pour faciliter le test local ; le trafic public en Production doit être protégé par HTTPS.

La base SQLite `tunetalk.db` est créée dans le répertoire de lancement et les migrations sont appliquées au démarrage. Les clés JWT et les chaînes de connexion de production doivent être fournies par le gestionnaire de secrets de l’hébergeur, jamais par `appsettings.json`.

## Endpoints

| Méthode | Endpoint | Accès | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Créer un compte et recevoir un JWT |
| POST | `/api/auth/login` | Public | Ouvrir une session |
| GET | `/api/channels/genres` | Public | Lister les genres autorisés |
| GET | `/api/channels?genre=rock` | Public | Trouver des stations par genre |
| GET | `/api/channels/{stationUuid}/messages` | JWT | Charger les 50 derniers messages |
| SignalR | `/hubs/chat` | JWT | Rejoindre/quitter un salon et envoyer/recevoir les messages |
| GET | `/health` | Public | Sonde de santé |

Les inscriptions imposent un pseudo de 2 à 40 caractères et un mot de passe de 10 caractères minimum avec au moins un chiffre. Les endpoints d’authentification sont limités à 10 requêtes par minute et par IP ; le chat à 5 messages par compte sur 10 secondes. Les mots de passe sont gérés par ASP.NET Identity ; les erreurs de connexion restent génériques.

## Structure

- `TunetalkApi.Domain` : règles et entités métier.
- `TunetalkApi.Application` : contrats, cas d’usage et modèles d’échange.
- `TunetalkApi.Infrastructure` : Identity, SQLite/EF Core, adaptateur Radio Browser.
- `TunetalkApi.Api` : endpoints REST, JWT, hub SignalR et composition de l’application.
- `TunetalkApi.Tests` : tests unitaires des règles métier, genres et présence.

Voir [`docs/architecture.md`](docs/architecture.md) et [`docs/adr/0001-radio-catalog.md`](docs/adr/0001-radio-catalog.md).

## Vérification

```powershell
dotnet test .\TunetalkApi.Api.slnx --configuration Release
dotnet publish .\TunetalkApi.Api\TunetalkApi.Api.csproj --configuration Release
```

## Conteneur

```powershell
$random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 64
$random.GetBytes($bytes)
$env:TUNETALK_JWT_SIGNING_KEY = [Convert]::ToBase64String($bytes)
$random.Dispose()
docker compose up --build
```

L’image écoute sur le port 8080, tourne sans privilèges et conserve SQLite dans un volume Docker. Compose ne publie le port que sur `127.0.0.1` : placez un reverse proxy HTTPS devant le conteneur avant toute exposition réseau. Le workflow `api-publish.yml` publie l’image dans GHCR sur un tag `api-v*`. Le déploiement sur un hébergeur final est volontairement séparé : il dépend du fournisseur choisi, de son stockage persistant et de la configuration TLS/secrets.

## Limites à lever avant production

- La présence SignalR est conservée en mémoire dans une seule instance. Pour plusieurs instances, configurer Azure SignalR ou un backplane Redis et un suivi de présence distribué.
- SQLite convient au prototype et aux faibles volumes. Prévoir PostgreSQL ou SQL Server, sauvegardes et supervision avant une montée en charge.
- Ajouter une stratégie de renouvellement/révocation des JWT avant de transformer l’accès de 60 minutes en session longue.
- La disponibilité, la qualité éditoriale et les droits des flux tiers doivent être contrôlés par produit/juridique.
