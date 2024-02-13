# Hacker News Top Stories API

## Overview

This project is an ASP.NET Core Web API that retrieves the top `n` best stories from Hacker News, sorted by score. It implements caching to improve performance and reduce the load on the Hacker News API.

## Running the Application

1. Ensure you have .NET SDK installed (version 8.0).
2. Clone the repository to your local machine.
3. Navigate to the project directory in your terminal.
4. Run `dotnet restore` to install required packages.
5. Run `dotnet run` to start the application.
6. Access the API at `https://localhost:5001/api/stories/{n}`, replacing `{n}` with the number of stories you want to retrieve.

## Assumptions

- The Hacker News API's rate limits are not publicly documented, so a conservative caching strategy was implemented.
