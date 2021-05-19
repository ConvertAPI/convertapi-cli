# ConvertAPI CLI Client

## Convert your files with our command-line file conversion utility

ConvertAPI helps in converting various file formats. Creating PDF and Images from various sources like Word, Excel, Powerpoint, images, web pages or raw HTML codes. Merge, Encrypt, Split, Repair and Decrypt PDF files and many other file manipulations. You can integrate it into your application in just a few minutes and use it easily.

The ConvertAPI CLI makes it easier to use the Convert API from your shell without having to build your own HTTP calls.
You can get your free secret at https://www.convertapi.com/a

## Installation

Download compressed CLI executable

* Linux: [convertapi_lin.zip](https://github.com/ConvertAPI/convertapi-cli/releases/download/v1/convertapi_lin.zip)
* Linux ARM64: [convertapi_lin_arm.zip](https://github.com/ConvertAPI/convertapi-cli/releases/download/v1/convertapi_lin_arm.zip)
* Darwin (MacOS): [convertapi_mac.zip](https://github.com/ConvertAPI/convertapi-cli/releases/download/v1/convertapi_mac.zip)
* Windows: [convertapi_win.zip](https://github.com/ConvertAPI/convertapi-cli/releases/download/v1/convertapi_win.zip)

(this utility can also be built from source code for many other CPU and OS)

Unzip executable 

```shell
unzip convertapi_*.zip
```

And you are done.
Optionally you can move the executable file to a more appropriate place and make utility accessible for all local users. On Linux it would be:   

```shell
sudo mv convertapi /usr/local/bin
```

## Usage

### Before you start

In order to use this CLI utility, you must create your free trial account on https://www.convertapi.com site.  
After the sign-up process, you will get your secret at https://www.convertapi.com/a .
The secret will be used in every CLI utility run.

### Basic file conversion

Before we go into detail, a short usage example of how to convert DOCX file to PDF.

```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx" --out="@/path/to/resultdir" --secret=your-api-secret
```

### Arguments documentation 

#### --iformat
Input file format (file name extension).
All supported input formats can be found [here](https://www.convertapi.com).
_Example:_

```shell
--iformat=docx
```

#### --oformat
Output file format (file name extension).
All supported output formats can be found [here](https://www.convertapi.com).
_Example:_

```shell
--oformat=pdf
```

#### --params
Conversion parameter list.
All supported conversion parameters can be found [here](https://www.convertapi.com).
Parameter name and value is separated by `:` colon.
Parameters are separated by `,` comma.
_Example:_ 

```shell
--params="param1:value1, param2:value2, param3:value3"
```

##### File parameter

Special case is file parameter.
Value can have a prefix and can be provided as an array with the `;` semicolon separator. 
The prefix can be one of those:

###### no prefix

No prefix means that the value is http or https scheme URL to a file.
_Example:_ 

```shell
--params="file:https://cdn.convertapi.com/cara/testfiles/presentation.pptx"
```

###### @

Parameter values starting with `@` are treated as paths to the local files or directories.
_Example:_ 

```shell
--params="file:@/path/to/myfile.doc"
```

###### <

Parameters with `<` values are substituted with the data taken from `STDIN`.
As a raw value can only be the URL then, in this case, the data received from `STDIN` should be URL or URL array separated by `\n`
_Example:_ 

```shell
--params="file:<"
```

###### @<

Parameters with `@<` values are substituted with the data taken from `STDIN`.
Data received from `STDIN` should be a local path to the file or directory. 
It can also be an array of paths separated by `\n`.
_Example:_ 

```shell
--params="file:@<"
```

###### <<

Parameters with `<<` values are substituted with the data taken from `STDIN`.
Data received from `STDIN` should be file content. 
_Example:_ 

```shell
--params="file:<<"
```

##### Array parameter

If a parameter name is suffixed with `[]`, the parameter is treated as an array parameter.
Mainly array parameters are used when one conversion needs to accept multiple files (e.g. pdf merge or zip compression).

```shell
--params="files[]:@/path/to/dir"
```

#### --out

The argument defines how conversion result should be outputted.
Values can be one of those:

##### url

This is the default output method. It prints to `STDOUT` URL or array of URLs that point to the converted files.
This method should be used for conversion chaining.

##### @

Value prefixed with `@` is treated as a local file or directory path where the converted file will be stored. 
_Example:_ 

```shell
--out="@/path/to/result.pdf"
```
or
```shell
--out="@/path/to/resultdir"
```

##### stdout

The Conversion result will be outputted to `STDOUT`.
If the result contains multiple files, second and the following files will be outputted to the file descriptors starting from 3.

```shell
--out="stdout"
```

#### --secret

ConvertAPI user secret.
Get your secret at https://www.convertapi.com/a

#### --version

Outputs CLI utility version information and exits.

#### --help

Displays a short usage information.


### Examples

Convert a local DOCX file to PDF A3 page size saving the result to `/path/to/resultdir`
```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx, pagesize:a3" --out="@/path/to/resultdir" --secret=your-api-secret
```

Merge all PDF files that are located in `/path/to/dir` directory and save it locally
```shell
convertapi --iformat=pdf --oformat=merge --params="files[]:@/path/to/dir" --out="@/path/to/resultdir" --secret=your-api-secret
```

Convert remote PPTX file to PDF saving the result to `/path/to/result.pdf`
```shell
convertapi --iformat=pptx --oformat=pdf --params="file:https://example.com/myfile.pptx" --out="@/path/to/result.pdf" --secret=your-api-secret
```

Convert from DOCX to JPG and ZIP the result JPG files into a single archive
```shell
convertapi --iformat=docx --oformat=jpg --params="file:@/path/to/test.docx" --secret=your-api-secret \
    | convertapi --iformat=jpg --oformat=zip --params="files[]:<" --out="@/path/to/result.zip" --secret=your-api-secret
```

Convert DOCX to PDF and save the result on a remote server over SSH
```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx" --out=stdout --secret=your-api-secret \
    | ssh user@myserver "cat >/tmp/my.pdf"
```

Get the PDF file from a remote server, convert it to JPG and save the result locally
```shell
ssh user@server "cat /tmp/my.pdf" \
    | convertapi --iformat=pdf --oformat=jpg --params="file:<<" --out=@/path/to/resultdir --secret=your-api-secret
```

Do PDF->JPG and DOCX->JPG conversions in parallel and ZIP the converted JPG files
```shell
( \
    convertapi --iformat=pdf --oformat=jpg --params="file:/path/to/dir" --secret=your-api-secret \
    & convertapi --iformat=docx --oformat=jpg --params="file:@/path/to/dir" --secret=your-api-secret \
) | convertapi --iformat=jpg --oformat=zip --params="files[]:<" --out=@/path/to/resultdir  --secret=your-api-secret
```

Merge PDFs files from various locations: a remote SSH server, local file, local directory, and a remote HTTP server.
Save the result file on a remote SSH server. All of this is done without writing to disk. 
```shell
ssh user@server1 "cat /tmp/my.pdf" \
    | convertapi --iformat=pdf --oformat=merge --params="files[]:<<;@/path/to/test.pdf;@/path/to/dir;https://example.com/my.pdf" --out=stdout --secret=your-api-secret \
    | ssh user@myserver2 "cat >/tmp/my.pdf"
```

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI CLI is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license.
Refer to the [LICENSE](https://raw.githubusercontent.com/ConvertAPI/convertapi-cli/master/LICENSE.txt) file for more information.
