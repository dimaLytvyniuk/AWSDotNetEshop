# Task 1 (Cloud Introduction)

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).

## Tasks

---

### Task 1.1

1. Join our communication channel

- [AWS for .Net](https://teams.microsoft.com/l/channel/todotodo)

### Task 1.2

1. Install .net SDK version 7 or later
2. Update to VS 17.6.0 or later
3. Install Node.js https://nodejs.org/en/download
4. Run application locally
    - Clone the [selected repo] (https://git.epam.com/oleksandr_dmytriienko/awsdotneteshop)
    - Install dependencies
    - Rubuild solution
    - Set as Start-up project 'docker-compose'
    - Run the app 
    - Check that site running and you can see catalog of items, make login and add item to the basket
    - Optinally import postman collection from '\Learn\Optional\AWS donNet.postman_collection.json' and play with requests in 'Health Check Module 2' folder

### Task 1.3

1. Register in AWS following best practices.

    - Do be careful with your root credentials and make sure to protect it properly.

2. Create an IAM user and assign AdministratorAccess policy to it.
3. Using CLI, connect to your AWS account and get the created IAM user information

    - Do remember about AWS credentials setup on your local machine
    - Command example that needs to work from your terminal: `aws iam get-user --user-name=MyUser`
4. 	Create AWS credentials for using AWS CLI in Github actions
5.  Add to Git secrets aws keys 


