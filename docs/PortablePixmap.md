# PortablePixmap

Utility class for reading, writing, and manipulating Portable Pixmap (PPM) image files in the P6 (binary) format. Provides static methods for format detection, decoding, encoding, and pixel-level hashing.

## API

### `public static bool IsSupported(string filePath)`

Determines whether the file at the given path is a supported Portable Pixmap (PPM) file in P6 (binary) format. The check is based on the file extension and a basic header inspection.

- **Parameters**
  - `filePath` – Path to the file to check.
- **Return Value**
  Returns `true` if the file appears to be a valid PPM P6 file; otherwise, `false`.
- **Exceptions**
  Throws `ArgumentNullException` if `filePath` is `null`.
  Throws `ArgumentException` if `filePath` is empty or contains invalid path characters.
  Throws `FileNotFoundException` if the file does not exist.
  Throws `UnauthorizedAccessException` if the caller lacks permissions to read the file.

---

### `public static Image Load(string filePath)`

Loads a Portable Pixmap (PPM) image from the specified file path. The file must be in P6 (binary) format. The returned `Image` object contains the decoded pixel data in a standard format.

- **Parameters**
  - `filePath` – Path to the PPM P6 file to load.
- **Return Value**
  Returns an `Image` instance representing the decoded pixel data.
- **Exceptions**
  Throws `ArgumentNullException` if `filePath` is `null`.
  Throws `ArgumentException` if `filePath` is empty or invalid.
  Throws `FileNotFoundException` if the file does not exist.
  Throws `UnauthorizedAccessException` if the caller lacks read permissions.
  Throws `InvalidDataException` if the file is not a valid PPM P6 file or contains corrupt data.

---

### `public static Image Decode(Stream stream)`

Decodes a Portable Pixmap (PPM) image from the provided stream. The stream must contain data in P6 (binary) format. The stream is not closed by this method.

- **Parameters**
  - `stream` – A readable stream containing PPM P6 data.
- **Return Value**
  Returns an `Image` instance with the decoded pixel data.
- **Exceptions**
  Throws `ArgumentNullException` if `stream` is `null`.
  Throws `ArgumentException` if `stream` is not readable.
  Throws `InvalidDataException` if the stream does not contain valid PPM P6 data or is corrupted.

---

### `public static void Save(Image image, string filePath)`

Saves the given `Image` as a Portable Pixmap (PPM) file in P6 (binary) format at the specified file path.

- **Parameters**
  - `image` – The image to save.
  - `filePath` – Destination file path.
- **Exceptions**
  Throws `ArgumentNullException` if `image` or `filePath` is `null`.
  Throws `ArgumentException` if `filePath` is empty or invalid.
  Throws `UnauthorizedAccessException` if the caller lacks write permissions.
  Throws `InvalidOperationException` if the image contains unsupported pixel formats or dimensions.

---

### `public static string PixelHash(Image image)`

Computes a deterministic hash of the pixel data in the given `Image`. The hash is based on the raw pixel values and is intended for change detection or cache invalidation.

- **Parameters**
  - `image` – The image whose pixels are to be hashed.
- **Return Value**
  Returns a hexadecimal string representing the hash of the pixel data.
- **Exceptions**
  Throws `ArgumentNullException` if `image` is `null`.

---
### `public static string PixelHash(Stream stream)`

Computes a deterministic hash of the pixel data in the provided stream, which must contain PPM P6 data. The stream is not closed by this method.

- **Parameters**
  - `stream` – A readable stream containing PPM P6 data.
- **Return Value**
  Returns a hexadecimal string representing the hash of the pixel data.
- **Exceptions**
  Throws `ArgumentNullException` if `stream` is `null`.
  Throws `ArgumentException` if `stream` is not readable.
  Throws `InvalidDataException` if the stream does not contain valid PPM P6 data.

## Usage
