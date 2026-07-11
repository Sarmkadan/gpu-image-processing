# FileOperationUtilities

Utility class providing safe, asynchronous file system operations with metadata inspection, hashing, and atomic writes. Designed for robust file handling in image processing pipelines where data integrity and thread safety are critical.

## API

### `CalculateFileHashAsync`
Computes the SHA-256 hash of a file asynchronously.

- **Parameters**
  - `filePath` (string): Absolute or relative path to the file.
- **Returns**
  - `Task<string>`: Hex-encoded SHA-256 hash of the file content.
- **Exceptions**
  - `FileNotFoundException`: If `filePath` does not exist.
  - `IOException`: If file access is denied or the file is locked.
  - `UnauthorizedAccessException`: If the caller lacks permissions.

### `VerifyFileHashAsync`
Validates that a file's content matches a provided SHA-256 hash.

- **Parameters**
  - `filePath` (string): Path to the file to verify.
  - `expectedHash` (string): Expected SHA-256 hash in hexadecimal format.
- **Returns**
  - `Task<bool>`: `true` if the computed hash matches `expectedHash`; otherwise, `false`.
- **Exceptions**
  - `FileNotFoundException`: If `filePath` does not exist.
  - `ArgumentException`: If `expectedHash` is not a valid hexadecimal string.

### `SafeCopyFileAsync`
Copies a file from source to destination with overwrite protection and atomic finalization.

- **Parameters**
  - `sourcePath` (string): Path to the source file.
  - `destinationPath` (string): Destination file path.
  - `overwrite` (bool, optional): Whether to overwrite an existing file. Defaults to `false`.
- **Returns**
  - `Task<bool>`: `true` if the file was copied; `false` if the destination already exists and `overwrite` is `false`.
- **Exceptions**
  - `FileNotFoundException`: If `sourcePath` does not exist.
  - `IOException`: If the source is inaccessible or the destination directory is unwritable.
  - `UnauthorizedAccessException`: If permissions are insufficient.

### `GetFileMetadata`
Retrieves metadata for a file without loading its content.

- **Parameters**
  - `filePath` (string): Path to the file.
- **Returns**
  - `FileMetadata`: Object containing file name, size, timestamps, extension, and read-only status.
- **Exceptions**
  - `FileNotFoundException`: If `filePath` does not exist.
  - `IOException`: If metadata cannot be accessed.

### `SafeDeleteFileAsync`
Deletes a file only if it exists and is not read-only.

- **Parameters**
  - `filePath` (string): Path to the file to delete.
- **Returns**
  - `Task`: Completes when deletion is attempted.
- **Exceptions**
  - `IOException`: If the file is read-only or deletion fails.
  - `UnauthorizedAccessException`: If permissions prevent deletion.

### `EnsureDirectoryExists`
Ensures that a directory exists, creating it if necessary.

- **Parameters**
  - `directoryPath` (string): Path to the directory.
- **Returns**
  - `DirectoryInfo`: The `DirectoryInfo` instance for the resolved path.
- **Exceptions**
  - `UnauthorizedAccessException`: If the caller lacks permissions to create or access the directory.

### `IsValidFilePath`
Checks whether a given string is a valid file system path.

- **Parameters**
  - `path` (string): The path to validate.
- **Returns**
  - `bool`: `true` if the path is valid and normalized; otherwise, `false`.
- **Remarks**
  - Validates against system-specific path rules and disallows path traversal sequences.

### `GetUniqueFileName`
Generates a unique filename in a target directory by appending a numeric suffix if a collision occurs.

- **Parameters**
  - `directoryPath` (string): Target directory.
  - `desiredName` (string): Base name for the file (without extension).
  - `extension` (string): File extension including the leading dot (e.g., `.jpg`).
- **Returns**
  - `string`: A unique filename within `directoryPath` with the same extension.
- **Exceptions**
  - `DirectoryNotFoundException`: If `directoryPath` does not exist.

### `ReadFileAsync`
Reads the entire content of a file as a UTF-8 string.

- **Parameters**
  - `filePath` (string): Path to the file.
- **Returns**
  - `Task<string>`: File content as a string.
- **Exceptions**
  - `FileNotFoundException`: If `filePath` does not exist.
  - `IOException`: If the file is locked or access is denied.
  - `UnauthorizedAccessException`: If permissions are insufficient.

### `WriteFileAtomicAsync`
Atomically writes content to a file by writing to a temporary file and renaming it upon success.

- **Parameters**
  - `filePath` (string): Destination file path.
  - `content` (string): Content to write.
  - `encoding` (Encoding, optional): Text encoding. Defaults to UTF-8.
- **Returns**
  - `Task`: Completes when the file is written atomically.
- **Exceptions**
  - `IOException`: If the temporary file cannot be created or renamed.
  - `UnauthorizedAccessException`: If permissions prevent writing to the target directory.

### `FileMetadata` Members

- `Name` (string): File name without path.
- `FullPath` (string): Full absolute path to the file.
- `SizeBytes` (long): File size in bytes.
- `SizeFormatted` (string): Human-readable size (e.g., "2.4 MB").
- `CreatedAt` (DateTime): File creation timestamp.
- `ModifiedAt` (DateTime): Last modification timestamp.
- `Extension` (string): File extension including the leading dot.
- `IsReadOnly` (bool): Whether the file is read-only.

## Usage

### Example 1: Hashing and Verifying a File
