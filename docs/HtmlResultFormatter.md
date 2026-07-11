# HtmlResultFormatter
The `HtmlResultFormatter` type is designed to format the results of image processing operations into HTML strings, making it easier to display the output in a web-based interface. This class provides various formatting methods to accommodate different types of results, such as individual results, collections of results, jobs, devices, and errors.

## API
* `public string GetFileExtension`: Returns the file extension associated with the HTML format. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public string GetMimeType`: Returns the MIME type associated with the HTML format. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public string FormatResult`: Formats a single result into an HTML string. This method takes no parameters and returns a string. It does not throw any exceptions.
* `public string FormatResults`: Formats a collection of results into an HTML string. This method takes no parameters and returns a string. It does not throw any exceptions.
* `public string FormatJob`: Formats a job into an HTML string. This method takes no parameters and returns a string. It does not throw any exceptions.
* `public string FormatDevice`: Formats a device into an HTML string. This method takes no parameters and returns a string. It does not throw any exceptions.
* `public string FormatError`: Formats an error into an HTML string. This method takes no parameters and returns a string. It does not throw any exceptions.
* `public string Format`: A general-purpose formatting method. This method takes no parameters and returns a string. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `HtmlResultFormatter` class:
```csharp
// Example 1: Formatting a single result
HtmlResultFormatter formatter = new HtmlResultFormatter();
string html = formatter.FormatResult();
Console.WriteLine(html);

// Example 2: Formatting a collection of results
HtmlResultFormatter formatter2 = new HtmlResultFormatter();
string html2 = formatter2.FormatResults();
Console.WriteLine(html2);
```

## Notes
When using the `HtmlResultFormatter` class, keep in mind that the formatting methods do not throw exceptions. However, the class may not be thread-safe, as the documentation does not explicitly state this. Therefore, it is recommended to use the class in a single-threaded context or to synchronize access to the class instance. Additionally, the class does not provide any methods to customize the formatting, so the output will always be in the default HTML format. If custom formatting is required, consider using a different formatter or modifying the output HTML string after formatting.
