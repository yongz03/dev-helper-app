# Client

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 18.1.4.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Install and lint

Run `npm ci` to install the exact dependencies from `package-lock.json`, then `npm run lint` to lint TypeScript and Angular templates.

On Windows, stop `ng serve` and any other process using this client's `node_modules` before running `npm ci`. It replaces `node_modules`; an active esbuild process can lock `esbuild.exe` and leave the install incomplete. After a failed install, stop the process and rerun `npm ci` before building.

For local development, start the API with `dotnet run --launch-profile http` from `api/`, then start Angular with `npm start` from `client/`. The Angular dev server proxies `/api/**` to the API at `http://127.0.0.1:5004`.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
