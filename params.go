package main

import (
	"bufio"
	"errors"
	"fmt"
	"github.com/ConvertAPI/convertapi-go/param"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

func parseParams(paramString string, ext string) (paramsets [][]param.IParam, err error) {
	var newParams []param.IParam
	var parallel bool
	const esc = "|escapedcomma|"
	var escParamStr = strings.ReplaceAll(paramString, "\\,", esc)
	for _, p := range strings.Split(escParamStr, ",") {
		kv := strings.SplitN(p, ":", 2)
		k := strings.TrimSpace(kv[0])
		v := strings.ReplaceAll(strings.TrimSpace(kv[1]), esc, ",")
		if newParams, parallel, err = newCaParams(k, v, ext); err != nil {
			return
		}
		paramsets = mergeParams(paramsets, newParams, parallel)
	}
	return
}

func mergeParams(paramsets [][]param.IParam, params []param.IParam, parallel bool) (res [][]param.IParam) {
	if params == nil || len(params) == 0 {
		return paramsets
	}
	if parallel {
		for _, p := range params {
			if paramsets == nil || len(paramsets) == 0 {
				res = append(res, []param.IParam{p})
			}
			for _, set := range paramsets {
				mergedSet := append(set, p)
				res = append(res, mergedSet)
			}
		}
	} else {
		if paramsets == nil || len(paramsets) == 0 {
			return [][]param.IParam{params}
		}
		for i, set := range paramsets {
			res[i] = append(set, params...)
		}
	}
	return
}

func newCaParams(key string, value string, ext string) (caParams []param.IParam, parallel bool, err error) {
	parallel = !strings.HasSuffix(key, "[]")
	if !parallel {
		key = strings.TrimSuffix(key, "[]")
	}

	var i int
	for _, v := range strings.Split(value, ";") {
		vals := []string{strings.TrimSpace(v)}
		var paramType string
		if strings.HasPrefix(v, "@") {
			paramType = "path"
			v = strings.TrimSpace(strings.TrimPrefix(v, "@"))
			paths := []string{v}
			if strings.HasPrefix(v, "<") {
				paths, err = stdinLines()
			}
			if vals, err = flattenPaths(paths, ext); err != nil {
				return
			}
		} else if strings.HasPrefix(v, "<<") {
			paramType = "stdin"
		} else {
			paramType = "str"
			if strings.HasPrefix(v, "<") {
				vals, err = stdinLines()
			}
		}

		for _, val := range vals {
			name := key
			if !parallel {
				name = fmt.Sprintf("%s[%d]", key, i)
				i += 1
			}
			switch paramType {
			case "path":
				caParams = append(caParams, param.NewPath(name, val, nil))
			case "stdin":
				caParams = append(caParams, param.NewReader(key, os.Stdin, "file."+ext, nil))
			default:
				caParams = append(caParams, param.NewString(name, val))
			}
		}
	}

	return
}

func stdinLines() (lines []string, err error) {
	s := bufio.NewScanner(os.Stdin)
	for s.Scan() {
		lines = append(lines, s.Text())
	}
	return
}

func flattenPaths(paths []string, ext string) (res []string, err error) {
	flat := []string{}
	for _, p := range paths {
		if flat, err = dirToFiles(p, ext); err != nil {
			return
		}
		res = append(res, flat...)
	}
	return
}

func dirToFiles(path string, ext string) (paths []string, err error) {
	dir, err := isDir(path)
	if err == nil {
		if dir {
			paths = []string{}
			wildcardPath := filepath.Join(path, "*."+ext)
			files, err := filepath.Glob(wildcardPath)
			if err == nil {
				for _, f := range files {
					paths = append(paths, f)
				}
			}
			sort.Strings(paths)
		} else {
			if strings.EqualFold(filepath.Ext(path), "."+ext) {
				paths = []string{path}
			} else {
				err = errors.New(fmt.Sprintf("File %s is not %s format.", path, ext))
			}
		}
	}
	return
}

func isDir(path string) (isDir bool, err error) {
	info, err := os.Stat(path)
	if err == nil {
		isDir = info.IsDir()
	}
	return
}

func prepare(paramset []param.IParam) error {
	var wait []chan error
	for i, p := range paramset {
		wait = append(wait, make(chan error))
		go func(c chan error, p param.IParam) { c <- p.Prepare() }(wait[i], p)
	}

	for _, c := range wait {
		if err := <-c; err != nil {
			return err
		}
	}

	return nil
}
