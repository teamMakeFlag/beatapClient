$command = "C:\Users\fazerog\git\beatapClient\Beatap\Beatap.exe"
if ($Args[0] -ne $null){
	$Args[0] = $Args[0].Replace("beatap:", "")
	$Args[0] = $Args[0].Replace("//", "")
	$params = $Args[0].Split("/")
	for ($i = 0; $i -lt $params.Length; $i++){
		$command += " "+$params[$i]
	}
}
$command
Invoke-Expression $command