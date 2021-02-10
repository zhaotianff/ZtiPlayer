#register lib
regsvr32 /s .\APlayer.dll
regsvr32 /s .\APlayerUI.dll

#check network connection

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$DecodePackUrl = "http://aplayer.open.xunlei.com/codecs.zip"
$TempPath = $env:TEMP
$CodecsFileName = "codecs.zip"
$FullPath = "$TempPath\$CodecsFileName"

$CodecsExtractName = "codecs"
$CodecsExtractPath = "$TempPath\$CodecsExtractName"

$thumderPath = [System.Environment]::GetFolderPath('CommonDesktopDirectory').Replace("Desktop","Thunder Network") + "\APlayer\codecs"

$testIP = [System.Net.Dns]::GetHostEntry("aplayer.open.xunlei.com").AddressList[0].ToString();
$pingResult = Test-NetConnection $testIP
if($pingResult.PingSucceeded -eq $true)
{
    $client = New-Object System.Net.WebClient
    "Downloading decode pack,please wait......."

    $existDecodePack = Test-Path $FullPath
    if($existDecodePack -eq $false)
    {
        $client.DownloadFile($DecodePackUrl,$FullPath)
    }
    "Extracting decode pack......"

    $existExtractDir = Test-Path $CodecsExtractPath
    if($existExtractDir -eq $false)
    {
        md $CodecsExtractPath
    }


    #temp
    $count = (ls $CodecsExtractPath).Count

    if($count -eq 0)
    {
        [System.IO.Compression.ZipFile]::ExtractToDirectory($FullPath, $CodecsExtractPath);
        "Extract codec file success."
    }

    #copy file 
    $existThumderDir = Test-Path $thumderPath
    if($existThumderDir -eq $false)
    {
        md $thumderPath
    }

    copy -Path ($CodecsExtractPath + "\*") -Destination $thumderPath -Recurse -Force

    "Register success,press any key to exit"
    Read-Host
}
else
{
    [System.Windows.Forms]::MessageBox.Show("Check you network connection")
    retrun;
}
