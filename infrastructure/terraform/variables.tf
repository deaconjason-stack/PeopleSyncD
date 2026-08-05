variable "name_prefix" {
  description = "Prefix applied to PeopleSyncD infrastructure resources."
  type        = string
  default     = "peoplesyncd"
}

variable "environment" {
  description = "Deployment environment name."
  type        = string

  validation {
    condition     = contains(["development", "test", "staging", "production"], var.environment)
    error_message = "Environment must be development, test, staging, or production."
  }
}
