
# KIV/PIA - Management Tool Project 2022

This is a brief description of my result for this assignment.
It walks you through how to compile the whole project and how to get it up and running. 
The entire description of the assignment can be found over at Teams.
Don't panic, the UI is in czech language because it was specified in the assignment. 
Unfortunately I didnt make any localizations. 
Although this assignment is long overdue I hope it is good enough to get needed points.


### Github

Full repository can be found on address https://github.com/Pultak/ManagementTool.PIA


## System + Architecture

System was written in `C#` language with <b>Blazor</b> web framework and <b>ASP.NET</b> for development of Web API. 
Namely the current system frontent is using <b>Blazor WebAssembly</b>.
For the purpose of easy deployment all the system components are published into  <b>docker</b> containers. 
Frontend is hosted with <b>nginx</b>.


As was mentioned earlier this system is contained of <b>Single Paged Application</b> (SPA) thin client which 
communicates and gets all data from <b>Web API</b> and a <b>Postgresql</b> database. The whole system can be then split into three layers. 
The client and API controllers are part of <b>Presentation layer</b> using only presentation models defined in `ManagementTool.Shared`. 
When the client calls one of the API endpoints, then the specified controller calls methods from services (<b>Business Logic layer</b>).
These services similiarly use only business models that are isolated from the client in `ManagementTool.Server`. 
These models are mapper by <b>AutoMapper</b>. 
Services then get data from repositories which only use <b>Data Access layer</b> models (they are also mapped by the <b>Automapper</b>).
Repositories then use <b>Entity Framework</b> queries to get data from database.

The user identity is authenticated on server-side via <b>JWT</b>. 
The client asks the API for current logged in user and if there is any, it returns needed information so that the frontent 
can show pages to the user based on his permissions.


Naming conventions of classes, interfaces and enums were taken from Microsoft standards:
 https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces

![Screenshot](img/PIA-architecture.drawio.png)

## Application setup

Required software for deployment is installed and running software: `docker`

Then all you're required to setup the application is to execute the following command in the root folder of the project structure:

```
docker-compose up --build
```

Assuming you don't have any docker images previously downloaded, it will go ahead and pull down 3 docker images 
needed for starting the appropriate containers. All the images are defined in the `docker-compose.yml` file located
in the root folder of the project. These images will be created:

- `db` my posgresql database
- `managementtool.server` image for the server part of the system
- `managementtool.client` image for the thin client

These three servers are sitting on the same subnet which was created within `Docker`.
However, I needed to expose a few ports to the local machine, so we can interact with the application
effortlessly using a web browser.

The client application can be accessed on http://127.0.0.1:5080 and server http://127.0.0.1:5180. Database runs on port :5432


### Deployment outside of docker

It is also possible to run the application outside of docker but it requires the <b>.NET 6.0 SDK</b> installed on the computer.
But if you have the prerequisites you can simply call these commands:

##### Client

`dotnet restore ManagementTool/Client/ManagementTool.Client.csproj`

`dotnet build -c Release -o /build`

`dotnet publish -c Release -o /publish`

`dotnet run ManagementTool.Client.dll`


##### Server

`dotnet restore ManagementTool/Server/ManagementTool.Server.csproj`

`dotnet build -c Release -o /build`

`dotnet publish ManagementTool/Server/ManagementTool.Server.csproj -c Release -o /publish`

`dotnet run ManagementTool.Server.dll`

Part of this project is also a <b>Visual Studio </b>solution  (`.sln`) so you can simply load it in, build it and run/debug in no time.


### Application configuration

Sometimes the change of API or database address needs to be done. 

