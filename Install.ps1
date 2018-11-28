#check network connection
$testIP = [System.Net.Dns]::GetHostEntry("technet-info.com").AddressList[0].ToString();
$pingResult = Test-NetConnection $testIP
if($pingResult.PingSucceeded -eq $true)
{
    "True"
}
else
{
    "False"
    retrun;
}

"HelloWorld"