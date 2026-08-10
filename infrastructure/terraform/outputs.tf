output "deployment_name" {
  description = "Stable generated deployment name for downstream modules."
  value       = local.deployment_name
}

output "common_tags" {
  description = "Required governance tags for provider-specific modules."
  value       = local.common_tags
}
