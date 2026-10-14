---
title: CI/CD Integration
summary: "Examples for generating and publishing Kampose documentation from GitHub Actions, Azure Pipelines, and GitLab CI/CD."
---

# CI/CD Integration

Kampose integrates seamlessly into automated build and deployment pipelines, enabling documentation generation as part of your continuous integration workflow. This guide provides examples for popular CI/CD platforms.

## Project Assumptions

The examples in this guide assume a standard .NET project structure:

- Solution file in the repository root
- Project source code in `src/` directory
- Build output in `bin/Release/` directories
- XML documentation files generated alongside assemblies
- Kampose installed as a local tool, with `.config/dotnet-tools.json` committed to the repository

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

### .NET SDK Requirements

Kampose is built with .NET 10.0 and requires the .NET 10.0 SDK to be installed. The workflow examples install .NET 10.0 for this purpose. If your project targets a different .NET version (e.g., .NET 8.0), you can install multiple SDK versions in the same pipeline.

## Tool Installation

Install Kampose as a local tool of the repository you are documenting, so the version it runs is recorded in source control rather than chosen by whichever machine happens to run the build.

Run these once in your repository root, then commit the file they produce:

```shell
dotnet new tool-manifest
dotnet tool install kampose --version 1.2.0
```

That writes `.config/dotnet-tools.json`, which records the exact version. Every pipeline example below restores that version instead of installing one:

```shell
dotnet tool restore
dotnet tool run kampose build
```

> [!IMPORTANT]
> Pinning makes pipeline behavior repeatable and prevents an untested Kampose release from changing your build. Because the pin lives in the committed manifest, your local builds and your pipeline always run the same version. Change the version in the manifest on a branch to test an upgrade before merging it.

To move to a later release, run `dotnet tool update kampose --version <new-version>` in the repository and commit the changed manifest.

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
    - uses: actions/checkout@v7

    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v6
      with:
        dotnet-version: '10.0.x'

    - name: Restore Dependencies
      run: dotnet restore

    - name: Build Solution
      run: dotnet build --no-restore -c Release

    - name: Test Solution
      run: dotnet test --no-restore --no-build -c Release --verbosity minimal

    - name: Restore Kampose for Documentation Generation
      run: |
        dotnet tool restore
        dotnet tool run kampose --version

    - name: Generate Documentation
      run: dotnet tool run kampose build

    # Only deploy to GitHub Pages on main branch (not on PRs)
    - name: Deploy Documentation to GitHub Pages
      if: github.ref == 'refs/heads/main' && github.event_name != 'pull_request'
      uses: peaceiris/actions-gh-pages@v4
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./.doc-site
        keep_files: false
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
      version: '10.0.x'

  - script: dotnet restore
    displayName: 'Restore Dependencies'

  - script: dotnet build --no-restore -c Release
    displayName: 'Build Solution'

  - script: dotnet test --no-restore --no-build -c Release --verbosity minimal
    displayName: 'Test Solution'

  - script: |
      dotnet tool restore
      dotnet tool run kampose --version
    displayName: 'Restore Kampose for Documentation Generation'

  - script: dotnet tool run kampose build
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
      version: '10.0.x'

  - script: dotnet restore
    displayName: 'Restore Dependencies'

  - script: dotnet build --no-restore -c Release
    displayName: 'Build Solution'

  - script: dotnet test --no-restore --no-build -c Release --verbosity minimal
    displayName: 'Test Solution'

  - script: |
      dotnet tool restore
      dotnet tool run kampose --version
    displayName: 'Restore Kampose for Documentation Generation'

  - script: dotnet tool run kampose build
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
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet restore
    - dotnet build --no-restore -c Release
  artifacts:
    paths:
      - "src/**/bin/Release/"
    expire_in: 1 hour

test:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:10.0
  needs:
    - build
  script:
    - dotnet test --no-restore --no-build -c Release --verbosity minimal

documentation:
  stage: documentation
  image: mcr.microsoft.com/dotnet/sdk:10.0
  needs:
    - build
  script:
    - dotnet tool restore
    - dotnet tool run kampose --version
    - dotnet tool run kampose build
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

### Pipeline with Wiki Publishing

Publish documentation directly to the project's GitLab Wiki. Update your `kampose.json` to use the `gitLab` convention:

```json
{
  "convention": "gitLab",
  "outputDirectory": ".doc-site",
  "assemblies": ["src/**/bin/Release/**/*.dll"],
  "theme": "classic",
  "audit": {
    "options": ["recommended"],
    "stopOnIssues": true
  }
}
```

The `gitLab` convention writes the home topic to `home.md`, the page GitLab Wiki serves as the front page, and the built-in Markdown theme starts each page with a front matter `title` that GitLab displays instead of the file name. See the [Configuration Guide](configuration.md#convention) for more details about conventions.

The wiki is a separate Git repository, so the job needs a token that can push to it:

1. Create a [project access token](https://docs.gitlab.com/user/project/settings/project_access_tokens/) with the Developer role and the `write_repository` scope.
2. Add the token as a masked CI/CD variable named `WIKI_ACCESS_TOKEN`.

Replace the `pages` job of the basic pipeline with the following job:

```yaml
# Only publish to wiki on main branch (not on merge requests)
wiki:
  stage: deploy
  image: mcr.microsoft.com/dotnet/sdk:10.0
  needs:
    - documentation
  script:
    - git clone "https://wiki-bot:${WIKI_ACCESS_TOKEN}@${CI_SERVER_HOST}/${CI_PROJECT_PATH}.wiki.git" wiki
    - cp -r .doc-site/* wiki/
    - cd wiki
    - git config user.email "wiki-bot@noreply.${CI_SERVER_HOST}"
    - git config user.name "Wiki Bot"
    - git add .
    - git diff --cached --quiet || git commit -m "Update documentation from ${CI_COMMIT_SHORT_SHA}"
    - git push origin HEAD
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
```

> [!NOTE]
> The wiki repository URL is the project path followed by `.wiki.git`. If your GitLab instance uses a different URL or port, copy the URL from **Wiki actions** > **Clone repository** on the wiki page. Project access tokens on GitLab.com require the Premium or Ultimate tier. On the Free tier of GitLab.com, use a personal access token with the `write_repository` scope of a user who has at least the Developer role.
