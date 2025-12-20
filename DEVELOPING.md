# 🛠️ Developing

A quick setup for contributors working on PoolMaster.

## ⚙️ Prerequisites
- Unity 6.0–6.4 (Built-in, URP, HDRP)
- Git (with hooks enabled)
- CSharpier (code formatter)

## 🔗 Enable Repo Hooks
Run once in this repo to use shared hooks:

```sh
git config core.hooksPath .githooks
```

## 🎨 Formatting (CSharpier)
Check formatting:
```sh
csharpier check .
```
Format the repo:
```sh
csharpier format .
```
Install if missing:
```sh
dotnet tool install -g csharpier
```

Note: A pre-commit hook runs `csharpier` automatically.

## 🧪 Running Tests (Unity Test Runner)
- Open Unity → `Window > Test Runner` (or `Window > General > Test Runner` in newer versions)
- Run EditMode and PlayMode tests in the `Tests/` folder
- Assemblies:
  - `PoolMaster.Tests.asmdef` (EditMode/PlayMode as configured)

## ✅ Commit Guidelines
- Formatting-only: `chore(format): apply CSharpier`
- Docs: `docs: update quick start`
- Features: `feat: add X`
- Fixes: `fix: correct Y`

## 🧭 Useful Paths
- Runtime code: `Runtime/`
- Editor tools: `Editor/`
- Examples: `Examples/`
- Tests: `Tests/`

## 🚀 Quick Start (Dev)
1. Enable hooks: `git config core.hooksPath .githooks`
2. Verify CSharpier: `csharpier --version`
3. Format: `csharpier format .`
4. Open Unity and run tests via Test Runner
