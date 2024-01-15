# CampusConnections Source Code

### Introduction

This is a prototype of CampusConnections application. This application aims to give McMaster students and visitors an immersive user experience when walking on campus. This unity project will include user authentication, friend interaction, campus map and lecture/event management features. It imports some third-party libraries including Firebase for authentication and data storing, Mapbox for customized maps .

This prototype includes user real time location tracking, user authentication and basic database connection, allowing user to do basic CRUD operations.

  

### How to install

Open the project with Unity Editor version *__2022.3.13f__*, which is the newest long term support release (2022.3 works in gerneral). Since Mapbox and Firebase packages have some conflicts, the team includes a customized Mapbox package in version control and the only packages the contributor needs to import are __FirebaseAuth__ and __FirebaseDatabase__ (NOTE: import __ALL__ folders). After importing corresponding Firebase packages, the project should be able to run.