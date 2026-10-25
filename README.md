# Assitech (non compilé : à tester)
## API (Api/)
1. Installer .NET 8 et PostgreSQL ; renseigner `appsettings.json` (chaîne de connexion, clé JWT aléatoire).
2. `cd Api && dotnet run --urls http://0.0.0.0:5000`
## Application (App/)
1. Installer la charge de travail MAUI : `dotnet workload install maui`
2. Ajuster `Api.Base` (10.0.2.2 = hôte vu depuis l'émulateur Android ; HTTP en clair à autoriser en dev).
3. `cd App && dotnet build -t:Run -f net8.0-android`
## Restant à faire
Inscription + code e-mail, profil, mot de passe oublié, photos, notifications push (FCM), Google/Apple, écrans CGU, mode sombre, back-office expert, chiffrement/RGPD.
