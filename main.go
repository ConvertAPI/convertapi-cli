package main

import (
	"errors"
	"flag"
	"fmt"
	"github.com/ConvertAPI/convertapi-go/config"
	"os"
)

const Version = 3
const Name = "convertapi"
const HelpFlagName = "help"

func main() {
	iFormatF := flag.String("iformat", "", "input format e.g. docx, pdf, jpg")
	oFormatF := flag.String("oformat", "", "output format e.g. pdf, jpg, zip")
	paramsF := flag.String("params", "", "conversion parameters, see full list of available parameters at https://www.convertapi.com .  Allowed values: [ value | @<path> | @< | < | << ]. Usage example: --params=\"file:@/path/to/file.doc, pdftitle:My title\"")
	outF := flag.String("out", "url", "place where to output converted files. Allowed values: [ url | @<path> | stdout ]. Save to directory example: --out=\"@/path/to/dir\" ")
	secretF := flag.String("secret", "", "ConvertAPI user secret. Get your secret at https://www.convertapi.com/a")
	tokenF := flag.String("token", "", "ConvertAPI user token. Get your secret at https://www.convertapi.com/a/auth")
	apikeyF := flag.String("apikey", "", "ConvertAPI user apikey. Get your secret at https://www.convertapi.com/a/auth")
	verF := flag.Bool("version", false, "output version information and exit")
	helpF := flag.Bool(HelpFlagName, false, "display this help and exit")
	flag.Parse()

	if *verF {
		printVersion()
	}
	if *helpF {
		printHelp()
	}
	if *iFormatF == "" {
		printError(errors.New("Input format is not set. Please set --inf"), 1)
	}
	if *oFormatF == "" {
		printError(errors.New("Output format is not set. Please set --outf"), 1)
	}

	if *secretF == "" {
		if *tokenF == "" {
			printError(errors.New("ConvertAPI user secret is not set. Please set --secret or --token parameter. Get your secret at https://www.convertapi.com/a"), 1)
		} else {
			config.Default.Token = *tokenF
			config.Default.ApiKey = *apikeyF
		}
	} else {
		config.Default.Secret = *secretF
	}

	if *paramsF == "" {
		printError(errors.New("Conversion parameters are not set. Please set --params parameter."), 1)
	} else {
		if paramsets, err := parseParams(*paramsF, *iFormatF); err == nil {
			convert(*iFormatF, *oFormatF, paramsets, *outF)
		} else {
			printError(fmt.Errorf("Conversion parameters are invalid. %s", err), 1)
		}
	}
}

func printError(err error, exitCode int) {
	fmt.Fprintf(os.Stderr, "%s: %s\n", Name, err)
	fmt.Fprintf(os.Stderr, "Try '%s --%s' for more information.\n", Name, HelpFlagName)
	os.Exit(exitCode)
}

func debug(d ...interface{}) {
	fmt.Fprintf(os.Stderr, "%+v\n", d)
}

func printHelp() {
	flag.PrintDefaults()
	fmt.Println("\nFull documentation can be found at https://github.com/ConvertAPI/convertapi-cli\n")
	os.Exit(0)
}

func printVersion() {
	fmt.Printf("%s %d\n", Name, Version)
	os.Exit(0)
}
