# Security Policy

## Reporting a Vulnerability

**DO NOT open a public GitHub issue for a security vulnerability.** Public disclosure can compromise the security of all users.

Instead, please report security vulnerabilities through one of these channels:

### Option 1: GitHub Private Vulnerability Reporting (Recommended)

Visit: https://github.com/sarmkadan/gpu-image-processing/security/advisories/new

This allows you to report vulnerabilities privately to the maintainers.

### Option 2: Email

Send a detailed report to: **rutova2@gmail.com**

Include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Potential impact
- Suggested remediation (if available)

## Response Timeline

We commit to the following response times for security reports:

- **Acknowledgment**: Within 48 hours of submission
- **Assessment**: Initial assessment within 1 week
- **Resolution**: Depends on severity and complexity
  - Critical: Expedited fix and release
  - High: Patch release within 2 weeks
  - Medium/Low: Included in next regular release

## Supported Versions

| Version | Status | Security Updates |
|---------|--------|------------------|
| v1.x    | Active | ✅ Supported     |

Only the latest major version (v1.x) receives security updates. Users are encouraged to upgrade to the latest version.

## Security Best Practices

When using GPU Image Processing:

1. **Input Validation**: Always validate user-provided file paths and filter parameters
2. **Resource Limits**: Set appropriate batch sizes to prevent memory exhaustion
3. **Error Handling**: Don't expose sensitive error details to users
4. **Updates**: Keep the library and .NET SDK up to date
5. **Dependencies**: Monitor dependencies for known vulnerabilities

## Known Limitations

- Requires a system with OpenCL 1.2+ compatible device
- GPU memory constraints should be considered for large batch operations
- Input files should be validated before processing

## Security Considerations for Contributors

When contributing code:

- Avoid hardcoding secrets or API keys in examples
- Review dependencies for security issues
- Follow secure coding practices for file I/O and memory operations
- Use parameterized queries if database access is involved
- Validate all user input at system boundaries

## Disclosure Policy

Once a vulnerability is fixed and released:

1. The security advisory is published on GitHub
2. Details are included in the release notes
3. Contributors will be credited (if desired)
4. A timeline of disclosure is maintained

## Questions?

For security-related questions, email: rutova2@gmail.com

Thank you for helping keep GPU Image Processing secure!
