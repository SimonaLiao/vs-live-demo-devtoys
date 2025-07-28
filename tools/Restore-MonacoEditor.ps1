# Run this script using PowerShell to download and 
# install dependencies into the project directory before building.

# Reference to Monaco Version to Use in the Package
$monaco_version = "0.20.0"

# ------------------------
$monaco_tgz_url = "https://registry.npmjs.org/monaco-editor/-/monaco-editor-$monaco_version.tgz"
$temp_dir_name = ".temp"

function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}

$script_dir = Get-ScriptDirectory

Push-Location $script_dir

function Extract-TGZ {
    Param([string]$gzArchiveName, [string] $destFolder)
    
    $tarFileName = $gzArchiveName -replace '\.tgz$', '.tar'
    
    # Decompress gzip to tar
    $gzipStream = New-Object System.IO.FileStream($gzArchiveName, [System.IO.FileMode]::Open)
    $gzipDecompressor = New-Object System.IO.Compression.GzipStream($gzipStream, [System.IO.Compression.CompressionMode]::Decompress)
    $tarStream = New-Object System.IO.FileStream($tarFileName, [System.IO.FileMode]::Create)
    
    $gzipDecompressor.CopyTo($tarStream)
    
    $tarStream.Close()
    $gzipDecompressor.Close()
    $gzipStream.Close()
    
    # Create destination folder if it doesn't exist
    if (-not (Test-Path $destFolder)) {
        New-Item -Path $destFolder -ItemType Directory -Force | Out-Null
    }
    
    # Extract tar using external tar command if available, otherwise use workaround
    if (Get-Command tar -ErrorAction SilentlyContinue) {
        tar -xf $tarFileName -C $destFolder
    } else {
        # Fallback: rename to .zip and try to extract (works for some tar files)
        $zipFileName = $tarFileName -replace '\.tar$', '.zip'
        Rename-Item $tarFileName $zipFileName
        try {
            Expand-Archive -Path $zipFileName -DestinationPath $destFolder -Force
        } catch {
            Write-Warning "Could not extract using Expand-Archive. Please install tar command or extract manually."
            throw
        }
    }
    
    # Clean up tar file
    if (Test-Path $tarFileName) {
        Remove-Item $tarFileName -Force
    }
}

# Remove Old Dependency
Remove-Item "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -Force -Recurse -ErrorAction SilentlyContinue

# Create Temp Directory and Output
New-Item -Name $temp_dir_name -ItemType Directory -Force | Out-Null
New-Item -Name "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -ItemType Directory -Force | Out-Null

Write-Host "Downloading Monaco"

[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
Invoke-WebRequest -Uri $monaco_tgz_url -OutFile ".\$temp_dir_name\monaco.tgz"

Write-Host "Extracting..."

Extract-TGZ "$script_dir\$temp_dir_name\monaco.tgz" "$script_dir\$temp_dir_name\monaco"

Copy-Item -Path ".\$temp_dir_name\monaco\package\*" -Destination "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -Recurse

# Clean-up Temp Dir
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

Pop-Location