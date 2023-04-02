using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using doob.Reflectensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

#pragma warning disable 618
#pragma warning disable SYSLIB0001

namespace doob.Middler.Action.Scripting.Helper;

public static class FileStreamingHelper
{
    internal static readonly FormOptions _defaultFormOptions = new FormOptions();

    public static async Task<MultiPartContent> StreamFiles(this HttpRequest request, DirectoryInfo directory, string path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = "";

        var dirName = Path.Combine(directory.FullName, path);
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);


        var result = new List<FileStreamInfo>();

        if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            throw new Exception($"Expected a multipart request, but got {request.ContentType}");
        }

        // Used to accumulate all the form url encoded key value pairs in the 
        // request.
        var formAccumulator = new KeyValueAccumulator();


        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            ContentDispositionHeaderValue contentDisposition;
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {

                    FileMultipartSection currentFile = section.AsFileSection();
                    var fileName = currentFile.FileName;
                    var filePath = Path.Combine(dirName, fileName);
                    var fileInfo = new FileInfo(filePath);

                    if (!fileInfo.Directory.Exists)
                    {
                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                    }


                    using (var targetStream = File.Create(filePath))
                    {
                        await section.Body.CopyToAsync(targetStream).ConfigureAwait(false);
                    }
                    result.Add(new FileStreamInfo(filePath, fileName));

                    //await section.Body.CopyToAsync(targetStream);
                }
                else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    // Content-Disposition: form-data; name="key"
                    //
                    // value

                    // Do not limit the key name length here because the 
                    // multipart headers length limit is already in effect.
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                    var encoding = section.GetEncoding();
                    using (var streamReader = new StreamReader(
                        section.Body,
                        encoding,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 1024,
                        leaveOpen: true))
                    {
                        // The value length limit is enforced by MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();
                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }
                        formAccumulator.Append(key.ToString(), value);
                        
                        if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                        {
                            throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                        }
                    }
                }
            }

            // Drains any remaining section body that has not been consumed and
            // reads the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        Dictionary<string, StringValues> dict = formAccumulator.GetResults();

        return new MultiPartContent(new ExpandableObject(dict),  result.ToArray());

    }


    internal static Encoding GetEncoding(this MultipartSection section)
    {
        MediaTypeHeaderValue mediaType;
        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
        // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
        // most cases.
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }
        return mediaType.Encoding;
    }

}

public class FileStreamInfo
{

    public string LocalPath { get; }

    public string FileName { get; set; }

    public string ReadAsBase64()
    {
        var bytes = ReadAsBytes();
        return Convert.ToBase64String(bytes);
    }

    public string ReadAsString()
    {
        return File.ReadAllText(LocalPath);
    }

    public byte[] ReadAsBytes()
    {
        return File.ReadAllBytes(LocalPath);
    }

    public FileStreamInfo(string localPath, string fileName)
    {
        LocalPath = localPath;
        FileName = fileName;
    }
}


public class MultiPartContent
{
    public ExpandableObject FormData { get; }
    public FileStreamInfo[] FileStreams { get; }

    public MultiPartContent(ExpandableObject formData, FileStreamInfo[] fileStreams)
    {
        FormData = formData;
        FileStreams = fileStreams;
    }
}
