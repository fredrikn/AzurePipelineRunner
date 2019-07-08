# Local Azure Pipeline Runner

This is a tool that can run a basic Azure Pipeline's Yaml file locally.

At the moment it only supports the tasks that supports Powershell 3.

### How to Start

1) You need to install latest version of node, which can be downloaded [here](https://nodejs.org/en/download/).

2) You need Typescript Compiler 2.2.0 or greater, which can be downloaded [here](https://www.npmjs.com/package/typescript).

3) Download the [azure-pipelines-tasks](https://github.com/microsoft/azure-pipelines-tasks) code.

The local azure pipeline runner will use the Tasks from the above git repo. It's the same code used in Azure Devops.

4) Go to the root folder where the azure-pipelines-tasks code is and run the following commands:

```
npm install

node make.js build
```

Note: The build can take a while! If the build breaks it may be because of a path length issue.

The build outputs will be in a folder called "_build".

5) Clone this repo, AzuirePipelineRunner and open it in Visual Studio 2019 and build the code.
   Note: A build script will soon be added to avoid using Visual Studio to build the code.

6) Open the file "appsettings.json" and change the location of where the tasks are located, for example:

```
{
  "taskFolder": "C:\\ap\\_build\\Tasks",
  "agentTmpDir": "C:\\temp",
  "systemDebug":  true
}

6) Run the code from Visual Studio to test it.
   !!Note: There are at the moment some hardcoded path structures in the code. That will be fixed later!!

### Example of using the runner:

```
AzurePipelineRunner build.yaml
```

Example Yaml file:

```
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'

steps:
- script: dotnet build --configuration $(buildConfiguration)
  displayName: 'dotnet build $(buildConfiguration)'

- powershell: Write-Host 'Hello World!'
  displayName: 'Write-Host Hello World!'

- task: BatchScript@1
  displayName: 'Run test.bat'
  inputs:
    filename: 'test.bat'
    arguments: 'Default %BUILD_DEFINITIONVERSION% %BUILD_SOURCESDIRECTORY%\ %BUILD_ARTIFACTSTAGINGDIRECTORY% sentbuildp02.lindex.local:9000'

- task: CmdLine@2
  displayName: 'echo Write your commands here.'
  inputs:
    script: 'echo Write your commands here.'

- task: PowerShell@2
  inputs:
    script: 'Write-Host Hello World again!'
    targetType: inline

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation="$(build.artifactStagingDirectory)\WebApp.zip" /p:DeployIisAppPath="Default Web Site"'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'

- task: VSTest@2
  inputs:
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'
```

