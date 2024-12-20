# ConvertAPI CLI Client

## Convert your files with our command-line file conversion utility

ConvertAPI helps in converting various file formats. Creating PDF and Images from various sources like Word, Excel, Powerpoint, images, web pages or raw HTML codes. Merge, Encrypt, Split, Repair and Decrypt PDF files and many other file manipulations. You can integrate it into your application in just a few minutes and use it easily.

The ConvertAPI CLI makes it easier to use the Convert API from your shell without having to build your own HTTP calls.
You can get your free secret at https://www.convertapi.com/a

## Installation

Download compressed CLI executable

* Linux: [convertapi_lin.tar.gz](https://github.com/ConvertAPI/convertapi-cli/releases/download/v3/convertapi_lin.tar.gz)
* Linux ARM64: [convertapi_lin_arm.tar.gz](https://github.com/ConvertAPI/convertapi-cli/releases/download/v3/convertapi_lin_arm.tar.gz)
* Darwin (MacOS): [convertapi_mac.tar.gz](https://github.com/ConvertAPI/convertapi-cli/releases/download/v3/convertapi_mac.tar.gz)
* Darwin (MacOS) M1: [convertapi_mac_arm.tar.gz](https://github.com/ConvertAPI/convertapi-cli/releases/download/v3/convertapi_mac_arm.tar.gz)
* Windows: [convertapi_win.zip](https://github.com/ConvertAPI/convertapi-cli/releases/download/v3/convertapi_win.zip)

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

#### Example for windows `.exe`:
```shell
convertapi-cli.exe <api-token> <output-directory> <input-files...> [from-format] [to-format] [key1=value1 key2=value2 ...]
```

### Arguments documentation

#### executable
_Example:_

```shell
convertapi-cli.exe
```

#### api-token (Authentication)
Any of these authentication mechanisms can be used as `api-token`:
- ConvertAPI user**secret**: https://www.convertapi.com/a
- **Access token**: [ConvertAPI dashboard](https://www.convertapi.com/a/access-tokens).
- **JWT token**: [ConvertAPI dashboard](https://www.convertapi.com/a/jwt-tokens).

_Example:_

```shell
secret_asdaSERTervcxsFWtt
```

#### output-directory
Directory where converted file / files needs to be saved in your system.
_Example:_

```shell
<directory-in-your-file-system>
```

##### input-files parameter
The full file path in your file system. If the conversion supports multiple input files, separate their full paths with spaces (' ').
```shell
<full-path1> <full-path2> <full-path3>
```


##### from-format
Find specific formats from all conversions here: https://www.convertapi.com/api. When you open a conversion, the **From** (Source) and **To** (Destination) formats are visible in the browser's URL and in the API Request panel, which displays the HTTP POST request URL.
_Example:_
- Docx to Html conversion: https://www.convertapi.com/a/api/docx-to-html. **From-format** is: `docx`.
- Watermark PDF conversion: https://www.convertapi.com/a/api/pdf-to-watermark. **From-format** is: `pdf`.
- Images to Join conversion: https://www.convertapi.com/a/api/images-to-join. **From-format** is: `images`.

##### to-format
Find specific formats from all conversions here: https://www.convertapi.com/api. When you open a conversion, the **From** (Source) and **To** (Destination) formats are visible in the browser's URL and in the API Request panel, which displays the HTTP POST request URL.

_Example:_
- Docx to Html conversion: https://www.convertapi.com/a/api/docx-to-html. **To-format** is: `html`.
- Watermark PDF conversion: https://www.convertapi.com/a/api/pdf-to-watermark. **To-format** is: `watermark`.
- Images to Join conversion: https://www.convertapi.com/a/api/images-to-join. **To-format** is: `join`.

##### Parameters
All parameters can be found on a specific conversion. Parameters are separated with spaces (' ').

_Example:_
For example, we are doing PDF to JPG conversion: https://www.convertapi.com/a/api/pdf-to-jpg and want to set Pdf password, result file name and result image resolution.
```shell
password=1234 filename=new-wonderful-name ImageResolution=300 
```

## Examples

Convert a single PDF to DOCX:
```shell
convertapi-cli.exe YOUR_API_TOKEN output.docx input.pdf
```

Merge multiple PDF files into one:
```shell
convertapi-cli.exe YOUR_API_TOKEN merged_output.pdf file1.pdf file2.pdf file3.pdf pdf merge
```

Protect a PDF with a password:
```shell
convertapi-cli.exe YOUR_API_TOKEN protected_output.pdf input.pdf pdf protect UserPassword=1234 OwnerPassword=abcd FileName=protected
```

Add a watermark to a PDF:
```shell
convertapi-cli.exe YOUR_API_TOKEN watermarked_output.pdf input.pdf pdf watermark Text=Confidential FileName=watermark
```


### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The ConvertAPI CLI is licensed under the [MIT](https://opensource.org/license/mit "Read more about the MIT license form") license.
Refer to the [LICENSE](https://raw.githubusercontent.com/ConvertAPI/convertapi-cli/master/LICENSE.txt) file for more information.