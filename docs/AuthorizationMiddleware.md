# AuthorizationMiddleware

The `AuthorizationMiddleware` class provides mechanisms for managing API key-based authentication and authorization within the request processing pipeline. It facilitates the registration and revocation of API keys, associates keys with specific users and scopes, and validates incoming requests by processing them against the configured authorization rules.

## API

### Constructors

*   **`AuthorizationMiddleware()`**
    Initializes a new instance of the `AuthorizationMiddleware` class.

### Methods

*   **`void RegisterApiKey(string key, string userId, List<string> scopes)`**
    Registers a new API key and associates it with the specified user and authorization scopes.
*   **`void RegisterUser(string userId)`**
    Registers a new user within the authorization system.
*   **`Task<RequestMiddlewareResult> ProcessAsync(Request request)`**
    Processes the provided request to authenticate the API key and authorize the action based on the associated scopes. Returns a `RequestMiddlewareResult` indicating the outcome of the processing.
*   **`void RevokeApiKey(string key)`**
    Revokes and deactivates the specified API key, rendering it invalid for further requests.
*   **`List<ApiKeyInfo> ListActiveKeys()`**
    Retrieves a list of all currently active API keys.

### Properties

*   **`string Key`**
    Gets or sets the API key identifier.
*   **`string UserId`**
    Gets or sets the identifier of the user associated with the API key.
*   **`List<string> Scopes`**
    Gets or sets the list of authorization scopes assigned to the API key.
*   **`DateTime CreatedAt`**
    Gets or sets the timestamp indicating when the API key was created.
*   **`bool IsActive`**
    Gets or sets a value indicating whether the API key is currently active.
*   **`string KeyPreview`**
    Gets a preview string of the API key, typically used for logging or display purposes without revealing the full key.

## Usage

### Registering a New API Key
```csharp
var middleware = new AuthorizationMiddleware();
middleware.RegisterUser("user_123");
middleware.RegisterApiKey("secret_key_abc", "user_123", new List<string> { "read", "write" });
```

### Processing a Request
```csharp
var middleware = new AuthorizationMiddleware();
// ... assume middleware is configured and keys are registered ...

var request = new Request { /* ... */ };
RequestMiddlewareResult result = await middleware.ProcessAsync(request);

if (result.IsAuthorized)
{
    // Proceed with request
}
else
{
    // Handle unauthorized request
}
```

## Notes

*   **Thread Safety**: The `AuthorizationMiddleware` must be implemented to ensure thread safety when registering, revoking, or listing API keys, as these operations may occur concurrently with request processing.
*   **Key Validation**: The `ProcessAsync` method assumes that the underlying implementation handles the secure lookup and validation of the API key provided in the request.
*   **User Registration**: Attempting to register an API key for a user that has not been explicitly registered via `RegisterUser` may result in an exception, depending on the implementation constraints.
*   **Scope Management**: Scopes are enforced during the `ProcessAsync` stage; ensure that the requested action maps correctly to the scopes assigned during `RegisterApiKey`.
