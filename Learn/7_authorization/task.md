# Task 7 (Authorization)

## Prerequisites

---

- The task is a continuation of Homework 6.

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).

## Tasks

---

### Task 7.1

1. Create a lambda function called `basicAuthorizer`.
2. This lambda should have at least one environment variable with the following credentials:

```
  {yours_github_account_login}=TEST_PASSWORD
```

- `{yours_github_account_login}` - your GitHub account name. Login for test user should be your GitHub account name
- `TEST_PASSWORD` - password string. Password for test user must be "TEST_PASSWORD"
- example: `johndoe=TEST_PASSWORD`

3. This `basicAuthorizer` lambda should take _Basic Authorization_ token, decode it and check that credentials provided by token exist in the lambda environment variable.
4. This lambda should return 403 HTTP status if access is denied for this user (invalid `authorization_token`) and 401 HTTP status if Authorization header is not provided.

_NOTE: Do not send your credentials to the GitHub. Use `.env` file and `serverless-dotenv-plugin` serverless plugin to add environment variables to the lambda. Add `.env` file to `.gitignore` file._

```
  .env file example:
    vasiapupkin=TEST_PASSWORD
```

### Task 7.2

1. Add Lambda authorization to the `/import` path of the API Gateway.
2. Use your `basicAuthorizer` lambda as the Lambda authorizer

### Task 7.3

1. Request from the client application to the `/import` path of the Import Service should have _Basic Authorization_ header:

```
  Authorization: Basic {authorization_token}
```

- `{authorization_token}` is a base64-encoded `{yours_github_account_login}:TEST_PASSWORD`
- example: `Authorization: Basic sGLzdRxvZmw0ZXs0UGFzcw==`

2. Client should get `authorization_token` value from browser [localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage)

```
  const authorization_token = localStorage.getItem('authorization_token')
```
