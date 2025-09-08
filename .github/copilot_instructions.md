# GitHub Copilot Instructions for Reactive.Wasm

## 🚨 VITAL PREREQUISITES - READ FIRST 🚨

**BEFORE MAKING ANY CODE CHANGES OR BUILD ATTEMPTS**, you MUST ensure these requirements are met. Failure to do so will result in redundant actions and wasted time.

### 1\. .NET SDK Requirement

This project uses .NET Standard and .NET for its test projects. It is recommended to use the **.NET 8 SDK or later** to ensure all tools and build processes work correctly.

**Before any build or development work:**

```bash
# Check current .NET version
dotnet --version

# If you don't have .NET 8 or later installed:
# Download from: https://dotnet.microsoft.com/download
```

**Why this is VITAL:**

  - The test project targets `net8`.
  - The main library targets `netstandard2.0`, which is compatible with a wide range of .NET versions, but the build tooling benefits from a modern SDK.
  - Build will fail immediately without a proper SDK version.

### 2\. Non-Shallow Clone Requirement - MANDATORY

You MUST work with a full repository clone, not a shallow one. This is essential for the build system, which uses Nerdbank.GitVersioning, to function properly.

**If working with a shallow clone, fix it immediately:**

```bash
# Convert shallow clone to full clone
git fetch --unshallow

# Verify you have full history
git log --oneline | wc -l  # Should show more than just recent commits
```

**Why this is VITAL:**

  - Version generation and build scripts rely on the full commit history.
  - Shallow clones will cause build failures and inconsistent versioning behavior.

## Development Workflow

### Initial Setup (MANDATORY FIRST STEPS)

1.  **Verify .NET SDK installation** (.NET 8 or later).
2.  **Ensure non-shallow repository clone**.
3.  **Only then proceed with development tasks**.

### Build and Test

```bash
# Build the main solution (from repository root)
dotnet restore src/System.Reactive.Wasm.sln
dotnet build src/System.Reactive.Wasm.sln

# Run tests
dotnet test src/System.Reactive.Wasm.sln
```

### Platform-Specific Notes

The project supports the following target frameworks:

  - `netstandard2.0` (main library)
  - `net8` (for unit tests)

This ensures the library has wide compatibility while allowing tests to use modern .NET features.

## Validation and Quality Assurance

