# Local debugging

Follow the following steps to be able to debug the web app and functions locally. Local debug connects to azure resources like all config and iot hub so you need to grant some roles to your user and set that user to be the login to azure when debugging locally. 

## Add user roles

This can ether be done in each resource, or set in the subscription or resource group as the resources will inherit the role assignments. I've set them on the subscription level as I use these with other projects as well. For better security, follow the principle of least access, meaning these should be set on the resources them self. 

![Roles picture](./Pictures/Azure%20user%20roles.png)


## Setting up visual studio to use the correct user for accessing azure

In visual studio, to to `Tools -> Options -> Azure Service Authentication -> Account Selection` then make sure the user account you added to roles to are selected. for more info visit [Azure Identity client library for .NET - version 1.6.1](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme). 

You need to have azure CLI installed as this is used behind the scenes. 

![Azure service authentication in visual studio](./Pictures/Visual%20studio%20azure%20account%20selection.png)