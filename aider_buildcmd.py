#!/usr/bin/env python3
"""
Simple build command script for the gpu-image-processing repository.

Running this script will execute `dotnet test` to run all unit tests.
It is intended to be used as a quick way to verify that the project builds
and that the test suite passes.

If the test run fails, the script will exit with the same non‑zero exit
code returned by `dotnet test` and will print the error output to stderr.
"""

import subprocess
import sys

def main() -> None:
    # Execute `dotnet test` in the repository root.
    # The subprocess captures both stdout and stderr so we can display them.
    process = subprocess.run(
        ["dotnet", "test"],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
    )

    # Print the standard output from the test runner.
    print(process.stdout)

    # If the test runner returned a non‑zero exit code, print the error output
    # and exit with the same code so that CI pipelines can detect the failure.
    if process.returncode != 0:
        print(process.stderr, file=sys.stderr)
        sys.exit(process.returncode)


if __name__ == "__main__":
    main()
