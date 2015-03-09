# TFSEvolution
TFS Evolution supports scrum teams with the most important information about their current project during their daily stand-up meeting. The app is based on the integration features of Visual Studio Online like Rest API, OAuth2.0 and Service Hooks.


### Showcase – Set up

TFS Evolution is based on Visual Studio Online projects. It uses OAuth 2.0 and the new REST Api to retrieve data from a scrum based team project. This information is prepared for the two different views the app is providing to the scrum team. Some features synchronize data with VSO through service hooks (e.g. work item changed)


### Team Performance & Health State
Screen one consists of a burndown chart, a sprint progress chart and an event ticker.


### Task Board
The second screen visualizes the team tasks of one sprint as part of a task board. It’s optimized for touch and contains a fast edit mode for remaining work and the assignedTo-Field of the task work item. The remaining work can be edited with a simple touch gesture.


### Installation
To run the app, please follow these instructions:
 
How to
1.    Optionally set up a service hook for your team project in VSO to get automatic updates in the App
a.    Select “Web Hook” as target service
b.    Set “Work Item update” as event
c.     Set “https://oss-tfs.azurewebsites.net/api/event” as service URL
You don’t need to set any basic authentication settings!

2.    Install the app on your device
3.    Start the app and authenticate with your Microsoft Id and additionally with your alternative credentials
4.    Connect to your project and start working
 
Please note: 
Our web service collects and stores work item data and prepares it for our app. If you do not want your work item data to be stored in our azure storage we highly recommend you to install your own web service. The corresponding source code for deployment can be found in this repositories in the "TFSWebService" directory. 
To use your own web service you simply have to change the service URL in the app settings and in the service hook settings.
