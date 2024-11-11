# SoftwareEngineering2024

To run the Updater:

- Run one instance of UI as Server, "Start Server"
- The UI will display the directory being monitored, you can add DLLs in this directory
- In Updater.AppConstants, update the IPAddress and Port to the server IP and Port
- Connect as client to the server ip and port

Features incooperated:

- UI/UX (view) complete development
- The communication between server and multiple client is working
- Only the latest version of a tool is displayed (To test add the 3 dlls given in the repo into the directory under monitor)
- The user is notified of any change in the target directory
- Proper logs for file transfer is maintained

Work in progress:

- The buttons on UI
- Cloud Integration

Already integrated:

- Networking team
- UI team

Yet to integrate:

- Dashboard team

![Class Diagram](./UpdaterTeam.png)
