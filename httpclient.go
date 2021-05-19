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
	agent := fmt.Sprintf("convertapi-cli%s/%d", strings.Title(runtime.GOOS), Version)
	req.Header.Add("User-Agent", agent)
	return this.RoundTripper.RoundTrip(req)
}

func newHttpClient() *http.Client {
	return &http.Client{Transport: &transport{http.DefaultTransport}}
}
