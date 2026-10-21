# ADR 0001 — Annuaire de stations pour le MVP

- **Statut :** accepté pour le prototype, décision provisoire pour la production.
- **Date :** 2026-10-21.

## Contexte

Tunetalk doit proposer des stations par genre, sélectionnées par un tiers, sans laisser les utilisateurs créer ou administrer des chaînes. Il faut éviter de construire un système de programmation musicale et d’hébergement audio.

## Décision

Utiliser Radio Browser pour découvrir des stations déjà diffusées sur Internet. Le serveur ne relaie pas l’audio ; Android lit directement les flux HTTPS. Les genres affichés restent contrôlés par Tunetalk et correspondent aux tags recherchés chez Radio Browser.

## Alternatives étudiées

- **Live365 :** outils de gestion de stations et licences annoncées dans certaines offres pour les États-Unis, le Canada et le Mexique ; option de distribution/app personnalisée. Pas un catalogue public générique prêt à intégrer et la couverture annoncée ne suffit pas pour la France/UE.
- **Radio.co :** hébergement, programmation et outils de diffusion pour les stations de l’opérateur ; pas de catalogue public de chaînes musicales que Tunetalk pourrait simplement intégrer.
- **Spotify/plateformes grand public :** leur accès de lecture/contrôle n’équivaut pas à une autorisation de retransmission synchronisée à tous les auditeurs d’une application.

## Conséquences

- Aucun compte fournisseur n’est requis pour le prototype.
- Le catalogue est communautaire et peut être indisponible ou changer ; l’API expose explicitement l’erreur 503.
- Radio Browser ne garantit ni la qualité ni les droits des flux répertoriés.
- Avant monétisation ou lancement en France/UE, obtenir des droits couvrant l’usage de Tunetalk ou contractualiser avec un fournisseur approprié. L’interface `IRadioChannelCatalog` permet d’intégrer ensuite un fournisseur licencié sans modifier le chat ou l’interface mobile.

## Références

- <https://api.radio-browser.info/>
- <https://www.live365.com/broadcaster/radio-broadcasting>
- <https://www.radio.co/>
