#[void][System.Reflection.Assembly]::LoadFile("D:\PS to save entity\microsoft.xrm.sdk.dll")

#[void][System.Reflection.Assembly]::LoadFile("D:\PS to save entity\microsoft.crm.sdk.proxy.dll")

                           Param(
                                [string] $CRMSourceInstanceUrl,
                                [string]  $SolutionCheckerAppClientId,
                                [string]  $ClientApplicationSecret,
                                [string]  $ADAuthorityURL,                               
                                [string]  $CRMSourceServiceUrl,
                                [string]  $CRMSourceUrlwithSDKVersion,
                                [string]  $Status,
                                [string]  $dllPath,
                                [string]  $resultFileUrls,
                                [string]  $buildurl,
                                [string]  $releaseurl,
                                [string]  $EntityRecordId
                               
                                )
                                
$ApplicationSecret = ConvertTo-SecureString -String $ClientApplicationSecret -AsPlainText -Force

Write-Output "CRMSourceInstanceUrl :" $CRMSourceInstanceUrl
Write-Output "SolutionCheckerAppClientId :" $SolutionCheckerAppClientId
Write-Output "ClientApplicationSecret :" $ApplicationSecret
Write-Output "ADAuthorityURL :" $ADAuthorityURL
Write-Output "CRMSourceServiceUrl :" $CRMSourceServiceUrl
Write-Output "CRMSourceUrlwithSDKVersion :" $CRMSourceUrlwithSDKVersion

Write-Output "Status :"$Status
Write-Output "resultFileUrls :"$resultFileUrls 
Write-Output "buildurl :"$buildurl
Write-Output "releaseurl :"$releaseurl
Write-Output "EntityRecordId :"$EntityRecordId


 
$path1=Join-Path -Path $dllPath -ChildPath Microsoft.Xrm.Sdk.dll
$path2=Join-Path -Path $dllPath -ChildPath Microsoft.Crm.Sdk.Proxy.dll
$path3=Join-Path -Path $dllPath -ChildPath Microsoft.IdentityModel.Clients.ActiveDirectory.dll

Write-Output $path1
Write-Output $path2
Write-Output $path3

[void][System.Reflection.Assembly]::LoadFile($path1)
[void][System.Reflection.Assembly]::LoadFile($path2)
[void][System.Reflection.Assembly]::LoadFile($path3)
[void][System.Reflection.Assembly]::LoadWithPartialName("system.servicemodel")



         Write-Output "Azure Service principal Mode."

       #$CRMSourceUrlwithSDKVersion='https://igdcicd2.api.crm8.dynamics.com/XRMServices/2011/Organization.svc/web?SdkClientVersion=8.2'

        #$ADAuthorityURL = "https://login.microsoftonline.com/d9a1b506-a006-4359-966b-696cb2dad64d"   

        #$resourceURL = "https://igdcicd2.crm8.dynamics.com" # crmServiceUrl

        #$CRMSourceAppclientId = "6d12e9fd-d509-4a1d-babf-40f344202c2b";

        #$clientSecret = "2YtQ=.R9kGf3yZk1xF.U=:=Fe[4:@vil";

        Write-Host "Retrieving the AAD Credentials...";

        $credential = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential($SolutionCheckerAppClientId, $ApplicationSecret);

        $authenticationContext = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext($ADAuthorityURL);

        $authenticationResult = $authenticationContext.AcquireTokenAsync($CRMSourceInstanceUrl, $credential).Result;                    

        $ResultAAD = $authenticationResult.AccessToken;

        Write-Host "AccessToken is .....";

        write-output $ResultAAD

        $Timeoutvalue=new-object System.Timespan(1, 30, 0)

        $service=New-Object Microsoft.Xrm.Sdk.WebServiceClient.OrganizationWebProxyClient($CRMSourceUrlwithSDKVersion,$Timeoutvalue,$false);

        $service.HeaderToken= $ResultAAD;
       

$query = new-object Microsoft.Xrm.Sdk.Query.QueryExpression("syed_sourcecontrolqueue")

$query.ColumnSet = new-object Microsoft.Xrm.Sdk.Query.ColumnSet($true)

# RetrieveMultiple returns a maximum of 5000 records by default. 

# If you need more, use the response's PagingCookie.

$response = $service.RetrieveMultiple($query)

Write-Output  $response 

## need to chane the status for record build com0-lete , release compoleted

    $entity = New-Object Microsoft.Xrm.Sdk.Entity("syed_sourcecontrolqueue")

    $entity.Id = $EntityRecordId;

    if (-not([string]::IsNullOrEmpty($Status)))
      {
      $entity.Attributes["syed_status"] =$Status;
     }
    
    if (-not([string]::IsNullOrEmpty($resultFileUrls)))
      {
      $entity.Attributes["syed_solutionchecker"] =$resultFileUrls;
     }
    if (-not([string]::IsNullOrEmpty($buildurl)))
      {
      $entity.Attributes["syed_devopsbuildurl"] =$buildurl;
     }
    if (-not([string]::IsNullOrEmpty($releaseurl)))
      {
      $entity.Attributes["syed_devopsreleaseurl"] =$releaseurl;
     }
     
    #Write-Output ('Updating "{0}" (Id = {1})...' -f $_.name, $entity.Id)

    $service.Update($entity)