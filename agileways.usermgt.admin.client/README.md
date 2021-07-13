

## App Settings File

In the `Server` directory, you will need an `appsettings.json` file that has the necessary configuration values to connect to the B2C directory for authentication along with connecting with the B2C directory for MS Graph API calls.  The `appsettings.Development.json` file has the structure along with placeholder values. Those values are:

|Configuration Value    |Description    |
|-------------|-------------|
|AzureAdB2C.Instance    |The FQDN of your B2C instance, such as `https://myb2ctenant.b2clogin.com/` (the trailing `/` is important!)    |
|AzureAdB2C.ClientId    |The App ID of the app registration in the B2C tenant that represents the server application    |
|AzureAdB2C.Domain  |The FQDN of your B2C tenant, in the format of `myb2ctenant.onmicrosoft.com`    |
|AzureAdB2C.SignUpSignInPolicyId    |The name of the SUSI user flow or custom policy that is invoked when the user clicks the sign in button (or sign up)   |
|AzureAdB2CGraph.Instance   |The FQDN of the login endpoint you're connecting to. This should be set to `https://login.microsoftonline.com/{0}`. The placeholder is for your tenant id  |
|AzureAdB2CGraph.ApiUrl |The FQDN of the graph endpoint that your graph client will communicate with. This should be set to `https://graph.microsoft.com/`. |
|AzureAdB2CGraph.Tenant |The GUID of your B2C tenant that your graph client will be communicating with. |
|AzureAdB2CGraph.ClientId   |The App ID of the graph app registration in the B2C tenant that your graph client will use to connect to the B2C tenant. See table below for API Permissions to assign to this app registration    |
|AzureAdB2CGraph.ClientSecret   |The secret value for the Graph App ID  |


## Graph App API Permissions

For the Graph App Registration that you create in the B2C tenant, you should assign at least the following permissions to that app registration:

|API Permission |Type   |
|------------|------------|
|User.Read.All  |Application    |
|Application.Read.All   |Application    |
|Application.ReadWrite.All  |Application    |
|AppRoleAssignment.ReadWrite.All    |Application    |

## Script

```
dotnet new blazorwasm -au IndividualB2C --aad-b2c-instance "https://kubedave.b2clogin.com/" --api-client-id "df3b9765-f8a0-41ef-91d5-782dd536c084" --app-id-uri "df3b9765-f8a0-41ef-91d5-782dd536c084" --client-id "f669d15f-a651-4558-a7be-299de2e1a04e" --default-scope "API.Access" --domain "kubedave.onmicrosoft.com" -ho -o agileways.usermgt.admin.client -ssp "B2C_1_SUSI"
```