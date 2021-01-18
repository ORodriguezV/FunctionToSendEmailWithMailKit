# Azure Function to Send Email with MailKit

The proyect consists of an Azure function that is triggered by an http request to send an email using [MailKit client library](https://github.com/jstedfast/MailKit)

This can be useful if you want to send an email in response to a webhook setup on another system.  Most of the samples available on GitHub that send email with Azure functions use the SendGrid library.  This library and service is great if you plan to send a lot of emails.  But if you just need occasional emails, the MailKit client library should be more than enough. BTW, I think the MailKit library is awesome.

## Local Configuration

If you want to test the function locally in Visual Studio, please setup your **local.settings.json** this way:

```
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet",
		"EmailHost": "{YOUR-SMTP-HOST}",
		"EmailPort": "{YOUR-SMTP-PORT}",
		"EmailUser": "{YOUR-USER}",
		"EmailPassword": "{YOUR-PASSWORD}",
		"EmailFromEmail": "{YOUR-FROM-EMAIL}",
		"EmailFromName": "{YOUR-FROM-NAME}",
		"EmailHostUsesLocalCertificate": "false"
	}
}
```

## Modify the "EmailToSend" class to fit your needs

The function parses the http request body to populate the class called: "EmailToSend". This class has 4 properties: "To", "Subject", "PlainBody" and "HtmlBody".  Currently, the JSON to populate this class should be:

```
{
	"To": "{TO-EMAIL-ADDRESS}",
	"Subject": "{SUBJECT-OF-THE-EMAIL}",
	"PlainBody": "{BODY-OF-THE-EMAIL-IN-PLAIN-TEXT}",
	"HtmlBody": "{BODY-OF-THE-EMAIL-IN-HTML}"
}
```

However, depending on the webhook that you use, you should modify the "EmailToSend" class to adapt it to the JSON that you receive.

## Some considerations
* If the "To" email is not provided, the function will use the same email as the "From"
* When deploying the function to Azure, make sure to configure all the "Email..." parameters in the application settings
* If you require more security for your email credentials, you may choose to use the Key Vault service instead

I hope this helps! ;)
