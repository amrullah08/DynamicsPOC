
-   All the release configurations, setups are in this folder
    > [https://github.com/amrullah08/DynamicsPOC/tree/master/CRM%20Solution%20Manager/Release](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Fgithub.com%2Famrullah08%2FDynamicsPOC%2Ftree%2Fmaster%2FCRM%2520Solution%2520Manager%2FRelease&data=02%7C01%7CSyed.Amrullah%40microsoft.com%7C8018936436164527da3c08d74e0d971d%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C637063692589576050&sdata=2nITM6y572S6UcBHs5lZ9Ir9PRpPcS8bgpzH9qw%2FUKc%3D&reserved=0) 

-   Create a Azure AAD App , go to app registration ,new 

    -   In the API Permission 

        -   Go to apis my organization uses 

        -   Select Powerapps-Advisor 

        -   Select Dynamics CRM

        -   Select Application Permissoins, add Analysis.All 

        -   Click on Add permission, select azure keyvault select
            > user\_impersonation 

>  

-   Create a PAT token, in the DevOps go to extreme right,select your
    > user , click security, click PAT token, and select Code Only 

-   Once all is done , ensure that you have DynamicsPOC\\CRM Solution
    > Manager\\Release\\WebJob\\AzureKeyVaultNames.xlsx the details
    > menitioned in the excel. 

-   In the keyvault go to access policies, add access policy secret
    > management , select get and list, in principal select your above
    > create AAD app 
    > click on Add button

-   Create a DD365 branch 

-   Upload the folder DD365 into the tools folder or any required folder
    > in the branch 

-   In DD365 folder there are checkin and release folder which will be
    > used by the tool to trigger build and release. 

-   These folders will be modified by the tool 

-   Commit the branch and push the branch 

-   Configure Azure keyVault with the details as mentioned in the excel (path : E:\1\DynamicsPOC-master\CRM Solution Manager\Release\WebJob\AzureKeyVaultNames.xlsx)

-   Create Service connection 

    -   Go to project settings 

    -   Click on service connections 

    -   New service connection 

    -   Select azure resource manager 

    -   Select all build info 

-   Configure Build and Release Pipeline 

    -   Open folder DynamicsPOC\\CRM Solution Manager\\Release\\Build
        > and Release template 
    -   Only for techincal reosurce (Find dd365 in build pipelines replace with relative location of folder with your repository dd365 folder like Main/DD365)
    -   Import both build and release json 

    -   Click on build, click on new, click on import and select the
        > build json 

    -   After importing build json 

    -   Give build pipeline name and assign agent pool and agent
        > specification 

    -   Click on get source , configure the branch 

    -   Then 

    -   Click on variables, variables group 

    -   Remove existing varialbes group 

    -   Click on manage variables group 

    -   Click on new variable group 

    -   Give variable group name 

    -   Select link secrets 

    -   Select Azure Subscription 

    -   Select the keyvault name 

    -   And click on add, add all keyvault variables as mentioned in the
        > DynamicsPOC\\CRM Solution
        > Manager\\Release\\WebJob\\AzureKeyVaultNames.xlsx 

    -   After this come back to the build definition file cllck on
        > variables group, link variables group, select the newly
        > created variables group. 

    -   Modify build definition paths from here on 

        -   Modify copy .txt from Main folder, click on sourcefolder
            > redirect to dd365/release 

        -   And do the similar thing for rest of the task replace DD365
            > with the path that is there in the repo 
            
            -   And in copy zip file powershell task , we need to give the zip files location in source folder field based on project repository changes.
            
        -   And in trigger tab in build definition need to change the trigger files path specification
          ex: DD365/Release/solutions.txt 
              DD365/Release/trigger.txt         
             -   validate all the argument of all powershell tasks with respect to the powershell script before saving pipelines.

    -   Modify copy zip files to from release source folder to the path
        > where you want managed and unmanaged solution to be deployed 

>  
>
>  

-   Release configuration  

-   Import  

-   Remove existing artifacts  

-   Link to build pipeline 

-   Click on tasks 

-   For each of the task 

-   Modify script path and parameters  that have \"\_DD365 Daily\" with
    > the drop folder location 

-   Configure Customization 

-   Import the solution to CRM Instance

-   Open excel values from \\DynamicsPOC\\CRM Solution
    > Manager\\Release\\CRM Configuration settings 

-   Replace DD365 with the relative url where you have pasted the
    > folder 

-   Changed repository url 

-   Finally import Configuration Setting to CRM

-   Create Application User with Azure App Client Id and Assign System
    > Administrator role to the User.

-   Open DynamicsPOC\\CRM Solution
    > Manager\\Release\\WebJob\\release.zip 

-   Go to Azure portal, open webjobs , click add webjob and import the
    > zip file 

-   Triggered and manaul type of webjob 

-   Click on properties of webjob copy webhookurl, usrename and
    > password 

-   After this go to solutions open d2365 solutoin 

-   Click on each of the flow configure keyvault access 

-   Connect using the client id and secret that you had configured 

-   Scheduled dd365 and Trigger webjob flow with keyvault and webjob
    > url 

-   Connect each of the flow with service account 

-   In trigger webjob update the Web Job Url action 

-   In the http request 

-   Enter user name and password of webjob 

-   Configure environment in the trigger action 

Implement this 

[https://docs.microsoft.com/en-us/azure/devops/pipelines/integrations/microsoft-teams?view=azure-devops](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Fdocs.microsoft.com%2Fen-us%2Fazure%2Fdevops%2Fpipelines%2Fintegrations%2Fmicrosoft-teams%3Fview%3Dazure-devops&data=02%7C01%7CSyed.Amrullah%40microsoft.com%7C8018936436164527da3c08d74e0d971d%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C637063692589586044&sdata=mGnc9NpVvazjkMmhHYgyBvWjU%2Fl2o4OSTkh49QCUuKw%3D&reserved=0)
