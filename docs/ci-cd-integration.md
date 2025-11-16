# CI/CD Integration

Kampose integrates seamlessly into automated build and deployment pipelines, enabling documentation generation as part of your continuous integration workflow. This guide provides examples for popular CI/CD platforms.

## Project Assumptions

The examples in this guide assume a standard .NET project structure:

- Solution file in the repository root
- Project source code in `src/` directory
- Build output in `bin/Release/` directories
- XML documentation files generated alongside assemblies

### Configuration File

All examples use this `kampose.json` configuration file in the repository root:

```json
{
  "convention": "dotNet",
  "outputDirectory": ".doc-site",
  "assemblies": ["src/**/bin/Release/**/*.dll"],
  "theme": "classic",
  "audit": {
    "options": ["recommended"],
    "stopOnIssues": true
  }
}
```

This configuration:
- Uses the `dotNet` convention for .NET-style documentation structure
- Outputs generated documentation to the `.doc-site` directory
- Collects all assemblies and XML documentation files from Release build outputs
- Uses the `classic` theme for documentation styling
- Enables recommended audit options with quality gates (`stopOnIssues: true`) to fail builds if documentation issues are detected

## GitHub Actions

### Basic Workflow

Generate documentation on each push to the `main` branch and deploy to GitHub Pages:

```yaml
name: build

on:
  workflow_dispatch:  # Allow manual trigger
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-test-document:
    runs-on: ubuntu-latest

    permissions:
      contents: write  # Required for GitHub Pages deployment

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET 8.0
      uses: actions/setup-dotnet@v4

    - name: Restore Dependencies
      run: dotnet restore

    - name: Build Solution
      run: dotnet build --no-restore -c Release

    - name: Test Solution
      run: dotnet test --no-restore --no-build -c Release --verbosity minimal

    - name: Install Kampose for Documentation Generation
      run: dotnet tool install --global kampose

    - name: Generate Documentation
      run: kampose build

    # Only deploy to GitHub Pages on main branch (not on PRs)
    - name: Deploy Documentation to GitHub Pages
      if: github.ref == 'refs/heads/main' && github.event_name != 'pull_request'
      uses: peaceiris/actions-gh-pages@v4
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./.doc-site
```

## Azure Pipelines

### Basic Pipeline

Generate documentation on each push to the `main` branch and publish as a build artifact:

```yaml
# Trigger on pushes to main branch
trigger:
  branches:
    include:
      - main

# Trigger on pull requests to main branch
pr:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    displayName: 'Install .NET SDK'
    inputs:
      version: '8.0.x'

  - script: dotnet restore
    displayName: 'Restore Dependencies'

  - script: dotnet build --no-restore -c Release
    displayName: 'Build Solution'

  - script: dotnet test --no-restore --no-build -c Release --verbosity minimal
    displayName: 'Test Solution'

  - script: dotnet tool install --global kampose
    displayName: 'Install Kampose for Documentation Generation'

  - script: kampose build
    displayName: 'Generate Documentation'

  # Only publish artifact on main branch (not on PRs)
  - task: PublishBuildArtifacts@1
    displayName: 'Publish Documentation Artifact'
    condition: eq(variables['Build.SourceBranch'], 'refs/heads/main')
    inputs:
      pathToPublish: '.doc-site'
      artifactName: 'documentation'
```

### Pipeline with Wiki Publishing

Publish documentation directly to Azure DevOps Wiki. Update your `kampose.json` to use the `devOps` convention and wiki-compatible output directory:

```json
{
  "convention": "devOps",
  "outputDirectory": ".doc-site",
  "assemblies": ["src/**/bin/Release/**/*.dll"],
  "theme": "classic",
  "audit": {
    "options": ["recommended"],
    "stopOnIssues": true
  }
}
```

The `devOps` convention generates documentation optimized for Azure DevOps Wiki integration. See the [Configuration Guide](configuration.md#convention) for more details about conventions.

```yaml
# Trigger on pushes to main branch
trigger:
  branches:
    include:
      - main

# Trigger on pull requests to main branch
pr:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    displayName: 'Install .NET SDK'
    inputs:
      version: '8.0.x'

  - script: dotnet restore
    displayName: 'Restore Dependencies'

  - script: dotnet build --no-restore -c Release
    displayName: 'Build Solution'

  - script: dotnet test --no-restore --no-build -c Release --verbosity minimal
    displayName: 'Test Solution'

  - script: dotnet tool install --global kampose
    displayName: 'Install Kampose for Documentation Generation'

  - script: kampose build
    displayName: 'Generate Documentation'

  # Only publish to wiki on main branch (not on PRs)
  - script: |
      git clone https://$(System.AccessToken)@dev.azure.com/yourorg/yourproject/_git/yourproject.wiki wiki
      cp -r .doc-site/* wiki/
      cd wiki
      git config user.email "pipeline@azuredevops.com"
      git config user.name "Azure Pipeline"
      git add .
      git commit -m "Update documentation [skip ci]"
      git push origin HEAD:wikiMaster
    displayName: 'Publish Documentation to Wiki'
    condition: eq(variables['Build.SourceBranch'], 'refs/heads/main')
```

## GitLab CI/CD

### Basic Pipeline

Generate documentation on each push to the `main` branch and deploy to GitLab Pages:

```yaml
# Run pipeline on merge requests and main branch commits
workflow:
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
    - if: $CI_COMMIT_BRANCH == "main"

stages:
  - build
  - test
  - documentation
  - deploy

build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet restore
    - dotnet build --no-restore -c Release
  artifacts:
    paths:
      - "src/**/bin/Release/"
    expire_in: 1 hour

test:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:8.0
  needs:
    - build
  script:
    - dotnet test --no-restore --no-build -c Release --verbosity minimal

documentation:
  stage: documentation
  image: mcr.microsoft.com/dotnet/sdk:8.0
  needs:
    - build
  script:
    - dotnet tool install --global kampose
    - export PATH="$PATH:$HOME/.dotnet/tools"
    - kampose build
  artifacts:
    paths:
      - .doc-site/
    expire_in: 1 day

# Only deploy to GitLab Pages on main branch (not on merge requests)
pages:
  stage: deploy
  needs:
    - documentation
  script:
    - mkdir -p public
    - cp -r .doc-site/* public/
  artifacts:
    paths:
      - public
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
```
