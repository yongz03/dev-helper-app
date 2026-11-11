# Project instructions

## Stack
- Backend: ASP.NET Core Web API, C#, EF Core, xUnit
- Frontend: Angular, TypeScript strict mode, RxJS, Angular Material
- Package manager: npm
- OS: Windows

## Rules
- Follow existing naming, folder, and API conventions.
- Do not modify unrelated files.
- Use strongly typed TypeScript interfaces; avoid `any`.
- Add or update unit tests for changed backend and frontend logic.
- Run these checks before finishing:
  - dotnet build
  - dotnet test
  - npm ci
  - npm run lint
  - npm run build
- Do not commit secrets, API keys, connection strings, or generated files.