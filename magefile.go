//go:build mage
// +build mage

package main

import (
	"fmt"
	"runtime"

	"github.com/magefile/mage/sh"
)

// Default target to run when none is specified
// If not set, running mage will list available targets
//var Default = Build

// Docker build for local architecture (amd64 or arm64)
func DockerBuild() error {
	goos := runtime.GOOS
	goarch := runtime.GOARCH
	fmt.Printf("building for %s/%s\n", goos, goarch)

	switch {
	case goos == "darwin" && goarch == "arm64":
		return DockerBuildArm()
	case goos == "darwin" && goarch == "amd64":
		return DockerBuildAmd()
	case goos == "linux" && goarch == "arm64":
		return DockerBuildArm()
	case goos == "linux" && goarch == "amd64":
		return DockerBuildAmd()
	default:
		return DockerBuildAmd()
	}

}

// DockerBuildArm builds for amd64 architecture
func DockerBuildArm() error {
	cmd := []string{
		"docker",
		"build",
		"-t",
		"test",
		"-f",
		"linux-arm64.Dockerfile",
		".",
	}
	return sh.RunV(cmd[0], cmd[1:]...)
}

// DockerBuildAmd builds for amd64 architecture
func DockerBuildAmd() error {
	cmd := []string{
		"docker",
		"build",
		"-t",
		"test",
		"-f",
		"linux-amd64.Dockerfile",
		".",
	}
	return sh.RunV(cmd[0], cmd[1:]...)
}

// DockerRun runs the docker image
func DockerRun() error {
	cmd := []string{
		"docker",
		"run",
		"-it",
		"test",
		"hello",
	}
	return sh.RunV(cmd[0], cmd[1:]...)
}
