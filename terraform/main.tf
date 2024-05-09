variable "azure_credentials" {
  type = object({
    client_id       = string
    client_secret   = string
    tenant_id       = string
    subscription_id = string
  })
  sensitive = true
}

locals {
  app_zip_file = "../out/tic-tac-toe.zip"
  rg_name = "InterviewDemos"
  plan_name = "plan-demos"
  app_name = "yet-another-tic-tac-toe"
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
    }
  }
}

provider "azurerm" {
  features {}

  client_id       = var.azure_credentials.client_id
  client_secret   = var.azure_credentials.client_secret
  tenant_id       = var.azure_credentials.tenant_id
  subscription_id = var.azure_credentials.subscription_id
}

data "azurerm_resource_group" "main" {
  name = local.rg_name
}

data "azurerm_service_plan" "main" {
  name                = local.plan_name
  resource_group_name = data.azurerm_resource_group.main.name
}

resource "azurerm_windows_web_app" "app1" {
  name                = local.app_name
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_service_plan.main.location
  service_plan_id     = data.azurerm_service_plan.main.id
  
  ftp_publish_basic_authentication_enabled = true
  https_only = true
  #zip_deploy_file = local.app_zip_file
  
  app_settings = {
    WEBSITE_RUN_FROM_PACKAGE = "1"
    ASPNETCORE_ENVIRONMENT = "Production"
    ASPNETCORE_WEBROOT = "wwwroot/browser"
  }
    
  site_config {
    always_on = false
    ftps_state = "FtpsOnly"
    http2_enabled = true
    websockets_enabled = true
    
    application_stack {
      current_stack = "dotnet"
      dotnet_version = "v8.0"
    }
    
    virtual_application {
      physical_path = "site\\wwwroot"
      preload = false
      virtual_path = "/"
    }
  }
  
  #lifecycle {
  #  replace_triggered_by = [terraform_data.app_package_hash]
  #}
}

#resource "terraform_data" "app_package_hash" {
#  input = filesha256(local.app_zip_file)
#}
