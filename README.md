# CloakVision: A .NET 8 Image Service

"CloakVision" is a .NET 8 Web API designed as an image service. Its primary function is to manage and serve images, leveraging Azure for cloud storage and a relational database for metadata. The API is built with a clean architecture, separating concerns into distinct layers: Core (domain logic), Infrastructure (data access and external services), and the Web API (presentation).

## Key Functionalities

* **Image Management:** The API provides endpoints to manage images, including:
  * Retrieving all images.
  * Fetching a specific image by its unique identifier (GUID).
  * (Future) Creating new images.
* **Dynamic Image URLs:** Instead of storing static, publicly accessible URLs, the service generates Shared Access Signature (SAS) URLs for images stored in Azure Blob Storage. This enhances security by providing temporary, permission-based access to the image files.
* **Database Integration:** Image metadata (name, path, description, etc.) is stored in a SQL Server database, accessed via Entity Framework Core. This allows for efficient querying and management of image information.
* **Configuration Management:** The application utilizes Azure Key Vault to securely store and manage sensitive configuration data like database connection strings, separating them from the application's source code.

## Technical Architecture and Design

* **.NET 8 and ASP.NET Core:** The project is built on the latest long-term support (LTS) version of .NET, providing a modern, high-performance foundation for the web API.
* **Clean Architecture:** The codebase is structured into three main projects:
  * **CloakVision (Web API):** The entry point of the application, responsible for handling HTTP requests, controllers, and dependency injection setup.
  * **Core:** Contains the application's business logic, including domain entities, interfaces for services and repositories, and Data Transfer Objects (DTOs). This layer has no dependencies on external frameworks like Entity Framework or Azure SDKs.
  * **Infrastructure:** Implements the interfaces defined in the Core layer. This includes the database context, repositories that interact with the database, and any other external service integrations.
* **Repository Pattern:** The project uses a generic repository pattern to abstract the data access logic, making the application more testable and maintainable. A `BaseRepository` provides common CRUD (Create, Read, Update, Delete) operations, which can be extended by specific repositories like the `ImageRepository`.
* **Dependency Injection:** The application makes extensive use of dependency injection to manage the lifetime and dependencies of services, repositories, and other components. This promotes loose coupling and testability.
* **Azure Integration:**
  * **Azure Blob Storage:** Used for storing the actual image files. The `BlobServiceClient` is registered to use `DefaultAzureCredential` for authentication, allowing the application to securely access the storage account without hardcoding credentials.
  * **Azure Key Vault:** As mentioned, this is used for secure configuration management.
* **API Documentation:** The project integrates Swagger (OpenAPI) to provide interactive API documentation. This includes detailed information about the API endpoints, request/response models, and contact information. The API also supports Scalar API references for an alternative documentation UI.
* **CORS:** Cross-Origin Resource Sharing (CORS) is configured to allow requests from any origin, which is useful for development and for allowing web applications hosted on different domains to interact with the API.
* **Error Handling:** A centralized `ApiResponseHelper` is used to create consistent and structured API responses for success and error scenarios, following REST best practices and the RFC 7807 problem details standard.

## How it Works

1. A client sends an HTTP request to an `ImageController` endpoint.
2. The controller calls the appropriate method on the `IImageService`.
3. The `ImageService` retrieves the image metadata from the `ImageRepository`.
4. The `ImageRepository`, using Entity Framework Core, queries the SQL Server database for the image information.
5. If the image path stored in the database is a relative path (not a full URL), the `ImageService` uses the `BlobServiceClient` to generate a secure, time-limited SAS URL for the image in Azure Blob Storage.
6. The `ImageService` returns the image data (including the potentially generated SAS URL) to the controller.
7. The controller, with the help of `ApiResponseHelper`, formats the data into a standard HTTP response and sends it back to the client.
