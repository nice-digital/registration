variable "table_name" {
  description = "Name of the DynamoDB table"
  type        = string
  default     = "StakeholderRegTable"
}

variable "custom_authorizer_code_bucket" {
  description = "Location in S3 of the code to use for the custom authoriser for the API Gateway"
  type        = string
  default     = "jwcodefordevtotest"
}

variable "custom_authorizer_code_filename" {
  description = "Location in S3 of the code to use for the custom authoriser for the API Gateway"
  type        = string
  default     = "IdentityCustomAuthoriser.9+r2D41D48.zip"
}

variable "token_issuer" {
  description = "The domain of the tenant"
  type        = string
  default     = "https://alpha-identity.nice.org.uk/"
}

variable "audience" {
  description = "Audience of the API"
  type        = string
  default     = "https://alpha-identityapi.nice.org.uk/api"
}

variable "jwks_uri" {
  description = "JSON Web Key Set Uri"
  type        = string
  default     = "https://alpha-identity.nice.org.uk/.well-known/jwks.json"
}