### Code Style and Analysis Enforcement

  - **EditorConfig Compliance**: Repository uses a comprehensive `.editorconfig` with detailed rules for C\# formatting, naming conventions, and code analysis.
  - **StyleCop Analyzers**: Enforces consistent C\# code style with `stylecop.analyzers`.
  - **Roslynator Analyzers**: Additional code quality rules with `Roslynator.Analyzers`.
  - **Analysis Level**: Set to `latest` with enhanced .NET analyzers enabled.
  - **CRITICAL**: All code must comply with ReactiveUI contribution guidelines: [https://www.reactiveui.net/contribute/index.html](https://www.reactiveui.net/contribute/index.html).

## C\# Style Guide

**General Rule**: Follow "Visual Studio defaults" with the following specific requirements:

### Brace Style

  - **Allman style braces**: Each brace begins on a new line.
  - **Single line statement blocks**: Can go without braces but must be properly indented on its own line and not nested in other statement blocks that use braces.
  - **Exception**: A `using` statement is permitted to be nested within another `using` statement by starting on the following line at the same indentation level, even if the nested `using` contains a controlled block.

### Indentation and Spacing

  - **Indentation**: Four spaces (no tabs).
  - **Spurious free spaces**: Avoid, e.g., `if (someVar == 0)...` where dots mark spurious spaces.
  - **Empty lines**: Avoid more than one empty line at any time between members of a type.
  - **Labels**: Indent one less than the current indentation (for `goto` statements).

### Field and Property Naming

  - **Internal and private fields**: Use `_camelCase` prefix with `readonly` where possible.
  - **Static fields**: `readonly` should come after `static` (e.g., `static readonly` not `readonly static`). `s_` prefix should be used for private/internal static fields.
  - **Public fields**: Use PascalCasing with no prefix (use sparingly).
  - **Constants**: Use PascalCasing for all constant local variables and fields (except interop code, where names and values must match the interop code exactly).
  - **Fields placement**: Specify fields at the top within type declarations.

### Visibility and Modifiers

  - **Always specify visibility**: Even if it's the default (e.g., `private string _foo` not `string _foo`).
  - **Visibility first**: Should be the first modifier (e.g., `public abstract` not `abstract public`).
  - **Modifier order**: `public`, `private`, `protected`, `internal`, `static`, `extern`, `new`, `virtual`, `abstract`, `sealed`, `override`, `readonly`, `unsafe`, `volatile`, `async`.

### Namespace and Using Statements

  - **Namespace imports**: At the top of the file, outside of `namespace` declarations.
  - **Sorting**: System namespaces alphabetically first, then third-party namespaces alphabetically.
  - **Global using directives**: Use where appropriate to reduce repetition across files.
  - **Placement**: Use `using` directives outside `namespace` declarations.

### Type Usage and Variables

  - **Language keywords**: Use instead of BCL types (e.g., `int`, `string`, `float` instead of `Int32`, `String`, `Single`) for type references and method calls (e.g., `int.Parse` instead of `Int32.Parse`).
  - **var usage**: Encouraged for large return types or refactoring scenarios; use full type names for clarity when needed.
  - **this. avoidance**: Avoid `this.` unless absolutely necessary.
  - **nameof(...)**: Use instead of string literals whenever possible and relevant.

### Code Patterns and Features

  - **Method groups**: Use where appropriate.
  - **Pattern matching**: Use modern C\# pattern matching for expressive conditional logic.
  - **Inline out variables**: Use the C\# inline variable feature with `out` parameters.
  - **Non-ASCII characters**: Use Unicode escape sequences (`\uXXXX`) instead of literal characters to avoid garbling by tools or editors.
  - **Modern C\# features (C\# 8+)**:
      - Enable nullable reference types to reduce null-related errors.
      - Use ranges (`..`) and indices (`^`) for concise collection slicing.
      - Employ `using` declarations for automatic resource disposal.
      - Declare static local functions to avoid state capture.
      - Prefer switch expressions over statements for concise control flow.
      - Use records and record structs for data-centric types with value semantics.
      - Apply init-only setters for immutable properties.
      - Utilize target-typed `new` expressions to reduce verbosity.
      - Declare static anonymous functions or lambdas to prevent state capture.
      - Use file-scoped namespace declarations for concise syntax.
      - Apply `with` expressions for nondestructive mutation.
      - Use raw string literals (`"""`) for multi-line or complex strings.
      - Mark required members with the `required` modifier.
      - Use primary constructors to centralize initialization logic.
      - Employ collection expressions (`[...]`) for concise array/list/span initialization.
      - Add default parameters to lambda expressions to reduce overloads.

### Documentation Requirements

  - **XML comments**: All publicly exposed methods and properties must have .NET XML comments, including protected methods of public classes. The project is configured to generate a documentation file.
  - **Documentation culture**: Use `en-US` as specified in `src/stylecop.json`.

### File Style Precedence

  - **Existing style**: If a file differs from these guidelines (e.g., private members named `m_member` instead of `_member`), the existing style in that file takes precedence.
  - **Consistency**: Maintain consistency within individual files.

### Notes

  - **EditorConfig**: The `.editorconfig` at the root of the repository enforces formatting and analysis rules.
  - **Example Updates**: The example incorporates modern C\# practices like file-scoped namespaces and nullable reference types. Refer to Microsoft documentation for further integration of modern C\# features.

### Code Formatting (Fast - Always Run)

  - **ALWAYS** run formatting before committing:
    ```bash
    cd src
    dotnet format whitespace --verify-no-changes
    dotnet format style --verify-no-changes
    ```
    Time: **2-5 seconds per command**.

### Code Analysis Validation

  - **Run analyzers** to check StyleCop and code quality compliance:
    ```bash
    cd src
    dotnet build --configuration Release --verbosity normal
    ```
    This runs all analyzers (StyleCop SA\*, Roslynator RCS\*, .NET CA\*) and treats warnings as errors.
  - **Analyzer Configuration**:
      - StyleCop settings in `src/stylecop.json`.
      - EditorConfig rules in `.editorconfig` (root level).
      - Analyzer packages in `src/Directory.Build.props`.
      - All code must follow the **ReactiveUI C\# Style Guide** detailed above.

## Common Issues and Solutions

### Build failures with version/history errors

  - **Solution**: Ensure you have a full clone (`git fetch --unshallow`).
  - **Do NOT**: Attempt workarounds with shallow clones. This is required for the versioning tool to work correctly.

### Build Issues

  - **Solution**: Ensure you have a compatible .NET SDK installed (.NET 8 or newer).
  - **Note**: The combination of a modern SDK and a full Git clone is required for the build system to function correctly.
