<#
.Synopsis
   Automates the TLS Enablement of RabbitMQ.
.DESCRIPTION
   This script will use provided certs to create the TLS configuration for the RabbitMQ. Unless provided will generate the selfsigned certficate to do so.This script uses OpenSSL to generate certficates and test SSl.
   If not installed the script will download and install OpenSSL.
.PARAMETER RabbitMQver
   The version of the RabbitMQ installed at "C:\Program Files\RabbitMQ Server\" to manipulate the service.
.PARAMETER CA
   The certficate authority chain to be used.
.PARAMETER Key
   The key used to sign the certficate.
.PARAMETER Cert
   The cert that is bound to the TLS port of RabbitMQ.
.NOTES
   Purpose: Automates the TLS Enablement of Rabbit MQ
   Author: Samir Das
   Date: 28/11/2019
#>

param (
    [string]$RabbitMQver = "3.7.9",
	[string]$CA = "",
	[string]$Key = "",
	[string]$Cert = ""
 )


# if openssl is not installed download and install openssl

$OpenSSLInstallPath = "C:\Program Files\OpenSSL-Win64"
$OpenSSLDownloadPath = "c:\Win64OpenSSL_Light-1_1_1j.exe"

$RabbitMQPath = "C:\Program Files\RabbitMQ Server\rabbitmq_server-" + $RabbitMQver + "\sbin"

$OpenSSLService =  [bool] (Get-Command openssl -ErrorAction SilentlyContinue)

$OpenSSLSetupExists = Test-Path $OpenSSLDownloadPath -PathType Leaf

$NewLine = "`r`n`r`n"

if (-Not $OpenSSLService){

    if(-Not $OpenSSLSetupExists){
		Write-Host "OpenSSL is not installed. Downloading OpenSSL 64 bit.." $NewLine

		#Download
		Invoke-WebRequest https://slproweb.com/download/Win64OpenSSL_Light-1_1_1j.exe -OutFile $OpenSSLDownloadPath
	}
    

    #Install silently if not installed
	Write-Host "OpenSSL is being installed silently.." $NewLine
	Start-Process -FilePath $OpenSSLDownloadPath -Verb runAs -ArgumentList '/silent /verysilent /sp- /suppressmsgboxes' -Wait
}


    #Reload system path and set OpenSSL as system path 
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    $env:Path = $env:Path + ";" + $OpenSSLInstallPath + "\bin;" + $RabbitMQPath



#Setup Self sign config for open ssl
$SelfSignConfContent = '[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no
[req_distinguished_name]
C = VN
ST = HoChiMinh
L = HoChiMinh
O = KeppelLand
OU = Development
CN = localhost
[v3_req]
keyUsage = critical, digitalSignature, keyAgreement
extendedKeyUsage = serverAuth, clientAuth
subjectAltName = @alt_names
[alt_names]
DNS.1 = localhost
DNS.2 = 127.0.0.1
DNS.3 = ::1'

$SelfSignConfLocation = "C:\"
$SelfSignConf = "req.cnf"


#Create a self signed certficate
$CertPath = $(get-location) -replace '\\','\\'
$SelfSignedCAFile = "selfsigned.crt"
$SelfSignedKey = "selfsigned.key"
$SelfSignedCRT = "selfsigned.crt"

if($Cert -eq "") {
	
	Write-Host "Creating OpenSSL config for self signed certficate generation..." $NewLine
	$FileResponse = New-Item -Path $SelfSignConfLocation -Name $SelfSignConf -ItemType "file" -Value $SelfSignConfContent

	Write-Host $NewLine

	Write-Host "Generating self signed certs as no certs were provided..." $NewLine
	openssl req -x509 -nodes -days 730 -newkey rsa:2048 -keyout $SelfSignedKey -out $SelfSignedCRT -config $SelfSignConfLocation$SelfSignConf -sha256
}
else{
	$SelfSignedCAFile = $CA
	$SelfSignedKey =  $Key
	$SelfSignedCRT =  $Cert
}

if($SelfSignedCAFile -eq "" -or $SelfSignedKey -eq "") {
	Write-Host "Certificate CA or Key was not found or supplied. Exiting with code 111"
	exit 111
}


# Add the cert to the Current User "Trusted Cert store", will prompt the user to say "Yes" in order to add to the cert store
certutil -addstore -user -f "Root" $SelfSignedCRT

#Bind the cert to the Rabbit MQ port

$SSLRabbitConfig = 'listeners.ssl.default = 5671

## TLS configuration.
##
## Related doc guide: https://rabbitmq.com/ssl.html.
##

ssl_options.verify               = verify_none
ssl_options.fail_if_no_peer_cert = false
ssl_options.cacertfile           = ' + $CertPath + '\\' + $SelfSignedCAFile + '
ssl_options.certfile             = ' + $CertPath + '\\' + $SelfSignedCRT + '
ssl_options.keyfile              = ' + $CertPath + '\\' + $SelfSignedKey + '
'

Write-Host 'Using new rabbit config...' $NewLine + $SSLRabbitConfig
Set-Content -Path $Env:APPDATA\RabbitMQ\rabbitmq.conf -Value $SSLRabbitConfig


# Re-install Rabbit MQ service
Write-Host "Re-install RabbitMQ service for config to take effect..."
rabbitmq-service remove
rabbitmq-service install
rabbitmq-service start


#Cleanup
Write-Host "Housekeeping..."

#1 - Delete downloaded file if any

if(Test-Path $OpenSSLDownloadPath -PathType Leaf)
{
    Write-Host "- Removing installation file for OpenSSL..." $NewLine
    Remove-Item $OpenSSLDownloadPath
}

#2 - remove self sign conf file
Write-Host "- Removing self sign config file for OpenSSL..."
Remove-Item $SelfSignConfLocation$SelfSignConf

Write-Host "Waiting for RabbitMQ to be completely up.. waiting for 15 secs" $NewLine

Start-Sleep -s 15

$SSLResponse = openssl s_client -connect localhost:5671

if($SSLResponse -contains "Secure Renegotiation IS supported") {
	Write-Host  $NewLine "TLS now successfully enabled on RabbitMQ!" $NewLine
}