To change the API server address go to `ManagementTool\Client\wwwroot\`.
There you can find `appsettings.json` where you need to change `ApiBaseUrl` (`appsettings.Development.json` for development configuration).


Database connection string can be changed under `ManagementTool\Server\` again in file `appsettings.json` where you can find `DBPosgreSQL`.
Needed format of the connection string can found [here](https://www.connectionstrings.com/postgresql/).
There is also `Security` section with `JWTTokenKey` and `JWTTimeoutDays` that can be changed and will affect the JWT generation.

#### Testing the application on localhost

The client application itself is running over at http://127.0.0.1:5080. 
The database server has its default port `5432` exposed to the local machine as well. 
So, it can be accessed using a database client such as `DBeaver`, for instance. 
The credentials can be found in `docker-compose.yml`

Note that when accessing server on http://127.0.0.1:5180/swagger/index.html you can access the Open API UI (<b>swagger</b>) to see and test endpoints of the API.
Although this is not a good practice in deployment I decided to leave it open for the purposes of PIA subject.
If you want to test it this way you need to get AWT token token to HTTP header. 
Firstly you need to generate one by calling `/api/auth` endpoint : 

![Swagger Login](img/swaggerLogin.png)

After you press execute button an reponse body will be returned from API.

![Swagger Login Response](img/swaggerLoginResponse.png)

Please copy the token string without quotation marks and press the `Authorize` button on the top of the page. 

![Swagger Auth](img/swaggerAuth.png)

Dialog will be showed and fill text box with value you copied earlier like this: `bearer <copied-JWT-token>`

![Swagger Auth2](img/swaggerAuth2.png)

##### Default user accounts

Here you can see list of all users with their credentials:

| Username | Password | Roles |
| :---: | :---: | :---: |
| admin | Abc12345  | All main roles
| depMan1 | Abc12345  | Departman manager
| sup1 | Abc12345  | Superior
| sup2 | Abc12345  | Superior
| secrt | Abc12345 | Secretariat


You maybe wonder why there is no project manager role. 
It is because there is no project for which a project role could be created.
If you want a project manager role follow user manual.



### User module unit tests 

The business logic of user + login module is tested with unit tests which are located inside subproject `ManagementTool.ServerTests`. 
<b>Nunit</b> and <b>Moq</b> framework were used for the purpose of testing.
 The total code coverage can be found under folder `.\TestResults\`.



## Optional functional requirements done

I've decided to pull off the following features:

- SPA + WS API architecture [+10p]
- Detailed maintenance guide [+2p] for installation and standard/regular maintenance task
- Detailed user guide [+2p] with screenshots for the main use-cases 


#### Features added of my own
- Passwords are hashed with <b>SHA256</b> standard, random salt is saved with user in database in `base64` format and is used during user login. 
- Passwords need to have atleast 8 characters containing atlest one upercase, one lowercase and one number 
- Although not explicitly specified in the Teams assignment you can delete users and delete created assignment. Projects cant be deleted.
- Every input form is validated via Data anotations and that means the user instatly gets what is wrong. 
- Configured Swagger UI for easier API debugging
- Users can change their password in any time they desire


### Issues

Known issue is in database where init data in the `db-init.sql` fills the database with needed data,
 but doesnt increment the serial counter for ids. So every time I insert a new entry an exception is thrown.
I've tried to set the start index or set it in entity framework to ignore id variable for new object but it was not working.
Temporary hotfix is to set the init data indexes high enough so it wont be triggered. Not a clean solution but i works.


# User manual


## User creation + management

User management module can be found under `Sekretariát` in left navigation bar. 
This page can be accessed only if you have secreatariat rights.

![Users Nav Bar](img/usersNavBar.png)

Upon navigating to the page you can see list of all users currently registered. 
There are two buttons you can choose of. 
One is to add to new users to the list and second one is to edit new users.
Both buttons get you to the same page but one is filled with data of the selected user and other is empty.

![User View](img/userView.png)

##### New user creation

![User Creation](img/userCreation.png)

If you fill invalid data under user creation and edit page you will be notified by following messages:

![Validation](img/validation.png)


For newly created user we have generated password showed in modal dialog.

![Pwd Generation](img/pwdGeneration.png)

If this new user logs in this page will appear. 
Upon changing of the password the user is logout and has to login again to try his new password.

![Change Pwd](img/changePwd.png)


##### User edit

![User Edit](img/userEdit.png)

You can also delete already existing user but you will be asked first.

![User Delete](img/userDelete.png)

## Project creation + management

Project management module can be found on multiple places 

![Project Nav Bar](img/projectNavBar.png)

Similalry to users view in this view you can add new projects, edit existing and assign users to project. 
Project and Department managers cant add new projects or assign people to them.

![Project View](img/projectView.png)

This page is also similar to the user creation/edit -> same page for both of those functionalities but the content is different.

![Edit Project](img/editProject.png)

You can also assign existing users to desired project. 
To multiselect users you need to hold `ctrl` and click on users you want to select.
Then when you click on `Odebrat` or `Pøidat` those users will be removed/added to the project.

![Project Assign](img/projectAssign.png)


## Assignment creation + management
Assignment related tasks can be found here in navigation bar:

![Assignment Nav Bar](img/assignmentNavBar.png)

My/project/deparment assignments view contains three important buttons. 
One of the buttons is to add new assignments.
If there is an existing assignment there will be another two buttons. 
First one is to navigate to detail mode (assignment note can be seen here) and the other one is to edit that assignment.

![Assignments View](img/assignmentsView.png)


The edit page is also similar to the other creation/edit pages -> same page for both of those functionalities but the content is different.


![Assignment Creation](img/assignmentCreation.png)

## Workload
Workload view can be accesses here in navigation bar:

![Workload Nav Bar](img/workloadNavBar.png)

To see users workload you firstly need to specify users you want to see and time scope that should be visualized.
You can select multiple users again by holding `control` and click on user names.

If the request was successful there will be a table of dates from desired time scope and rows with user workloads.
The days are split into two parts. First value is workload on that day for active assignments and the second one is for workload on all (even finished, etc.).

![Workload](img/workload.png)

