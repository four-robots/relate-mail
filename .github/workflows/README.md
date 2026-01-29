# GitHub Actions Workflows

This directory contains automated workflows for CI/CD.

## docker-publish.yml

Builds and publishes Docker images to GitHub Container Registry (ghcr.io).

### Triggers

The workflow runs on:

1. **Push to main or develop branches** - Builds and publishes images
2. **Version tags** (e.g., `v1.0.0`, `v2.1.3`) - Builds and publishes with semantic versioning
3. **Pull requests to main/develop** - Builds images without publishing (for testing)
4. **Manual dispatch** - Can be triggered manually from the Actions tab

### Images Built

Four Docker images are built and published:

1. `ghcr.io/tyevco/relate-smtp-api` - REST API service
2. `ghcr.io/tyevco/relate-smtp-smtp` - SMTP server
3. `ghcr.io/tyevco/relate-smtp-pop3` - POP3 server
4. `ghcr.io/tyevco/relate-smtp-web` - Web UI

### Image Tags

Images are automatically tagged with:

- **Branch name**: `main`, `develop`
- **PR number**: `pr-123`
- **Semantic version**: `1.0.0`, `1.0`, `1` (from git tags like `v1.0.0`)
- **Git SHA**: `main-abc1234`, `develop-def5678`
- **Latest**: Only for main branch

#### Example Tags for Version v1.2.3

When you push tag `v1.2.3`, the following tags are created:

```
ghcr.io/tyevco/relate-smtp-api:v1.2.3
ghcr.io/tyevco/relate-smtp-api:1.2.3
ghcr.io/tyevco/relate-smtp-api:1.2
ghcr.io/tyevco/relate-smtp-api:1
ghcr.io/tyevco/relate-smtp-api:latest  (if on main branch)
```

### Platform Support

Images are built for multiple platforms:
- `linux/amd64` (x86_64)
- `linux/arm64` (ARM64, including Apple Silicon)

### Build Cache

The workflow uses GitHub Actions cache to speed up builds:
- Docker layer caching is enabled
- Subsequent builds are significantly faster
- Cache is automatically cleaned up

### Permissions Required

The workflow needs these permissions (automatically granted):
- `contents: read` - Read repository code
- `packages: write` - Publish to GHCR
- `id-token: write` - OIDC token for security

### Secrets Required

No manual secrets needed! The workflow uses:
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions

### Configuration

The workflow is configured with these environment variables:

```yaml
env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ${{ github.repository }}
```

You don't need to change these unless you want to use a different registry.

## How to Use

### Publishing a New Version

1. **Create a version tag**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Wait for the workflow** to complete (check Actions tab)

3. **Images are published** to GHCR with version tags

### Deploying Published Images

Use the published images in your deployment:

```bash
# Using docker-compose
docker compose -f docker/docker-compose.ghcr.yml up -d

# Using docker run
docker run -d ghcr.io/tyevco/relate-smtp-api:v1.0.0

# Using Kubernetes
kubectl set image deployment/api api=ghcr.io/tyevco/relate-smtp-api:v1.0.0
```

### Pulling Images Locally

For public repositories:
```bash
docker pull ghcr.io/tyevco/relate-smtp-api:latest
```

For private repositories:
```bash
# Login first
echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_USERNAME --password-stdin

# Then pull
docker pull ghcr.io/tyevco/relate-smtp-api:latest
```

### Creating a Personal Access Token

If you need to pull images from a private repository:

1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate new token (classic)
3. Select scopes:
   - `read:packages` - Download packages from GHCR
   - `write:packages` - Upload packages to GHCR (if needed)
4. Copy the token and use it to login:
   ```bash
   echo $TOKEN | docker login ghcr.io -u YOUR_USERNAME --password-stdin
   ```

## Viewing Published Images

Visit your repository's packages page:
```
https://github.com/tyevco/relate-smtp/pkgs/container/relate-smtp-api
```

Or search on GitHub Container Registry:
```
https://github.com/orgs/tyevco/packages
```

## Customizing the Workflow

### Change Build Platforms

Edit the `platforms` field in `docker-publish.yml`:

```yaml
platforms: linux/amd64,linux/arm64,linux/arm/v7
```

### Add Build Arguments

Add build args to the Docker build step:

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    # ... existing config ...
    build-args: |
      VERSION=${{ steps.meta.outputs.version }}
      BUILD_DATE=${{ steps.meta.outputs.created }}
```

### Change Trigger Branches

Edit the `on` section:

```yaml
on:
  push:
    branches:
      - main
      - develop
      - staging  # Add more branches
```

### Add Image Scanning

Add a security scanning step:

```yaml
- name: Run Trivy vulnerability scanner
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.name }}:${{ steps.meta.outputs.version }}
    format: 'sarif'
    output: 'trivy-results.sarif'

- name: Upload Trivy results to GitHub Security tab
  uses: github/codeql-action/upload-sarif@v2
  with:
    sarif_file: 'trivy-results.sarif'
```

## Troubleshooting

### Build Fails with "Authentication Required"

**Problem**: Workflow can't push to GHCR

**Solution**: Check that the workflow has `packages: write` permission in the job definition.

### Images Don't Show Up in GHCR

**Problem**: Images built but not visible in packages

**Solutions**:
1. Check if the repository is private (you need proper access)
2. Verify the workflow completed successfully
3. Check the workflow logs for push errors
4. Ensure `push: ${{ github.event_name != 'pull_request' }}` is working correctly

### Build is Slow

**Problem**: Builds take too long

**Solutions**:
1. Workflow uses caching - first build is slow, subsequent builds are faster
2. Check cache is enabled: `cache-from: type=gha`
3. Consider using local registry for development
4. Reduce number of platforms if you don't need multi-arch

### Tag Not Created

**Problem**: Git tag exists but image tag missing

**Solutions**:
1. Ensure tag follows semver format: `v1.0.0` not `1.0.0` or `release-1.0.0`
2. Check workflow trigger includes `tags: ['v*']`
3. Verify the tag was pushed: `git push origin v1.0.0`

## Best Practices

1. **Use semantic versioning** for tags: `v1.0.0`, `v1.2.3`, etc.
2. **Tag releases** when deploying to production
3. **Use branch names** for development: `main`, `develop-abc1234`
4. **Pin versions** in production deployments (don't use `latest`)
5. **Review workflow runs** after significant changes
6. **Keep dependencies updated** (actions, base images)
7. **Test locally** before pushing: `docker build --target api .`
8. **Document** any custom workflow changes in this README

## Security

- GitHub automatically rotates `GITHUB_TOKEN` for each workflow run
- Never commit sensitive data to workflows
- Use GitHub Secrets for sensitive environment variables
- Enable branch protection for main branch
- Review workflow changes in pull requests
- Keep base images updated for security patches

## Further Reading

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Container Registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [Docker Metadata Action](https://github.com/docker/metadata-action)
