# resource "aws_dynamodb_table" "StakeholderRegTable" {
#   name           = var.table_name
#   hash_key       = "Id"
#   range_key      = "CreatedTimestampUTC"
#   billing_mode   = "PAY_PER_REQUEST"
#   read_capacity  = 2
#   write_capacity = 2


#   attribute {
#     name = "Id"
#     type = "S"
#   }

#   attribute {
#     name = "CreatedTimestampUTC"
#     type = "S"
#   }
# }
