$ErrorActionPreference = 'Stop'

npm install
npm run build

$api = Start-Process -FilePath 'npm.cmd' -ArgumentList @('run', 'dev:api') -PassThru
$web = Start-Process -FilePath 'npm.cmd' -ArgumentList @('run', 'dev:web') -PassThru

Write-Host 'PeopleSyncD API: http://127.0.0.1:8080'
Write-Host 'PeopleSyncD Web: http://127.0.0.1:5173'
Write-Host 'Close the spawned terminal processes to stop Genesis.'

Wait-Process -Id $api.Id, $web.Id
