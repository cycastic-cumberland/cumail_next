# CumailNEXT API server

This project is an API server for CumailNEXT, a chat application developed in C# using `ASP.NET` Core intended for my university semester project. The server follows a stateless design and consists of two main components: an authentication module and the main chat application module.

The full breakdown and features list of this project can be founded [here](https://docs.google.com/document/d/17DEX3SNfcOHoudm06Z7rVTxYYuEazj-0la5GNgK5hoY/edit?usp=sharing) in Vietnamese.

## Summary

CumailNEXT API Server is built to support a chat application and provides a robust backend infrastructure for managing user authentication and facilitating real-time communication between users. The server is implemented in C# using the `ASP.NET` Core framework, ensuring high performance and scalability.

## Features

- **Stateless Design**: The API server is designed to be stateless, allowing it to handle requests independently and horizontally scale as per demand. This design promotes flexibility and enhances the server's performance.
- **Authentication Module**: The authentication module is a crucial component of the API server. It manages user authentication, ensuring secure access to the chat app. It provides features such as user registration, login, logout.
- **Chat App Module**: The chat app module enables real-time communication between users. It handles various chat-related functionalities, including creating and managing chat rooms, sending and receiving messages, etc.

## Technologies used

- C#: The server is implemented using the C# programming language, known for its efficiency and versatility.
- `ASP.NET` Core: The server is built on the `ASP.NET` Core framework, which offers a robust and scalable platform for developing web applications.
- RESTful API: The server follows the principles of REST (Representational State Transfer) to provide a standardized and interoperable interface for communication.
- Redis: CumailNEXT API server use Redis to store authentication data, allow authentication operations to be performed at a much faster pace
- PostgreSQL: The main Chat App Module use PostgreSQL to store and query its data. PostgreSQL is one of the world most popular RDBMS, used by many corporations and companies alike thanks to its versatility and scalability

NOTE: This application can also use MongoDB as database, but support for that is not completed.

## Installation and Setup

To setup and use CumailNEXT API server, follow these steps:

- Clone [this repository](https://github.com/UwUOwOUmUOmO/cumail_next.git)
- Ensure that you have the latest version of [.NET 7.0 or higher](https://dotnet.microsoft.com/download) installed on your machine.
- Open the project in your integrated development environment (I personally used Rider, not sponsored)
- Install the required dependencies by running the following command in the terminal: `dotnet restore` 
- Build the solution using your preferred configuration
- After the build process is completed, navigate to the directory that contains the produced binary (look something likes `CumailNEXT\bin\Debug\net7.0`) and create a `config.json` file. This is the main configuration file for the server, and it must contains the following entries (Note that all entries have string values):
~~~ 
{
    "core/db/max_reconnection_time_ms": "<Maximum time for a database provider to reconnect, default: 500>",
    "auth/password/min_len": "<Minimum length for user password, default: 6>",
    "auth/password/salt_round": "<Salt round used for password hashing, default: 12>",
    "auth/db/endpoint": "<Endpoint for redis server used in authentication>",
    "auth/db/username": "<Username for redis server used in authentication, default empty>",
    "auth/db/password": "<Password for redis server used in authentication>",
    "auth/token/secret": "<String of random character, used for auth token generation>",
    "auth/token/expiration": "<The expiration rate for auth token, must be greater than 0 and have a decimal mark>",
    "auth/issuer": "<The token issuer, currently unimportant>",
    "chat/room_max_length": "<Max length for chat room name>",
    "chat/db/address": "<Endpoint for postgres server used for chat app>",
    "chat/db/dbname": "<Database name for postgres server used for chat app>",
    "chat/db/port": "<Database port for postgres server used for chat app>",
    "chat/db/username": "<Database username for postgres server used for chat app>",
    "chat/db/password": "<Database password for postgres server used for chat app>"
}
~~~
- Note: use the file `postgresql_setup.sql` to setup yout PosgreSQL server
- Run the executable (`CumailNEXT.exe`)
- To actually uses the application, start a CumailNEXT client, preferably the official CumailNEXT client from [this repository](https://github.com/UwUOwOUmUOmO/cumail_next_client.git)

## Usage

Once the Chat App API Server is up and running, you can interact with it using the exposed RESTful API endpoints. To see the list of active endpoints, run the server and the first webpage that shows up would be a Swagger page that list all the active endpoints

## Conclusion

CumailNEXT API server provides a reliable and efficient backend solution for a chat application. Its stateless design, along with the authentication and chat app modules, ensures secure user authentication and real-time communication capabilities. By leveraging technologies such as C# and `ASP.NET` Core, this API server offers a scalable and performant infrastructure for your chat app project.
