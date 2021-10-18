terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.62"
    }
  }
  backend "s3" {
    bucket = "jwterraformstate"
    key    = "dev_state"
    region = "eu-west-1"
  }

  required_version = ">= 0.14.9"
}

provider "aws" {
  profile = "default"
  region  = "eu-west-1"
}

data "aws_caller_identity" "current_id" {}
data "aws_region" "current_region" {}
data "aws_partition" "current" {}

locals {
  account_id     = data.aws_caller_identity.current_id.account_id
  region_name    = data.aws_region.current_region.name
  partition_name = data.aws_partition.current.partition
}