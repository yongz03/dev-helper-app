# GitHub Actions and Argo CD deployment

The workflow in `.github/workflows/ci-cd.yml` builds and tests the API and Angular client on pull requests and pushes to `main`. After a successful push to `main`, it publishes immutable, commit-SHA-tagged images to GitHub Container Registry (GHCR), updates the Kubernetes image references, and commits those references to `main`. Argo CD watches `deploy/kubernetes` and syncs each update.

## One-time setup

1. The Argo CD source and initial GHCR image references are configured for `yongz03/dev-helper-app`. Update them if the repository is renamed or transferred.
2. In GitHub, enable Actions and ensure the repository Actions setting **Workflow permissions** allows read and write access. The workflow uses `GITHUB_TOKEN` to push GHCR images and commit manifest updates; no personal access token is needed.
3. Ensure the Argo CD installation can access this GitHub repository. For a private repo, add the repository credentials to Argo CD.
4. Apply the Argo CD Application: `kubectl apply -f deploy/argocd/application.yaml`.
5. Ensure the cluster has a default StorageClass and a LoadBalancer implementation. The API stores its SQLite database on a 1 GiB PVC. The web Service gets an external address from the cluster's load balancer; add an Ingress or DNS/TLS configuration if that is how the cluster exposes services.
6. If the GHCR packages are private, configure Kubernetes image pull credentials for both deployments in the `dev-helper` namespace. Public GHCR packages need no pull secret.

After the first successful `main` deployment, both manifests point to the published commit SHA. The initial `latest` references are placeholders to be replaced by the first workflow run.

## Operational notes

- Deployments are single-environment and track `main`; use a protected branch and required pull request checks to control production changes.
- SQLite is kept on persistent storage and the API has one replica. For multiple API replicas or managed backups, move to a server database and update the API connection configuration.
- The web Service uses `LoadBalancer` so the app can be reached without assuming an Ingress controller. On clusters without a LoadBalancer implementation, change it to `ClusterIP` and configure the cluster's ingress separately.
