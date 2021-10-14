resource "aws_iam_role" "role_for_registration_lambdas" {
  name = "role_for_registration_lambdas"

  # Terraform's "jsonencode" function converts a
  # Terraform expression result to valid JSON syntax.
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = [
            "lambda.amazonaws.com"
          ]
        },
      },
    ]
  })

  managed_policy_arns = ["arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"]

  inline_policy {
    name   = "policy_for_registration_lambda_to_invoke"
    policy = data.aws_iam_policy_document.inline_policy_for_registration_lambda_to_invoke.json
  }

  inline_policy {
    name   = "policy_for_registration_lambda_to_access_dynamodb"
    policy = data.aws_iam_policy_document.inline_policy_for_registration_lambda_to_access_dynamodb.json
  }

}
data "aws_iam_policy_document" "inline_policy_for_registration_lambda_to_invoke" {
  statement {
    effect    = "Allow"
    actions   = ["lambda:InvokeFunction"]
    resources = ["*"]
  }
}

data "aws_iam_policy_document" "inline_policy_for_registration_lambda_to_access_dynamodb" {
  statement {
    effect = "Allow"
    actions = ["dynamodb:GetItem",
      "dynamodb:DeleteItem",
      "dynamodb:PutItem",
      "dynamodb:Scan",
      "dynamodb:Query",
      "dynamodb:UpdateItem",
      "dynamodb:BatchWriteItem",
      "dynamodb:BatchGetItem",
      "dynamodb:DescribeTable",
    "dynamodb:ConditionCheckItem"]
    resources = [
      "arn:${local.partition_name}:dynamodb:${local.region_name}:${local.account_id}:table/${var.table_name}",
      "arn:${local.partition_name}:dynamodb:${local.region_name}:${local.account_id}:table/${var.table_name}/index/*"
    ]
  }
}

resource "aws_lambda_function" "get_registrations_for_user" {
  function_name = "get_registrations_for_user"
  s3_bucket     = var.registration_lamabda_code_bucket
  s3_key        = var.registration_lamabda_code_filename
  description   = "Function to get the current users registrations"

  memory_size = 512
  timeout     = 30

  role = aws_iam_role.role_for_registration_lambdas.arn

  handler = "NICE.Registration::NICE.Registration.Functions::GetRegistrationsForUserAsync"
  runtime = "dotnetcore3.1"

  environment {
    variables = {
      RegistrationTableName = var.table_name
    }
  }
}

resource "aws_lambda_function" "add_registration_for_user" {
  function_name = "add_registration_for_user"
  s3_bucket     = var.registration_lamabda_code_bucket
  s3_key        = var.registration_lamabda_code_filename
  description   = "Function to get the current users registrations"

  memory_size = 512
  timeout     = 30

  role = aws_iam_role.role_for_registration_lambdas.arn

  handler = "NICE.Registration::NICE.Registration.Functions::AddRegistrationAsync"
  runtime = "dotnetcore3.1"

  environment {
    variables = {
      RegistrationTableName = var.table_name
    }
  }
}