# Deploying to Render

## Important: the current Argo CD workflow targets Kubernetes

The deployment files in this repository are Kubernetes manifests. The GitHub Actions workflow builds the API and web Docker images, pushes them to GitHub Container Registry (GHCR), and updates `deploy/kubernetes/*.yaml`. Argo CD then applies those manifests to a Kubernetes cluster.

Render can run Docker images, but it does not act as the Kubernetes cluster targeted by these manifests. Applying `deploy/argocd/application.yaml` therefore will not deploy the app to Render. To use the existing Argo CD automation unchanged, deploy to a Kubernetes cluster that Argo CD can access. To use Render, follow the Render path below and treat Render as the runtime for the images published by GitHub Actions.

## Render deployment using the images published by GitHub Actions

### 1. Check the GitHub Actions workflow

In the GitHub repository, open **Settings → Actions → General**. Under **Workflow permissions**, allow read and write access so the workflow can publish packages and commit updated image references. The workflow uses `GITHUB_TOKEN`; it does not need a manually created token for publishing.

Push or merge an application change to `main`. In **Actions**, wait for the **CI and deploy** workflow to finish successfully. Its `publish-and-update` job publishes:

- `ghcr.io/yongz03/dev-helper-app-api:<commit-sha>`
- `ghcr.io/yongz03/dev-helper-app-web:<commit-sha>`

The commit SHA is an immutable image tag. If the GHCR packages are private, create a GitHub token with `read:packages` permission and add it in Render as a registry credential. Do not put the token in this repository or in a Dockerfile. If the packages are public, Render can pull them without a credential.

### 2. Make the web service's API address Render-compatible

The current [Nginx configuration](../client/nginx.conf) sends `/api/` requests to `http://dev-helper-api:8080`. That hostname is the Kubernetes Service name and will not resolve between Render services. Before deploying to Render, configure the web container to proxy `/api/` to the Render API's private service address, or change the frontend to use the API's public HTTPS URL. Keep the API and web services in the same Render region if using Render's private service address. Rebuild and publish the web image after changing the proxy target.

The exact Render API hostname is assigned when its service is created. For a public API URL, use the API service's `onrender.com` URL. For a private service, use its Render private hostname and port as shown in the service dashboard. Preserve `/api/` when forwarding requests because the Angular client uses that route prefix.

### 3. Create the API service

1. In the Render Dashboard, choose **New → Web Service** (or the service type you intend to use), then select **Existing Image**.
2. Enter the API image URL from the successful Actions run, including its full commit SHA tag, for example `ghcr.io/yongz03/dev-helper-app-api:<commit-sha>`.
3. If the GHCR package is private, select the registry credential created in step 1.
4. Set the service name and region. Use the same region for the web service.
5. Set the listening port to `8080` if Render asks for a port.
6. Add a persistent disk mounted at `/data`. Render requires a paid web service for a persistent disk. The API container uses SQLite at `/data/devhelper.db`; without persistent storage, database data can be lost when the service is replaced. Keep the API at one instance because a Render disk is attached to one service instance and SQLite is not configured for multi-instance use.
7. Add the environment variable `ConnectionStrings__DefaultConnection` with value `Data Source=/data/devhelper.db`.
8. Create the service and wait for its first deploy to become healthy. Copy its public URL or private hostname for the web proxy configuration in step 2.

### 4. Create the web service

1. Choose **New → Web Service → Existing Image**.
2. Enter `ghcr.io/yongz03/dev-helper-app-web:<commit-sha>` for the rebuilt web image. Select the GHCR credential if the package is private.
3. Use the same region as the API service. Set the listening port to `8080` if requested.
4. Create the service and wait for deployment to complete.
5. Open the Render URL and verify that the frontend loads and that API-backed actions work. Check the Render service logs if requests fail; confirm that `/api/` is forwarded to the API hostname and port.

### 5. Enable automatic Render updates

The `publish-and-update` job triggers Render deploy hooks after both GHCR images have been pushed. It passes each service its matching image URL with the full commit SHA tag, so Render deploys the exact images published by that workflow run. Render image-backed services do not automatically redeploy when a registry tag changes; the deploy hook starts that deploy.

For each Render service:

1. In the service's **Settings**, find **Deploy Hook** and create/copy its URL. Keep the API and web hook URLs separate; each URL is a secret that can trigger deploys for its service.
2. In the GitHub repository, open **Settings â†’ Secrets and variables â†’ Actions** and add these repository secrets:
   - `RENDER_API_DEPLOY_HOOK_URL`: the API service's deploy hook URL.
   - `RENDER_WEB_DEPLOY_HOOK_URL`: the web service's deploy hook URL.
3. Push or merge an application change to `main`. After CI passes and both images are pushed, Actions calls both hooks with their corresponding `ghcr.io/yongz03/dev-helper-app-...:<commit-sha>` image URLs. Check the Render service's **Events** or **Deploys** page for the resulting deployments.

If either secret is missing or Render rejects a hook request, the workflow fails at that deployment step. Regenerate a hook in Render and update its GitHub secret if a URL is exposed or stops working. The workflow continues to update the Kubernetes manifests for the separate Argo CD deployment path.

## If you intended to use Argo CD

Use a Kubernetes cluster as the deployment target, install Argo CD there, and follow [deploy/README.md](README.md). The cluster needs persistent storage for the API's 1 GiB SQLite PVC and a way to expose the web LoadBalancer Service. In that setup, Argo CD watches `deploy/kubernetes` and automatically syncs the image tag changes made by GitHub Actions.
