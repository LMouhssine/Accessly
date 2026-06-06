variable "prefix" {
  description = "Short prefix for resource names."
  type        = string
  default     = "accessly"
}

variable "location" {
  description = "Azure region."
  type        = string
  default     = "westeurope"
}

variable "sql_admin_login" {
  description = "Administrator login for the SQL server."
  type        = string
  default     = "accessly_admin"
}

variable "sql_admin_password" {
  description = "Administrator password for the SQL server (supply via TF_VAR_sql_admin_password)."
  type        = string
  sensitive   = true
}

variable "storage_account_name" {
  description = "Globally unique storage account name (3-24 lowercase alphanumeric chars)."
  type        = string
}

variable "api_image" {
  description = "Container image for the API."
  type        = string
  default     = "ghcr.io/lmouhssine/accessly-api:latest"
}

variable "worker_image" {
  description = "Container image for the worker."
  type        = string
  default     = "ghcr.io/lmouhssine/accessly-worker:latest"
}

variable "web_image" {
  description = "Container image for the web app."
  type        = string
  default     = "ghcr.io/lmouhssine/accessly-web:latest"
}

variable "tags" {
  description = "Tags applied to all resources."
  type        = map(string)
  default = {
    project     = "accessly"
    environment = "example"
  }
}
