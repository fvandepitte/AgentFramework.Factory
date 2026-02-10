# Library Package Workflow Documentation

## Overview

The AgentFramework.Factory repository uses an automated GitHub Actions workflow to build, version, and publish NuGet packages. The workflow supports semantic versioning (major.minor.patch) and automatically publishes to NuGet.org when version tags are pushed.

## Workflow Features

- ✅ **Automatic semantic versioning** from git tags
- ✅ **Multi-package support** (4 NuGet packages)
- ✅ **Version validation** (enforces semantic versioning)
- ✅ **Automated builds and tests**
- ✅ **Symbol packages** (.snupkg) for debugging
- ✅ **GitHub Releases** with package attachments
- ✅ **NuGet.org publishing** with environment protection
- ✅ **Manual workflow trigger** option

## Published Packages

The workflow creates and publishes the following NuGet packages:

1. **AgentFramework.Factory** - Core library for markdown-based agent definitions
2. **AgentFramework.Factory.Provider.AzureOpenAI** - Azure OpenAI provider integration
3. **AgentFramework.Factory.Provider.OpenAI** - OpenAI provider integration
4. **AgentFramework.Factory.Provider.GitHubModels** - GitHub Models provider integration

## Release Process

### Automated Tag Creation (Recommended)

The easiest way to create a release is to use the automated tag creation workflow:

1. **Navigate to the workflow:**
   - Go to **Actions** → **Create Release Tag**
   - Click **Run workflow**

2. **Select version bump type:**
   - **Patch** (v1.0.0 → v1.0.1): Bug fixes, backward compatible
   - **Minor** (v1.0.0 → v1.1.0): New features, backward compatible
   - **Major** (v1.0.0 → v2.0.0): Breaking changes, incompatible API

3. **Optional - Specify custom version:**
   - Leave empty to auto-increment based on the latest tag
   - Or enter a specific version (e.g., `1.5.0`)

4. **Run the workflow:**
   - Click **Run workflow** button
   - The workflow will automatically create and push the tag
   - This triggers the **Library Package** workflow automatically

5. **Monitor the release:**
   - The tag creation triggers the package build and publish workflow
   - Go to **Actions** to monitor progress
   - Check **Releases** for the published release

### Manual Tag Creation

If you prefer to create tags manually via git:

1. **Determine the version bump:**
   - **Major**: Breaking changes (e.g., `v1.0.0` → `v2.0.0`)
   - **Minor**: New features, backward compatible (e.g., `v1.0.0` → `v1.1.0`)
   - **Patch**: Bug fixes, backward compatible (e.g., `v1.0.0` → `v1.0.1`)

2. **Create and push a version tag:**
   ```bash
   # Example: Releasing version 1.2.3
   git tag v1.2.3
   git push origin v1.2.3
   ```

3. **Monitor the workflow:**
   - Go to **Actions** tab in GitHub
   - Find the **Library Package** workflow run
   - Verify all steps complete successfully

4. **Verify the release:**
   - Check the **Releases** page for the new release
   - Verify packages are attached to the release
   - Confirm packages appear on NuGet.org (may take a few minutes)

### Manual Package Build (Without Publishing)

If you need to create packages without publishing:

1. Go to **Actions** → **Library Package**
2. Click **Run workflow**
3. Enter the version number (without 'v' prefix, e.g., `1.2.3`)
4. Click **Run workflow** button

**Note:** Manual builds create packages but won't automatically publish to NuGet.org or create GitHub releases. You'll need to download the artifacts and publish manually if needed.

## Workflow Steps

### 1. Build and Pack Job

- **Checkout**: Clones the repository with full history
- **Setup .NET**: Installs .NET 10.0.x SDK
- **Extract Version**: Parses and validates the version from the tag
- **Restore**: Restores NuGet dependencies
- **Build**: Compiles all projects with version metadata
- **Test**: Runs any test projects found (currently skipped if none exist)
- **Pack**: Creates NuGet packages (.nupkg) and symbol packages (.snupkg)
- **Upload**: Stores packages as workflow artifacts
- **Release**: Creates a GitHub release with package attachments

### 2. Publish to NuGet Job

- **Download**: Retrieves packages from the previous job
- **Setup .NET**: Installs .NET SDK
- **Publish**: Pushes all packages to NuGet.org

