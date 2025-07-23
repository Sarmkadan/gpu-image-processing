# ImageFormatHelper

Utility class providing static methods to inspect and convert common image file extensions and formats, typically used when working with GPU-accelerated image processing pipelines where format compatibility and transparency support are critical.

## API

### `public static string GetExtension(ImageFormat format)`

Returns the standard file extension (including the leading dot) associated with the given `ImageFormat` value.

- **Parameters**
  - `format`: The `ImageFormat` enum value whose extension is requested.
- **Return value**
  - A string representing the canonical file extension (e.g., ".png", ".jpg").
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `format` is not a defined value in the `ImageFormat` enum.

---

### `public static ImageFormat FromExtension(string extension)`

Maps a file extension (with or without a leading dot) to the corresponding `ImageFormat` value.

- **Parameters**
  - `extension`: The file extension string to parse (case-insensitive).
- **Return value**
  - The `ImageFormat` enum value corresponding to the extension.
- **Exceptions**
  - Throws `ArgumentNullException` if `extension` is `null`.
  - Throws `ArgumentException` if the extension is unrecognized or malformed.

---

### `public static bool SupportsTransparency(ImageFormat format)`

Indicates whether the specified image format supports transparency (alpha channel).

- **Parameters**
  - `format`: The `ImageFormat` enum value to check.
- **Return value**
  - `true` if the format supports transparency; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `format` is not a defined value in the `ImageFormat` enum.

---
### `public static bool IsCompressed(ImageFormat format)`

Indicates whether the specified image format is typically stored in a compressed form (e.g., JPEG, WebP).

- **Parameters**
  - `format`: The `ImageFormat` enum value to check.
- **Return value**
  - `true` if the format is compressed; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `format` is not a defined value in the `ImageFormat` enum.

## Usage
