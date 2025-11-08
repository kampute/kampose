# CI/CD Integration

Kampose integrates seamlessly into automated build and deployment pipelines, enabling documentation generation as part of your continuous integration workflow. This guide provides examples for popular CI/CD platforms.

## GitHub Actions

### Basic Workflow

```yaml
name: Documentation

on:
  push:
    branches: [ main ]

jobs:
  generate-docs:
    runs-on: ubuntu-latest

    permissions:
      contents: write

    steps:
      - name: Checkout Repository
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Install Kampose
        run: dotnet tool install --global kampose

      - name: Build Solution
        run: dotnet build -c Release

      - name: Generate Documentation
        run: kampose build

      - name: Deploy Documentation to GitHub Pages
        if: github.ref == 'refs/heads/main'
        uses: peaceiris/actions-gh-pages@v4
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./docs
```

### Workflow with Quality Gates

Configure auditing in your `kampose.json` to enforce documentation standards:

```json
{
  "convention": "dotNet",
  "outputDirectory": "docs",
  "assemblies": ["bin/Release/**/*.dll"],
  "theme": "classic",
  "audit": {
    "options": ["recommended"],
    "stopOnIssues": true
  }
}
```

When `stopOnIssues` is `true`, the build fails if documentation issues are detected.

For all available audit options, see the [Configuration Guide](configuration.md#audit).

## Azure Pipelines

### Basic Pipeline

```yaml
trigger:
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

  - script: dotnet tool install --global kampose
    displayName: 'Install Kampose'

  - script: dotnet build -c Release
    displayName: 'Build Solution'

  - script: kampose build
    displayName: 'Generate Documentation'

  - task: PublishBuildArtifacts@1
    displayName: 'Publish Documentation Artifact'
    inputs:
      pathToPublish: 'docs'
      artifactName: 'documentation'
```

### Pipeline with Wiki Publishing

For Azure DevOps wiki integration, use the `devOps` convention in your `kampose.json` configuration file:

```json
{
  "convention": "devOps",
  "outputDirectory": "docs",
  "assemblies": ["bin/Release/**/*.dll"],
  "theme": "classic"
}
```

See the [Configuration Guide](configuration.md#convention) for more details about conventions.

```yaml
trigger:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.0.x'

  - script: dotnet tool install --global kampose
    displayName: 'Install Kampose'

  - script: dotnet build -c Release
    displayName: 'Build Solution'

  - script: kampose build
    displayName: 'Generate Documentation'

  - script: |
      git clone https://$(System.AccessToken)@dev.azure.com/yourorg/yourproject/_git/yourproject.wiki wiki
      cp -r docs/* wiki/
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

```yaml
stages:
  - build
  - documentation

build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet build -c Release
  artifacts:
    paths:
      - bin/
    expire_in: 1 hour

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
      - docs/

pages:
  stage: documentation
  needs:
    - documentation
  script:
    - mkdir -p public
    - cp -r docs/* public/
  artifacts:
    paths:
      - public
  only:
    - main
```

> The `pages` job is a special job in GitLab CI/CD that is automatically used to deploy to GitLab Pages when a `public` directory with content is found.
