# Task 2 (Containerization)

## Prerequisites

---

- [Docker](https://docs.docker.com/get-docker/) must be installed

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).

## Tasks

---

### Task 8.1

1. Create Build job for application in Github actions

### Task 8.2

1. Create and build Docker image
2. Create/Setup ECR
3. Optionally. Manually Tag and push Docker image into ECR repository
4. Deploy basic implementation to your AWS account using 
5. Manually check that services runnung correctly
   - by checking services logs
   - by making postman requests to application gateway (modify url request in folder 'Health Check Module 2' to reference your application gateway in AWS)
6. Verify using cloud mentor pipeline that you deployed services correctly
   - Fill in "Onboarding Form" in [Cloud-Mentor Quick Start](https://learn.epam.com/detailsPage?id=39ec362e-859e-4ffe-a56f-4e7a6e647bad)
   - In order to verify tasks use new pipeline multi_stage_pipeline

### Task 8.3

1. Optional. Automate steps in Gihub actions optional