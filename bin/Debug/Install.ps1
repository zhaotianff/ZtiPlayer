#register lib
regsvr32 /s .\APlayer.dll
regsvr32 /s .\APlayerUI.dll

#check network connection

Add-Type -AssemblyName System.Windows.Forms

$DecodePackUrl = "https://technet-info.com/ZtiPlayer/codecs.zip"
$TempPath = $env:TEMP
$CodecsFileName = "codecs.zip"
$FullPath = "$TempPath\$CodecsFileName"

$testIP = [System.Net.Dns]::GetHostEntry("technet-info.com").AddressList[0].ToString();
$pingResult = Test-NetConnection $testIP
if($pingResult.PingSucceeded -eq $true)
{
    $client = New-Object System.Net.WebClient
    "Downloading decode pack,please wait......."
    $client.DownloadFile($DecodePackUrl,$FullPath)

}
else
{
    [System.Windows.Forms]::MessageBox.Show("Check you network connection")
    retrun;
}