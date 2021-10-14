resource "aws_api_gateway_rest_api" "registration_rest_apigateway" {

  name        = "registration_rest_apigateway"
  description = "Stakeholder registration REST API"

  endpoint_configuration {
    types = ["REGIONAL"]
  }
}

resource "aws_api_gateway_authorizer" "custom_authoriser_for_registration_restapigateway" {
  name                           = "custom_authoriser_for_registration_restapigateway"
  rest_api_id                    = aws_api_gateway_rest_api.registration_rest_apigateway.id
  authorizer_uri                 = aws_lambda_function.CustomAuthorizerLambdaTF.invoke_arn
  authorizer_credentials         = aws_iam_role.RoleForCustomAuthorizerTF.arn
  type                           = "TOKEN"
  identity_source                = "method.request.header.Authorization"
  identity_validation_expression = "^Bearer [-0-9a-zA-Z\\._]*$"
}

resource "aws_lambda_permission" "permission_for_apigateway_to_invoke_get_registration_lambda" {
  statement_id  = "permission_for_apigateway_to_invoke_get_registration_lambda"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.get_registrations_for_user.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "arn:aws:execute-api:${local.region_name}:${local.account_id}:${aws_api_gateway_rest_api.registration_rest_apigateway.id}/*/GET/"
}

resource "aws_lambda_permission" "permission_for_apigateway_to_invoke_post_registration_lambda" {
  statement_id  = "permission_for_apigateway_to_invoke_post_registration_lambda"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.add_registration_for_user.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "arn:aws:execute-api:${local.region_name}:${local.account_id}:${aws_api_gateway_rest_api.registration_rest_apigateway.id}/*/POST/"
}

resource "aws_api_gateway_method" "get_method_for_registration_apigateway" {
  rest_api_id   = aws_api_gateway_rest_api.registration_rest_apigateway.id
  resource_id   = aws_api_gateway_rest_api.registration_rest_apigateway.root_resource_id
  http_method   = "GET"
  authorization = "CUSTOM"
  authorizer_id = aws_api_gateway_authorizer.custom_authoriser_for_registration_restapigateway.id
}

resource "aws_api_gateway_integration" "get_method_integration" {
  rest_api_id             = aws_api_gateway_rest_api.registration_rest_apigateway.id
  resource_id             = aws_api_gateway_rest_api.registration_rest_apigateway.root_resource_id
  http_method             = aws_api_gateway_method.get_method_for_registration_apigateway.http_method
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = aws_lambda_function.get_registrations_for_user.invoke_arn
  passthrough_behavior    = "WHEN_NO_MATCH"
  content_handling        = "CONVERT_TO_TEXT"
  timeout_milliseconds    = 29000
}

resource "aws_api_gateway_method" "post_method_for_registration_apigateway" {
  rest_api_id   = aws_api_gateway_rest_api.registration_rest_apigateway.id
  resource_id   = aws_api_gateway_rest_api.registration_rest_apigateway.root_resource_id
  http_method   = "POST"
  authorization = "CUSTOM"
  authorizer_id = aws_api_gateway_authorizer.custom_authoriser_for_registration_restapigateway.id
}

resource "aws_api_gateway_integration" "post_method_integration" {
  rest_api_id             = aws_api_gateway_rest_api.registration_rest_apigateway.id
  resource_id             = aws_api_gateway_rest_api.registration_rest_apigateway.root_resource_id
  http_method             = aws_api_gateway_method.post_method_for_registration_apigateway.http_method
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = aws_lambda_function.add_registration_for_user.invoke_arn
  passthrough_behavior    = "WHEN_NO_MATCH"
  content_handling        = "CONVERT_TO_TEXT"
  timeout_milliseconds    = 29000
}

resource "aws_api_gateway_deployment" "deployment_of_ref_restapigateway" {
  rest_api_id = aws_api_gateway_rest_api.registration_rest_apigateway.id

  triggers = {
    # NOTE: The configuration below will satisfy ordering considerations,
    #       but not pick up all future REST API changes. More advanced patterns
    #       are possible, such as using the filesha1() function against the
    #       Terraform configuration file(s) or removing the .id references to
    #       calculate a hash against whole resources. Be aware that using whole
    #       resources will show a difference after the initial implementation.
    #       It will stabilize to only change when resources change afterwards.
    redeployment = sha1(jsonencode([
      aws_api_gateway_method.get_method_for_registration_apigateway.id,
      aws_api_gateway_integration.get_method_integration.id,
      aws_api_gateway_method.post_method_for_registration_apigateway.id,
      aws_api_gateway_integration.post_method_integration.id,
    ]))
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_api_gateway_stage" "stage_of_deployment_restapigateway" {
  deployment_id = aws_api_gateway_deployment.deployment_of_ref_restapigateway.id
  rest_api_id   = aws_api_gateway_rest_api.registration_rest_apigateway.id
  stage_name    = var.stage_name
}

resource "aws_api_gateway_method_settings" "rest_api_method_settings" {
  rest_api_id = aws_api_gateway_rest_api.registration_rest_apigateway.id
  stage_name  = aws_api_gateway_stage.stage_of_deployment_restapigateway.stage_name
  method_path = "*/*"

  settings {
    metrics_enabled = true
    logging_level   = "INFO"
  }
}