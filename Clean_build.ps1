param([Parameter(Mandatory=$true)][string]$rootDir)

Get-ChildItem -Recurse -Path $rootDir -Filter obj -Directory | ForEach-Object { Remove-Item -Force -Recurse -Path $_.FullName  }
Get-ChildItem -Recurse -Path $rootDir -Filter bin -Directory | ForEach-Object { Remove-Item -Force -Recurse -Path $_.FullName  }
#Get-ChildItem -Recurse -Path $rootDir -Filter lambda.zip | ForEach-Object { Remove-Item -Force -Recurse -Path $_.FullName  }