This job:
- Only runs for tag pushes (not manual triggers)
- Requires the `nuget-production` environment (manual approval gate)
- Needs the `NUGET_API_KEY` secret to be configured

## Configuration Requirements

### GitHub Secrets

The following secrets must be configured in the repository:

1. **NUGET_API_KEY**: Your NuGet.org API key
   - Go to **Settings** → **Secrets and variables** → **Actions**
   - Click **New repository secret**
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet.org API key

### GitHub Environment (Optional)

For additional safety, configure the `nuget-production` environment:

1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Name: `nuget-production`
4. Configure protection rules:
   - ✅ **Required reviewers**: Add maintainers who should approve releases
   - ✅ **Wait timer**: Optional delay before publishing
   - ✅ **Deployment branches**: Limit to tags only

If the environment doesn't exist, the workflow will still run but without approval gates.

## Version Numbering Guidelines

Follow [Semantic Versioning 2.0.0](https://semver.org/):

### MAJOR version (X.0.0)
Increment when you make incompatible API changes:
- Removing public APIs
- Changing method signatures
- Breaking configuration changes

### MINOR version (1.X.0)
Increment when you add functionality in a backward-compatible manner:
- Adding new public APIs
- Adding new providers
- New features that don't break existing code

### PATCH version (1.0.X)
Increment when you make backward-compatible bug fixes:
- Bug fixes
- Documentation updates
- Internal refactoring
- Performance improvements

## Troubleshooting

### "Invalid version format" Error

**Problem**: The workflow fails with "Invalid version format"

**Solution**: Ensure your tag follows the pattern `vX.Y.Z` where X, Y, and Z are numbers:
```bash
# ✅ Correct
git tag v1.2.3
git tag v0.1.0
git tag v10.0.15

# ❌ Incorrect
git tag 1.2.3          # Missing 'v' prefix
git tag v1.2           # Missing patch version
git tag v1.2.3-beta    # Pre-release tags not supported
```

### "NUGET_API_KEY not found" Error

**Problem**: Publishing fails because the API key secret is missing

**Solution**: Add the `NUGET_API_KEY` secret in repository settings (see Configuration Requirements above)

### Packages Not Appearing on NuGet.org

**Problem**: Workflow succeeds but packages don't appear on NuGet.org

**Possible causes:**
1. **First-time package**: New package IDs require validation by NuGet.org (can take hours)
2. **Duplicate version**: Version already published (check NuGet.org)
3. **Invalid API key**: Key expired or doesn't have publish permissions
4. **Environment approval**: Manual approval pending in `nuget-production` environment

**Solution**: Check the workflow logs for the actual error message

### Build Failures

**Problem**: Build step fails

**Solution**: 
1. Ensure all projects build locally first: `dotnet build --configuration Release`
2. Check for breaking changes in dependencies
3. Review workflow logs for specific error messages

## Local Testing

Before pushing a version tag, test the versioning locally:

```bash
# Test build with custom version
dotnet build --configuration Release /p:Version=1.2.3

# Test package creation
dotnet pack AgentFramework.Factory/AgentFramework.Factory.csproj \
  --configuration Release \
  --output ./test-packages \
  /p:PackageVersion=1.2.3 \
  /p:Version=1.2.3

# Inspect package contents
unzip -l ./test-packages/AgentFramework.Factory.1.2.3.nupkg
```

## Best Practices

1. **Test before tagging**: Always build and test locally before creating a version tag
2. **Write changelogs**: Update CHANGELOG.md before releasing
3. **Review changes**: Use `git log` to review commits since the last release
4. **Coordinate with team**: Communicate releases to avoid conflicts
5. **Start with patch versions**: Use patch versions for initial fixes, avoid jumping versions
6. **Delete failed tags**: If a release fails, delete the tag locally and remotely:
   ```bash
   git tag -d v1.2.3              # Delete locally
   git push origin :refs/tags/v1.2.3  # Delete remotely
   ```

## Workflow File Location

The workflow is defined in:
```
.github/workflows/library-package.yml
```

## Related Documentation

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Semantic Versioning Specification](https://semver.org/)
- [NuGet Package Publishing Guide](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [.NET Package Versioning](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#version)
