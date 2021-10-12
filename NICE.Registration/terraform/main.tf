terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.62"
    }
  }

  required_version = ">= 0.14.9"
}

provider "aws" {
  profile = "default"
  region  = "eu-west-1"
}

resource "aws_dynamodb_table" "StakeholderRegTable" {
  name           = var.table_name
  hash_key       = "Id"
  range_key      = "CreatedTimestampUTC"
  billing_mode   = "PAY_PER_REQUEST"
  read_capacity  = 2
  write_capacity = 2


  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CreatedTimestampUTC"
    type = "S"
  }
}

resource "aws_iam_role" "RoleForCustomAuthorizerTF" {
  name = "RoleForCustomAuthorizerTF"

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
            "lambda.amazonaws.com",
            "apigateway.amazonaws.com"
          ]
        },
      },
    ]
  })

  managed_policy_arns = ["arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"]

  inline_policy {
    name   = "PolicyForCustomAuthorizerInline"
    policy = data.aws_iam_policy_document.inline_policy_for_custom_authorizer.json
  }

}

data "aws_iam_policy_document" "inline_policy_for_custom_authorizer" {
  statement {
    effect    = "Allow"
    actions    = ["lambda:InvokeFunction"]
    resources = ["*"]
  }
}


resource "aws_lambda_function" "CustomAuthorizerLambdaTF" {
  function_name = "CustomAuthorizerLambdaTF"
  s3_bucket     = var.custom_authorizer_code_bucket
  s3_key        = var.custom_authorizer_code_filename
  description   = "Custom authorizer lambda function to authenticate REST API Gateway"

  memory_size = 128
  timeout     = 30

  role = aws_iam_role.RoleForCustomAuthorizerTF.arn

  handler = "index.handler"


  runtime = "nodejs14.x"

  environment {
    variables = {
      TOKEN_ISSUER = var.token_issuer,
      AUDIENCE     = var.audience,
      JWKS_URI     = var.jwks_uri
    }
  }
}