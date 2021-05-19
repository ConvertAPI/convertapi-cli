# ConvertAPI CLI Client

## Convert your files with our command line file conversion utility

The ConvertAPI helps converting various file formats.
Creating PDF and Images from various sources like Word, Excel, Powerpoint, images, web pages or raw HTML codes.
Merge, Encrypt, Split, Repair and Decrypt PDF files.
And many others files manipulations.
In just few minutes you can integrate it into your application or shell script and use it easily.

The ConvertAPI CLI makes it easier to use the Convert API from your shell without having to build your own HTTP calls.
You can get your free secret at https://www.convertapi.com/a

## Installation

Download compressed CLI executable

* Linux: [convertapi_lin.zip](http://github)
* Linux ARM64: [convertapi_lin_arm.zip](http://github)
* Darwin (MacOS): [convertapi_mac.zip](http://github)
* Windows: [convertapi_win.zip](http://github)

(this utility also can be built from source code for many other CPU and OS)

Unzip executable 

```shell
unzip convertapi_*.zip
```

And you are done.
Optionally you can move executable file to more appropriate place and make utility accessible for all local users. On Linux would be:   

```shell
sudo mv convertapi /usr/local/bin
```

## Usage

### Before you start

In order to use this CLI utility you must create your free trial account on https://www.convertapi.com site.  
After sign up process you will get your secret at https://www.convertapi.com/a .
Secret will be used in every CLI utility run.

### Most basic file conversion

Before we go in to details, short usage example how to convert DOCX file to PDF.

```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx" --out="@/path/to/resultdir" --secret=<YOUR_SECRET_HERE>
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
Comma `,` inside the parameter value must be escaped with `\,`.


##### File parameter

Special case is file parameter.
Value can have prefix and can be provided as an array with `;` semicolon separator. 
Prefix can be one of those:

###### no prefix

No prefix means that value is http or https scheme URL to file.
_Example:_ 

```shell
--params="file:https://cdn.convertapi.com/cara/testfiles/presentation.pptx"
```

###### @

Parameter values starting from `@` are treated as paths to local files or directories.
_Example:_ 

```shell
--params="file:@/path/to/myfile.doc"
```

###### <

Parameters with `<` values are substituted with data taken from `STDIN`.
As raw value can only be URL so in this case data received from `STDIN` should be URL or URL array separated by `\n`
_Example:_ 

```shell
--params="file:<"
```

###### @<

Parameters with `@<` values are substituted with data taken from `STDIN`.
Data received from `STDIN` should be local path to file or directory. 
Also can be an array of paths separated by `\n`.
_Example:_ 

```shell
--params="file:@<"
```

###### <<

Parameters with `<<` values are substituted with data taken from `STDIN`.
Data received from `STDIN` should be file content. 
_Example:_ 

```shell
--params="file:<<"
```

##### Array parameter

If parameter name is suffixed with `[]` parameter gets treated as an array parameter.
Mainly array parameters are used when one conversion needs to accept multiple files (e.g. pdf merge or zip compression).

```shell
--params="files[]:@/path/to/dir"
```

#### --out

Argument defines how conversion result should be outputted.
Values can be one of those:

##### url

This is default output method. It prints to `STDOUT` URL or array of URLs that point to converted files.
This method should be used for conversion chaining.

##### @

Value prefixed with `@` is treated as local file or directory path where converted file will be stored.   
_Example:_ 

```shell
--out="@/path/to/result.pdf"
```
or
```shell
--out="@/path/to/resultdir"
```

##### stdout

Conversion result will be outputted to `STDOUT`.
If result consists of multiple files, second and other files will be outputted to file descriptors starting from 3.

```shell
--out="stdout"
```

#### --secret

ConvertAPI user secret.
Get your secret at https://www.convertapi.com/a

#### --version

Outputs CLI utility version information and exits.

#### --help

Displays short usage information.


### Examples

Convert local DOCX file to PDF A3 page size saving result to `/path/to/resultdir`
```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx, pagesize:a3" --out="@/path/to/resultdir" --secret=<YOUR_SECRET_HERE>
```

Merge all PDF files that are in `/path/to/dir` directory and save it locally
```shell
convertapi --iformat=pdf --oformat=merge --params="files[]:@/path/to/dir" --out="@/path/to/resultdir" --secret=<YOUR_SECRET_HERE>
```

Convert remote PPTX file to PDF saving result to `/path/to/result.pdf`
```shell
convertapi --iformat=pptx --oformat=pdf --params="file:https://example.com/myfile.pptx" --out="@/path/to/result.pdf" --secret=<YOUR_SECRET_HERE>
```

Convert from DOCX to JPG and ZIP result JPG files
```shell
convertapi --iformat=docx --oformat=jpg --params="file:@/path/to/test.docx" --secret=<YOUR_SECRET_HERE> \
    | convertapi --iformat=jpg --oformat=zip --params="files[]:<" --out="@/path/to/result.zip" --secret=<YOUR_SECRET_HERE>
```

Convert DOCX to PDF and save result in remote server over SSH
```shell
convertapi --iformat=docx --oformat=pdf --params="file:@/path/to/test.docx" --out=stdout --secret=<YOUR_SECRET_HERE> \
    | ssh user@myserver "cat >/tmp/my.pdf"
```

Get PDF file from remote server, convert it to JPG and save result locally
```shell
ssh user@server "cat /tmp/my.pdf" \
    | convertapi --iformat=pdf --oformat=jpg --params="file:<<" --out=@/path/to/resultdir --secret=<YOUR_SECRET_HERE>
```

Do PDF->JPG and DOCX->JPG conversions in parallel and ZIP result JPG files
```shell
( \
    convertapi --iformat=pdf --oformat=jpg --params="file:/path/to/dir" --secret=<YOUR_SECRET_HERE> \
    & convertapi --iformat=docx --oformat=jpg --params="file:@/path/to/dir" --secret=<YOUR_SECRET_HERE> \
) | convertapi --iformat=jpg --oformat=zip --params="files[]:<" --out=@/path/to/resultdir  --secret=<YOUR_SECRET_HERE>
```

Merge PDFs files from various locations: remote SSH server, local file, local directory, remote HTTP server.
Save result file to remote SSH server. All this done without writing to disk. 
```shell
ssh user@server1 "cat /tmp/my.pdf" \
    | convertapi --iformat=pdf --oformat=merge --params="files[]:<<;@/path/to/test.pdf;@/path/to/dir;https://example.com/my.pdf" --out=stdout --secret=<YOUR_SECRET_HERE> \
    | ssh user@myserver2 "cat >/tmp/my.pdf"
```

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI CLI is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license.
Refer to the [LICENSE](https://github.com/ConvertAPI/convertapi-cli/blob/master/LICENSE) file for more information.