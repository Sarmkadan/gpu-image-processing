# InteractiveShell
The `InteractiveShell` type provides a lightweight read‑eval‑print loop (REPL) for interacting with GPU‑image‑processing commands. It manages command registration, execution, and autocomplete facilities, allowing host applications to expose a console‑like interface for image‑processing workflows.

## API
### InteractiveShell()
Initializes a new instance of the shell with an empty command table and no registered handlers.

### RegisterHandler(string command, Func<CancellationToken, Task> handler)
Registers an asynchronous handler for a specific command.  
- **command**: The textual identifier that invokes the handler; must not be null or whitespace.  
- **handler**: A delegate that receives a cancellation token and returns a `Task` representing the operation; must not be null.  
- **Throws**: `ArgumentException` if `command` is empty or consists only of whitespace; `ArgumentNullException` if either argument is null; `InvalidOperationException` if a handler is already registered for the same command.

### RunAsync(CancellationToken cancellationToken = default)
Starts the shell’s main loop, reading input lines, invoking registered handlers, and providing autocomplete suggestions until the token is cancelled or the user exits.  
- **cancellationToken**: Optional token to signal termination; defaults to `None`.  
- **Returns**: A `Task` that completes when the loop ends.  
- **Throws**: `OperationCanceledException` if the token is triggered before the loop starts; `ObjectDisposedException` if the shell has been disposed.

### Add(string item)
Appends a string to the internal history buffer used by the shell.  
- **item**: The text to store; must not be null.  
- **Throws**: `ArgumentNullException` if `item` is null.

### GetAll()
Retrieves a snapshot of all strings currently stored in the history buffer.  
- **Returns**: A new `List<string>` containing the history entries in the order they were added.  
- **Throws**: None.

### CompletionProvider
Gets the object responsible for generating autocomplete suggestions based on the current input and registered commands.  
- **Returns**: A non‑null `CompletionProvider` instance; the provider’s lifetime is bound to the shell.  
- **Throws**: None.

### RegisterCommand(string name, Action<string[]> execute)
Registers a synchronous command that is invoked when the user types `name` followed by optional arguments.  
- **name**: The command identifier; must not be null or whitespace.  
- **execute**: A delegate that receives the parsed arguments (split by whitespace) and performs the command’s work; must not be null.  
- **Throws**: `ArgumentException` if `name` is empty or whitespace; `ArgumentNullException` if either argument is null; `InvalidOperationException` if a command with the same name is already registered.

### GetSuggestions(string partial)
Returns a list of possible completions for the given partial input.  
- **partial**: The text entered so far; may be null or empty, in which case all registered command names are returned.  
- **Returns**: A new `List<string>` containing matching command names or history entries, ordered by relevance.  
- **Throws**: None.

## Usage
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

var shell = new InteractiveShell();

// Register a simple command that prints the supplied arguments.
shell.RegisterCommand("echo", args =>
{
    Console.WriteLine(string.Join(" ", args));
});

// Register an asynchronous handler for a more complex operation.
shell.RegisterHandler("process", async token =>
{
    // Simulate GPU work.
    await Task.Delay(500, token);
    Console.WriteLine("Image processing completed.");
});

// Start the shell; exit by typing "exit" or cancelling the token.
var cts = new CancellationTokenSource();
Task.Run(() => shell.RunAsync(cts.Token));

// Later, to shut down:
// cts.Cancel();
```

```csharp
using System.Collections.Generic;
using System.Threading;

// Populate history and retrieve suggestions.
var shell = new InteractiveShell();
shell.Add("load image.png");
shell.Add("save result.jpg");

var history = shell.GetAll(); // ["load image.png", "save result.jpg"]
var suggestions = shell.GetSuggestions("lo"); // ["load image.png"]
```
## Notes
- The shell is **not** thread‑safe for concurrent modifications; all registration methods (`RegisterHandler`, `RegisterCommand`, `Add`) should be invoked from a single thread or protected by external synchronization before calling `RunAsync`.  
- `GetAll` and `GetSuggestions` return copies of internal collections; modifying the returned lists does not affect the shell’s state.  
- Registering a handler or command with a name that is already registered throws `InvalidOperationException`; callers must first unregister (if supported) or choose a distinct identifier.  
- The `CompletionProvider` property returns a live provider that reflects the current set of commands and history; its methods may be called concurrently with `RunAsync`, but the provider itself does not guarantee thread‑safety for its internal caches.  
- Passing `null` for any delegate or string argument results in an `ArgumentNullException`. Empty or whitespace‑only command identifiers trigger an `ArgumentException`.  
- The shell does not automatically trim or normalize input; handlers should expect the raw string as split by whitespace.  
- Cancellation via the token passed to `RunAsync` stops the input loop immediately; any handler already executing will receive the token and should observe it to terminate promptly.
