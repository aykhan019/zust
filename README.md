# Zust Social Media App - Getting Started Guide

Welcome to Zust Social Media App! This guide will help you get started with setting up and running the application using Visual Studio 2022. The app utilizes Entity Framework Code First approach and requires a SQL database with the necessary data. Before you proceed, ensure that you have the following prerequisites installed:

1. Visual Studio 2022 (or a compatible version).
2. SQL Server (Express Edition or higher) or SQL Server LocalDB.

## Clone the Repository

To get started, first, you need to clone the GitHub repository to your local machine. You can do this by following these steps:

1. Open a terminal or command prompt.
2. Change the current working directory to the location where you want to clone the repository.
3. Execute the following command:

```bash
git clone https://github.com/aykhan019/Zust
```

## Set Connection String

The connection string is read from the `appsettings.json` file, and it is already provided with a default value. There is no need to change the connection string unless you want to use a specific SQL Server instance or database name. If you wish to use the default settings, you can proceed to the next step.

## Set Up the Database

Next, we'll set up the SQL database for Zust Social Media App using Entity Framework Code First. Follow these steps to create the database and apply the initial migrations:

1. Open Visual Studio 2022.
2. Open the solution file (`Zust.sln`).
3. In the Solution Explorer, locate the `Zust.Web` project and make sure it's set as the startup project (right-click on the project and select "Set as Startup Project").
4. Open the Package Manager Console (PMC) from the Visual Studio menu: Tools > NuGet Package Manager > Package Manager Console.
5. In the PMC, make sure that the "Default Project" selected is `Zust.Web`.
6. Run the following command to create a new migration:

```bash
Add-Migration InitialCreate
```

This command will generate a migration file based on the changes in the model classes.

7. After creating the migration, run the following command to apply the initial migrations and create the database:

```bash
Update-Database
```

The `Update-Database` command will create the database and set up the required tables based on the defined model classes.

Please note that `Add-Migration` needs to be run whenever you make changes to the model classes or database schema in the future. It generates new migrations to capture those changes, and you need to run `Update-Database` again to apply those changes to the database.

## Insert Initial Data

Now that the database is set up with the required tables, you need to insert the initial data into the database. Zust Social Media App provides SQL scripts to insert the initial data. Follow these steps to insert the data:

1. Open the Solution Explorer.
2. Navigate to the `Database` folder.
3. Locate the file named `SqlInsertStatements.sql`.
4. Execute the SQL script by running it on your preferred database management tool or environment.

## Run the Application

Now that the database is set up with the initial data, and the connection string is configured, you can run the application using Visual Studio 2022. Follow these steps to run the application:

1. Open the solution file (`Zust.sln`) in Visual Studio 2022.
2. Set the `Zust.Web` project as the startup project (right-click on the project in the Solution Explorer and select "Set as Startup Project").
3. Press F5 or click the "Start" button in Visual Studio to run the application.

The application will build, and a new browser window will open with Zust Social Media App up and running.

## Contributing

If you'd like to contribute to the project, feel free to fork the repository, make your changes, and submit a pull request.

## Issues and Support

If you encounter any issues or need support, please check the existing issues on the GitHub repository. If your issue isn't already reported, feel free to open a new one.

## License

Zust Social Media App is released under the [MIT License](LICENSE). Feel free to use, modify, and distribute the code as per the terms of the license.

## Project Showcase

Check out this YouTube video showcasing Zust Social Media App:

<a href="https://www.youtube.com/watch?v=vOdeSuh_zMY">
    <img src="https://media.aykhan.net/thumbnails/step-it-academy/asp-net/task27.png" alt="Description" width="100%">
</a>

