output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "container_app_environment_domain" {
  value = azurerm_container_app_environment.main.default_domain
}

output "api_app_name" {
  value = azurerm_container_app.api.name
}

output "web_app_name" {
  value = azurerm_container_app.web.name
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "redis_hostname" {
  value = azurerm_redis_cache.main.hostname
}

output "storage_account_name" {
  value = azurerm_storage_account.main.name
}
