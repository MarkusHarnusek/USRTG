# USRTG Server

## Overview
The USRTG Server is a .NET-based application designed to handle network communication and packet processing. It is structured to provide efficient and scalable solutions for real-time data handling.

## Project Structure

The server directory contains the following key components:

- **Network.cs**: Handles network communication logic.
- **Packet.cs**: Defines the structure and processing of packets.
- **Program.cs**: The entry point of the application.
- **USRTG.csproj**: Project file for managing dependencies and build configurations.
- **AC/**: Contains auxiliary classes and structures:
  - `ACEnums.cs`: Enumerations used across the application.
  - `ACHandle.cs`: Handles auxiliary computations or operations.
  - `ACStructs.cs`: Defines auxiliary data structures.

## Build and Run

### Prerequisites
- .NET 9.0 SDK installed on your system.

### Steps
1. Navigate to the `server` directory.
2. Run the following command to build the project:
   ```pwsh
   dotnet build
   ```
3. To execute the application, use:
   ```pwsh
   dotnet run
   ```

## Debugging

Debug binaries are located in the `bin/Debug/net9.0/` directory. Use these for testing and debugging purposes.

## Contributions

Feel free to contribute to the project by submitting issues or pull requests. Ensure that your code adheres to the project's coding standards.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

# Utility Scripts

## getCurrentData.js

This script is a browser-based utility designed to fetch and display live data from a specified endpoint. It creates a user interface panel in the browser for real-time data fetching and monitoring.

### Features
- **Live Fetch Panel**: Displays a floating panel in the browser for fetching data from the endpoint `http://10.214.10.8:8080/packet`.
- **Customizable Interval**: Allows users to set the fetch interval (default is 2000ms).
- **Start/Stop Controls**: Provides buttons to start and stop the live data fetching process.
- **Error Handling**: Logs errors and implements an exponential backoff mechanism for retrying failed requests.
- **Output Display**: Shows the fetched data and logs in a user-friendly format.

### How to Use
1. Include the script in your browser environment.
2. Open the browser console and execute the script.
3. A floating panel will appear in the top-right corner of the browser window.
4. Use the `Start` button to begin fetching data and the `Stop` button to halt the process.
5. Adjust the fetch interval using the input field provided in the panel.

### Notes
- Ensure that the server at `http://10.214.10.8:8080/packet` is accessible and allows cross-origin requests (CORS).
- Press the `✕` button or the `Escape` key to close the panel.

This script is useful for debugging and monitoring live data streams in real-time.