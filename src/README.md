# CampusConnections Source Code

### Introduction

This is a prototype of CampusConnections application. This application aims to give McMaster students and visitors an immersive user experience when walking on campus. This unity project will include user authentication, friend interaction, campus map and lecture/event management features. It imports some third-party libraries including Firebase for authentication and data storing, Mapbox for customized maps .

This prototype includes user real time location tracking, user authentication and basic database connection, allowing user to do basic CRUD operations.

  

### How to contribute

Open the project with Unity Editor version *__2022.3.13f__*, which is the newest long term support release (2022.3 works in gerneral). Download the correct version of Unity Editor and clone the project. The following are some 3rd party packages you need to install locally. After importing all the following packages, the project should be able to run.

#### Dependencies: Firebase
Since Mapbox and Firebase packages have some conflicts, the team includes a customized Mapbox package in version control and the only packages the contributor needs to import From Firebase (version *__11.6.0__*) are __FirebaseAuth__ and __FirebaseDatabase__ (NOTE: import __ALL__ folders). 

#### Dependencies: Vuforia
Install Vuforia-10-20-3 from their website with the contributor's account and import the package into the project.

#### Dependencies: FancyCarouselView
Import the package in Package Manager with git url: https://github.com/setchi/FancyScrollView.git#upm