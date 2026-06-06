# Terraform — Azure-ready example

This configuration describes an example Azure deployment of Accessly:

- a resource group and Log Analytics workspace;
- an Azure Container Apps environment hosting the API, worker and web containers;
- an Azure SQL server and database;
- an Azure Cache for Redis instance;
- a storage account.

> **This is an example only.** CI runs `terraform fmt`, `validate` and `plan` on changes,
> but **never** `apply`. Review and adapt the configuration before any real deployment.

## Usage

```bash
cd infra/terraform

# Secrets are supplied via environment variables — never committed.
export TF_VAR_sql_admin_password='<a strong password>'

cp terraform.tfvars.example terraform.tfvars   # then edit values (e.g. a unique storage name)

terraform init
terraform validate
terraform plan      # review the plan
# terraform apply   # only when you intend to provision real resources
```

## Inputs

| Variable | Description | Default |
| --- | --- | --- |
| `prefix` | Resource name prefix | `accessly` |
| `location` | Azure region | `westeurope` |
| `sql_admin_login` | SQL administrator login | `accessly_admin` |
| `sql_admin_password` | SQL administrator password (sensitive, via `TF_VAR_…`) | — |
| `storage_account_name` | Globally unique storage account name | — |
| `api_image` / `worker_image` / `web_image` | Container images | GHCR placeholders |

## Outputs

Resource group name, Container Apps environment domain, app names, SQL FQDN, Redis hostname
and storage account name. See `outputs.tf`.

State and credentials are never stored in this repository.
