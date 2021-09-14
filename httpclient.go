package main

import (
	"fmt"
	"net/http"
	"runtime"
	"strings"
)

type transport struct {
	http.RoundTripper
}

func (this *transport) RoundTrip(req *http.Request) (*http.Response, error) {
	agent := fmt.Sprintf("ConvertAPI-CLI/%d (%s)", Version, strings.Title(runtime.GOOS))
	req.Header.Add("User-Agent", agent)
	return this.RoundTripper.RoundTrip(req)
}

func newHttpClient() *http.Client {
	return &http.Client{Transport: &transport{http.DefaultTransport}}
}
