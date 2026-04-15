# dotnet-superclean

A .NET global tool that recursively removes `bin` and `obj` folders from .NET projects and solutions, freeing up disk space and ensuring clean builds.

## Why?

.NET build artifacts accumulate quickly across large solutions. The built-in `dotnet clean` command only cleans projects it knows about — it won't touch orphaned or nested build folders. `dotnet-superclean` deletes every `bin` and `obj` directory it finds inside a .NET project directory.

## Installation

```bash
dotnet tool install --global DotnetSuperClean
```

## Usage

Clean the current directory:

```bash
dotnet superclean
```

Clean a specific path:

```bash
dotnet superclean /path/to/solution
```

Preview what would be deleted without deleting anything:

```bash
dotnet superclean --dry-run
```

Show each folder as it is deleted:

```bash
dotnet superclean --verbose
```

Combine options:

```bash
dotnet superclean /path/to/solution --dry-run --verbose
```

### Options

| Option | Short | Description |
|---|---|---|
| `--dry-run` | `-d` | List folders that would be deleted without deleting them |
| `--verbose` | `-v` | Print each folder path as it is deleted |

## How it works

1. Enumerates all subdirectories under the target path
2. Filters for directories named `bin` or `obj`
3. Skips directories already nested inside another `bin` or `obj` (their parent deletion covers them)
4. Skips directories whose parent does not contain a project file (`*.csproj`, `*.vbproj`, `*.fsproj`, etc.)
5. Deletes from deepest path to shallowest to avoid operating on already-deleted directories
6. Reports a summary of deleted folders and any failures

## Example output

```text
Super clean complete. 15 folder(s) removed.
```

Errors are printed in red, warnings in yellow, and success in green.
