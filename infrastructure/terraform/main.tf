resource "random_id" "deployment" {
  byte_length = 4
}

locals {
  deployment_name = "${var.name_prefix}-${var.environment}-${random_id.deployment.hex}"
  common_tags = {
    product     = "PeopleSyncD"
    environment = var.environment
    managed_by  = "Terraform"
  }
}
