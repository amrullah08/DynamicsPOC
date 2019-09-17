#[void][System.Reflection.Assembly]::LoadFile("D:\PS to save entity\microsoft.xrm.sdk.dll")

#[void][System.Reflection.Assembly]::LoadFile("D:\PS to save entity\microsoft.crm.sdk.proxy.dll")



                            Param(
                                [string] $crmServiceUrl,                            
                                [string]  $UserName,
                                [string]  $Password,
                                [string]  $Status,
                                [string]  $resultFileUrls,
                                [string]  $EntityRecordId
                               
                                )
Write-Output $crmServiceUrl
Write-Output $UserName
Write-Output $Password
Write-Output $Status
Write-Output $resultFileUrls 
Write-Output $EntityRecordId

if(-Not (Get-Module -ListAvailable -Name Xrm.Framework.CI.PowerShell.Cmdlets))
{
Import-Module $env:DOWNLOADSECUREFILE1_SECUREFILEPATH
Import-Module $env:DOWNLOADSECUREFILE2_SECUREFILEPATH
}
[void][System.Reflection.Assembly]::LoadWithPartialName("system.servicemodel")

$clientCredentials = new-object System.ServiceModel.Description.ClientCredentials

$clientCredentials.UserName.UserName =  $UserName

$clientCredentials.UserName.Password = $Password

$service = new-object Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy($crmServiceUrl, $null, $clientCredentials, $null)

$service.Timeout = new-object System.Timespan(0, 10, 0);

$request = new-object Microsoft.Crm.Sdk.Messages.WhoAmIRequest

$service.Execute($request)

$query = new-object Microsoft.Xrm.Sdk.Query.QueryExpression("syed_sourcecontrolqueue")

$query.ColumnSet = new-object Microsoft.Xrm.Sdk.Query.ColumnSet($true)

# RetrieveMultiple returns a maximum of 5000 records by default. 

# If you need more, use the response's PagingCookie.

$response = $service.RetrieveMultiple($query)

Write-Output  $response  

## need to chane the status for record build com0-lete , release compoleted

    $entity = New-Object Microsoft.Xrm.Sdk.Entity("syed_sourcecontrolqueue")

    $entity.Id = $EntityRecordId; 

    $entity.Attributes["syed_status"] =$Status;
    
    $entity.Attributes["syed_solutionchecker"] =$resultFileUrls;

    $service.Update($entity)