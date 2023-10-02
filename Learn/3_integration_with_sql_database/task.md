# Task 3 (Integration with SQL Database)

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).


## Tasks

### Task 3.1

1. Create MySql database Catalog in AWS Aurora using AWS Console
2. Reconfigure services to use this database instead on local My Sql in Docker container
3. Update source code in the application to use My Sql instead of memory storage for 'Release' configuration. (Search '#if DEBUG' in code)
4. Manually check that services runnung correctly
   - by checking services logs
   - by making postman requests to application gateway (modify url request in folder 'Health Check Module 3' to reference your application gateway in AWS)
5. Verify using cloud mentor pipeline that you deployed services correctly
6. Optional. Connect to database instance via a tool called [DBeaver](https://dbeaver.io/download/) or any other similar tools like DataGrip/PgAdmin
7. Optional. Setup instances in multiple regions and configure load balancing
8. Optional. Change deployment scripts to create/configure of My Sql in pipleline.
